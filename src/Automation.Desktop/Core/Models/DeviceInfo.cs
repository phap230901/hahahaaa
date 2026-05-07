namespace Automation.Desktop.Core.Models;

public sealed class DeviceInfo
{
    public string DeviceId { get; init; } = string.Empty;
    public string Status { get; init; } = "unknown";
    public string Model { get; init; } = string.Empty;
    public bool IsConnected => Status.Equals("device", StringComparison.OrdinalIgnoreCase);
}
