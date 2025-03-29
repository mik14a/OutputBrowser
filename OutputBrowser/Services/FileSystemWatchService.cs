using System;
using System.IO;

namespace OutputBrowser.Services;

public partial class FileSystemWatchService : IDisposable
{
    public event FileSystemEventHandler Created;
    public event FileSystemEventHandler Changed;
    public event FileSystemEventHandler Deleted;
    public event RenamedEventHandler Renamed;
    public event ErrorEventHandler Error;

    public string Name { get; }

    public string Path {
        get => _watcher.Path;
        set => _watcher.Path = value;
    }

    public string Filters {
        get => string.Join(";", _watcher.Filters);
        set {
            _watcher.Filters.Clear();
            foreach (var filter in value.Split(';')) {
                _watcher.Filters.Add(filter);
            }
        }
    }

    public FileSystemWatchService(string name, string path, string filters) {
        Name = name;
        _watcher = new(path) {
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName,
            EnableRaisingEvents = true,
            IncludeSubdirectories = true,
            InternalBufferSize = 32768,
        };
        Filters = filters;
        _watcher.Created += OnCreated;
        _watcher.Changed += OnChanged;
        _watcher.Deleted += OnDeleted;
        _watcher.Renamed += OnRenamed;
        _watcher.Error += OnError;
    }

    public void Dispose() {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing) {
        if (!_disposed) {
            if (disposing) {
                _watcher.Dispose();
            }
            _disposed = true;
        }
    }
    void OnCreated(object sender, FileSystemEventArgs e) {
        Created?.Invoke(this, e);
    }
    void OnChanged(object sender, FileSystemEventArgs e) {
        Changed?.Invoke(this, e);
    }
    void OnDeleted(object sender, FileSystemEventArgs e) {
        Deleted?.Invoke(this, e);
    }
    void OnRenamed(object sender, RenamedEventArgs e) {
        Renamed?.Invoke(this, e);
    }
    void OnError(object sender, ErrorEventArgs e) {
        Error?.Invoke(this, e);
    }

    bool _disposed;

    readonly FileSystemWatcher _watcher;
}
