# AudioDeviceManager

`AudioDeviceManager` is a Windows desktop WPF application built with C# to manage the audio devices currently connected to the machine.

## Features

- List active `Sound Output` devices
- List active `Sound Input` devices
- Show the current default device
- Switch a device to become the Windows default device
- Adjust volume per device
- Mute or unmute each device
- Automatically refresh the device list every 3 seconds

## Tech Stack

- .NET 8
- WPF
- NAudio
- Windows Core Audio API

## Project Structure

```text
AudioDeviceManager/
|- Interop/
|  `- PolicyConfigClient.cs
|- Models/
|  `- AudioDeviceItem.cs
|- Services/
|  `- AudioDeviceService.cs
|- App.xaml
|- MainWindow.xaml
|- MainWindow.xaml.cs
|- AudioDeviceManager.csproj
`- README.md
```

## Requirements

- Windows 10 or Windows 11
- .NET 8 SDK or later to build from source

If you only want to run the self-contained published executable, you do not need to install the .NET runtime separately.

## Open and Run the Project

### Option 1: Run with Visual Studio

1. Open the `AudioDeviceManager.sln` solution
2. Set `AudioDeviceManager` as the startup project
3. Press `F5` to run

### Option 2: Run with the .NET CLI

```powershell
dotnet restore
dotnet build
dotnet run
```

## Build and Publish

### Debug build

```powershell
dotnet build
```

### Publish a self-contained Windows x64 executable

```powershell
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true
```

Published output:

```text
bin/Release/net8.0-windows/win-x64/publish/AudioDeviceManager.exe
```

## How to Use

1. Select a device from the `Sound Output` or `Sound Input` list
2. Drag the volume slider to change its volume
3. Check `Mute` if you want to mute the selected device
4. Click `Set as Default` to make it the Windows default device
5. Click `Refresh` to update the list immediately

## Technical Notes

- The app currently lists only devices in the `Active` state
- When setting the default device, the app applies the change to `Console`, `Multimedia`, and `Communications`
- The default-device switching feature uses COM interop with Windows PolicyConfig
- The application is designed for Windows desktop only and is not intended for macOS or Linux

## Verification

The project has been verified with:

```powershell
dotnet build
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true
```

## Future Improvements

- Show `Disabled` and `Unplugged` devices
- Add clearer device icons and status indicators
- Add a test-audio button for playback devices
- Track system device changes via OS events instead of polling

## Author

This project was created to provide a simple Windows audio device manager built with C#.
