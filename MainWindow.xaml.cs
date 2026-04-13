using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

using AudioDeviceManager.Models;
using AudioDeviceManager.Services;

namespace AudioDeviceManager;

public partial class MainWindow : Window
{
    private readonly AudioDeviceService _audioDeviceService = new();
    private readonly DispatcherTimer _refreshTimer;
    private bool _isUpdatingPlaybackControls;
    private bool _isUpdatingRecordingControls;

    public ObservableCollection<AudioDeviceItem> PlaybackDevices { get; } = [];

    public ObservableCollection<AudioDeviceItem> RecordingDevices { get; } = [];

    public MainWindow()
    {
        InitializeComponent();

        PlaybackDevicesListBox.ItemsSource = PlaybackDevices;
        RecordingDevicesListBox.ItemsSource = RecordingDevices;

        _refreshTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(3),
        };
        _refreshTimer.Tick += (_, _) => RunSafely(
            () => RefreshDevices(keepSelections: true),
            "Couldn't refresh the device list.");
        _refreshTimer.Start();

        RunSafely(() => RefreshDevices(), "Couldn't load audio devices.");
    }

    protected override void OnClosed(EventArgs e)
    {
        _refreshTimer.Stop();
        _audioDeviceService.Dispose();
        base.OnClosed(e);
    }

    private void RefreshButton_OnClick(object sender, RoutedEventArgs e)
    {
        RunSafely(() => RefreshDevices(), "Couldn't refresh the device list.");
    }

    private void RefreshDevices(bool keepSelections = false)
    {
        var selectedPlaybackId = keepSelections ? GetSelectedDeviceId(PlaybackDevicesListBox) : null;
        var selectedRecordingId = keepSelections ? GetSelectedDeviceId(RecordingDevicesListBox) : null;

        ReplaceItems(PlaybackDevices, _audioDeviceService.GetPlaybackDevices());
        ReplaceItems(RecordingDevices, _audioDeviceService.GetRecordingDevices());

        PlaybackDevicesListBox.SelectedItem = ResolveSelection(PlaybackDevices, selectedPlaybackId);
        RecordingDevicesListBox.SelectedItem = ResolveSelection(RecordingDevices, selectedRecordingId);

        LastRefreshTextBlock.Text = $"Device list refreshed at {DateTime.Now:HH:mm:ss}.";
    }

    private static string? GetSelectedDeviceId(ListBox listBox)
    {
        return (listBox.SelectedItem as AudioDeviceItem)?.Id;
    }

    private static void ReplaceItems(ObservableCollection<AudioDeviceItem> target, IReadOnlyList<AudioDeviceItem> source)
    {
        target.Clear();
        foreach (var item in source)
        {
            target.Add(item);
        }
    }

    private static AudioDeviceItem? ResolveSelection(IEnumerable<AudioDeviceItem> devices, string? preferredDeviceId)
    {
        return devices.FirstOrDefault(device => device.Id == preferredDeviceId)
               ?? devices.FirstOrDefault(device => device.IsDefault)
               ?? devices.FirstOrDefault();
    }

    private void PlaybackDevicesListBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        UpdateDeviceDetails(
            device: PlaybackDevicesListBox.SelectedItem as AudioDeviceItem,
            selectedNameText: PlaybackSelectedName,
            statusText: PlaybackStatusText,
            volumeText: PlaybackVolumeText,
            volumeSlider: PlaybackVolumeSlider,
            muteCheckBox: PlaybackMuteCheckBox,
            isUpdatingControls: ref _isUpdatingPlaybackControls);
    }

    private void RecordingDevicesListBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        UpdateDeviceDetails(
            device: RecordingDevicesListBox.SelectedItem as AudioDeviceItem,
            selectedNameText: RecordingSelectedName,
            statusText: RecordingStatusText,
            volumeText: RecordingVolumeText,
            volumeSlider: RecordingVolumeSlider,
            muteCheckBox: RecordingMuteCheckBox,
            isUpdatingControls: ref _isUpdatingRecordingControls);
    }

    private static void UpdateDeviceDetails(
        AudioDeviceItem? device,
        TextBlock selectedNameText,
        TextBlock statusText,
        TextBlock volumeText,
        Slider volumeSlider,
        CheckBox muteCheckBox,
        ref bool isUpdatingControls)
    {
        isUpdatingControls = true;

        var volumePercent = device is null ? 0 : Math.Round(device.Volume * 100);

        selectedNameText.Text = device?.Name ?? "No device selected";
        statusText.Text = device is null
            ? "Status: -"
            : $"Status: {device.Status}" + (device.IsDefault ? " | Current default" : string.Empty);
        volumeSlider.Value = volumePercent;
        volumeText.Text = $"Volume: {volumePercent}%";
        muteCheckBox.IsChecked = device?.IsMuted ?? false;
        volumeSlider.IsEnabled = device is not null;
        muteCheckBox.IsEnabled = device is not null;

        isUpdatingControls = false;
    }

    private void PlaybackVolumeSlider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        UpdateVolume(
            listBox: PlaybackDevicesListBox,
            volumeSlider: PlaybackVolumeSlider,
            volumeText: PlaybackVolumeText,
            isUpdatingControls: _isUpdatingPlaybackControls);
    }

    private void RecordingVolumeSlider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        UpdateVolume(
            listBox: RecordingDevicesListBox,
            volumeSlider: RecordingVolumeSlider,
            volumeText: RecordingVolumeText,
            isUpdatingControls: _isUpdatingRecordingControls);
    }

    private void UpdateVolume(ListBox listBox, Slider volumeSlider, TextBlock volumeText, bool isUpdatingControls)
    {
        var volumePercent = Math.Round(volumeSlider.Value);
        volumeText.Text = $"Volume: {volumePercent}%";

        if (isUpdatingControls || listBox.SelectedItem is not AudioDeviceItem selectedDevice)
        {
            return;
        }

        RunSafely(() =>
        {
            var newVolume = (float)(volumeSlider.Value / 100d);
            _audioDeviceService.SetVolume(selectedDevice.Id, newVolume);
            selectedDevice.Volume = newVolume;
        }, $"Couldn't change the volume for {selectedDevice.Name}.");
    }

    private void PlaybackMuteCheckBox_OnChanged(object sender, RoutedEventArgs e)
    {
        UpdateMuteState(
            listBox: PlaybackDevicesListBox,
            muteCheckBox: PlaybackMuteCheckBox,
            isUpdatingControls: _isUpdatingPlaybackControls);
    }

    private void RecordingMuteCheckBox_OnChanged(object sender, RoutedEventArgs e)
    {
        UpdateMuteState(
            listBox: RecordingDevicesListBox,
            muteCheckBox: RecordingMuteCheckBox,
            isUpdatingControls: _isUpdatingRecordingControls);
    }

    private void UpdateMuteState(ListBox listBox, CheckBox muteCheckBox, bool isUpdatingControls)
    {
        if (isUpdatingControls || listBox.SelectedItem is not AudioDeviceItem selectedDevice)
        {
            return;
        }

        RunSafely(() =>
        {
            var isMuted = muteCheckBox.IsChecked == true;
            _audioDeviceService.SetMute(selectedDevice.Id, isMuted);
            selectedDevice.IsMuted = isMuted;
        }, $"Couldn't update mute state for {selectedDevice.Name}.");
    }

    private void SetPlaybackDefaultButton_OnClick(object sender, RoutedEventArgs e)
    {
        SetSelectedDeviceAsDefault(PlaybackDevicesListBox, "playback");
    }

    private void SetRecordingDefaultButton_OnClick(object sender, RoutedEventArgs e)
    {
        SetSelectedDeviceAsDefault(RecordingDevicesListBox, "recording");
    }

    private void SetSelectedDeviceAsDefault(ListBox listBox, string deviceType)
    {
        if (listBox.SelectedItem is not AudioDeviceItem device)
        {
            return;
        }

        RunSafely(() =>
        {
            _audioDeviceService.SetDefaultDevice(device.Id);
            RefreshDevices(keepSelections: true);
        }, $"Couldn't set the selected {deviceType} device as default.");
    }

    private static void RunSafely(Action action, string message)
    {
        try
        {
            action();
        }
        catch (Exception exception)
        {
            MessageBox.Show(
                $"{message}\n\n{exception.Message}",
                "AudioDeviceManager",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }
}
