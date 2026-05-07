using Automation.Desktop.Core.Interfaces;
using Automation.Desktop.Core.Models;

namespace Automation.Desktop.Application.Managers;

public sealed class DeviceManager(IAdbService adbService, IDatabaseService databaseService) : IDeviceManager
{
    private readonly List<DeviceInfo> _devices = [];
    private readonly SemaphoreSlim _gate = new(1, 1);

    public event EventHandler<IReadOnlyList<DeviceInfo>>? DevicesUpdated;

    public IReadOnlyList<DeviceInfo> CurrentDevices => _devices.AsReadOnly();

    public async Task RefreshDevicesAsync(CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            var devices = await adbService.GetDevicesAsync(cancellationToken);
            _devices.Clear();
            _devices.AddRange(devices);
            await databaseService.UpsertDevicesAsync(_devices, cancellationToken);
            DevicesUpdated?.Invoke(this, CurrentDevices);
        }
        finally
        {
            _gate.Release();
        }
    }
}
