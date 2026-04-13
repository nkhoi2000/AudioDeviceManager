using NAudio.CoreAudioApi;

namespace AudioDeviceManager.Models;

public sealed class AudioDeviceItem
{
    public required string Id { get; init; }

    public required string Name { get; init; }

    public required DataFlow Flow { get; init; }

    public required DeviceState State { get; init; }

    public float Volume { get; set; }

    public bool IsMuted { get; set; }

    public bool IsDefault { get; set; }

    public string Status => State.ToString();

    public string Summary => $"{Name} ({Math.Round(Volume * 100)}%)";
}
