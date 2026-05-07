using Automation.Desktop.Core.Models;

namespace Automation.Desktop.Core.Interfaces;

public interface IDeviceManager
{
    event EventHandler<IReadOnlyList<DeviceInfo>>? DevicesUpdated;
    IReadOnlyList<DeviceInfo> CurrentDevices { get; }
    Task RefreshDevicesAsync(CancellationToken cancellationToken = default);
}
