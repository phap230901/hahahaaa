using Automation.Desktop.Core.Models;

namespace Automation.Desktop.Core.Interfaces;

public interface IDatabaseService
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
    Task UpsertDevicesAsync(IEnumerable<DeviceInfo> devices, CancellationToken cancellationToken = default);
}
