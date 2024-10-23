setlocal

set APP_DIR=D:\Apps\OutputBrowser

dotnet publish -c Release -f net8.0-windows10.0.19041.0 -r win-x64 --self-contained ^
 OutputBrowser\OutputBrowser.csproj ^
 -o %APP_DIR% ^
 /p:Platform=x64 ^
 /p:PublishSingleFile=true ^
 /p:EnableMsixTooling=true ^
 /p:SelfContained=true ^
 /p:WindowsPackageType=None ^
 /p:GenerateDocumentationFile=false ^
 /p:DebugType=embedded

if errorlevel 1 (
    echo Build failed
    exit /b 1
)

start %APP_DIR%

if exist %APP_DIR%\..\OutputBrowser.7z (
    del %APP_DIR%\..\OutputBrowser.7z
)

start 7za a -t7z -m0=lzma -mx=9 -mfb=64 -md=32m -ms=on -mmt=%NUMBER_OF_PROCESSORS% ^
 %APP_DIR%\..\OutputBrowser.7z ^
 %APP_DIR%\OutputBrowser.exe

endlocal
