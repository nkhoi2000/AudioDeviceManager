using AudioDeviceManager.Interop;
using AudioDeviceManager.Models;

using NAudio.CoreAudioApi;

namespace AudioDeviceManager.Services;

public sealed class AudioDeviceService : IDisposable
{
    private readonly MMDeviceEnumerator _enumerator = new();
    private readonly PolicyConfigClient _policyConfigClient = new();

    public IReadOnlyList<AudioDeviceItem> GetPlaybackDevices()
    {
        return GetDevices(DataFlow.Render);
    }

    public IReadOnlyList<AudioDeviceItem> GetRecordingDevices()
    {
        return GetDevices(DataFlow.Capture);
    }

    public void SetDefaultDevice(string deviceId)
    {
        _policyConfigClient.SetDefaultDevice(deviceId);
    }

    public void SetVolume(string deviceId, float volume)
    {
        using var device = _enumerator.GetDevice(deviceId);
        device.AudioEndpointVolume.MasterVolumeLevelScalar = volume;
    }

    public void SetMute(string deviceId, bool isMuted)
    {
        using var device = _enumerator.GetDevice(deviceId);
        device.AudioEndpointVolume.Mute = isMuted;
    }

    private List<AudioDeviceItem> GetDevices(DataFlow flow)
    {
        using var defaultDevice = TryGetDefaultDevice(flow);
        var defaultId = defaultDevice?.ID;

        return _enumerator
            .EnumerateAudioEndPoints(flow, DeviceState.Active)
            .Select(device => CreateDeviceItem(device, flow, defaultId))
            .OrderByDescending(device => device.IsDefault)
            .ThenBy(device => device.Name)
            .ToList();
    }

    private static AudioDeviceItem CreateDeviceItem(MMDevice device, DataFlow flow, string? defaultId)
    {
        try
        {
            return new AudioDeviceItem
            {
                Id = device.ID,
                Name = device.FriendlyName,
                Flow = flow,
                State = device.State,
                Volume = device.AudioEndpointVolume.MasterVolumeLevelScalar,
                IsMuted = device.AudioEndpointVolume.Mute,
                IsDefault = string.Equals(device.ID, defaultId, StringComparison.OrdinalIgnoreCase),
            };
        }
        finally
        {
            device.Dispose();
        }
    }

    private MMDevice? TryGetDefaultDevice(DataFlow flow)
    {
        try
        {
            return _enumerator.GetDefaultAudioEndpoint(flow, Role.Multimedia);
        }
        catch
        {
            return null;
        }
    }

    public void Dispose()
    {
        _enumerator.Dispose();
    }
}
