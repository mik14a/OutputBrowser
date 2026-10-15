using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using OutputBrowser.ViewModels;
using Windows.Graphics;
using WinUIEx;

namespace OutputBrowser.Browser;

[INotifyPropertyChanged]
public sealed partial class ImageBrowserWindow : WindowEx
{
    public ObservableCollection<OutputViewModel> Outputs { get; private set; }
    public int Index { get; private set; } = -1;

    [ObservableProperty]
    public partial OutputViewModel Model { get; private set; }

    public bool CanTop => Outputs != null && 0 < Outputs.Count;
    public bool CanPrevious => Outputs != null && 0 < Index;
    public bool CanNext => Outputs != null && Index < Outputs.Count - 1;
    public bool CanBottom => Outputs != null && Index < Outputs.Count - 1;
    public bool IsAtEnd => Outputs != null && Index == Outputs.Count - 1;

    [ObservableProperty, NotifyPropertyChangedFor(nameof(ShowPromptVisibility))]
    public partial bool ShowPrompt { get; set; } = false;
    [ObservableProperty, NotifyPropertyChangedFor(nameof(ShowNavigationVisibility))]
    public partial bool ShowNavigation { get; set; } = true;

    public Visibility ShowPromptVisibility => ShowPrompt ? Visibility.Visible : Visibility.Collapsed;
    public Visibility ShowNavigationVisibility => ShowNavigation ? Visibility.Visible : Visibility.Collapsed;

    public ImageBrowserWindow() {
        InitializeComponent();
        ExtendsContentIntoTitleBar = true;
        SetTitleBar(_AppTitleBar);
        AppWindow.Resize(new SizeInt32(1024, 768));
        Closed += (_, _) => DetachOutputs();
    }

    public bool ShowAt(ObservableCollection<OutputViewModel> outputs, int index) {
        if (outputs == null) throw new ArgumentNullException(nameof(outputs));

        if (!ReferenceEquals(Outputs, outputs)) {
            DetachOutputs();
            Outputs = outputs;
            Outputs?.CollectionChanged += OnOutputsChanged;
        }

        ShowAt(index);
        return true;
    }

    [RelayCommand(CanExecute = nameof(CanTop))]
    void Top() {
        ShowAt(0);
    }

    [RelayCommand(CanExecute = nameof(CanPrevious))]
    void Previous() {
        ShowAt(Index - 1);
    }

    [RelayCommand(CanExecute = nameof(CanNext))]
    void Next() {
        ShowAt(Index + 1);
    }

    [RelayCommand(CanExecute = nameof(CanBottom))]
    void Bottom() {
        ShowAt(Outputs.Count - 1);
    }

    void OnAppTitleBarContextRequested(UIElement sender, ContextRequestedEventArgs e) {
        e.Handled = true;
    }

    void ShowAt(int index) {
        if (Outputs == null) throw new InvalidOperationException("Outputs is null.");
        if (index < 0 || Outputs.Count <= index) throw new ArgumentOutOfRangeException(nameof(index));

        Index = index;
        Model = Outputs.ElementAt(index);
        Title = Model.DisplayName ?? nameof(OutputBrowser);
        NotifyNavigation();
    }

    void OnOutputsChanged(object sender, NotifyCollectionChangedEventArgs e) {
        Index = Model == null || Outputs == null ? -1 : Outputs.IndexOf(Model);
        NotifyNavigation();
    }

    void DetachOutputs() {
        if (Outputs == null) return;
        Outputs.CollectionChanged -= OnOutputsChanged;
        Outputs = null;
    }

    void NotifyNavigation() {
        TopCommand.NotifyCanExecuteChanged();
        PreviousCommand.NotifyCanExecuteChanged();
        NextCommand.NotifyCanExecuteChanged();
        BottomCommand.NotifyCanExecuteChanged();
    }
}
