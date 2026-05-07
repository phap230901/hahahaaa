using Automation.Desktop.Core.Models;

namespace Automation.Desktop.Core.Interfaces;

public interface IAdbService
{
    Task<IReadOnlyList<DeviceInfo>> GetDevicesAsync(CancellationToken cancellationToken = default);
    Task<AdbCommandResult> TapAsync(string deviceId, int x, int y, CancellationToken cancellationToken = default);
    Task<AdbCommandResult> SwipeAsync(string deviceId, int x1, int y1, int x2, int y2, int durationMs, CancellationToken cancellationToken = default);
    Task<AdbCommandResult> InputTextAsync(string deviceId, string text, CancellationToken cancellationToken = default);
    Task<AdbCommandResult> CaptureScreenAsync(string deviceId, string outputPath, CancellationToken cancellationToken = default);
    Task<AdbCommandResult> OpenAppAsync(string deviceId, string packageName, string? activityName = null, CancellationToken cancellationToken = default);
    Task<AdbCommandResult> ForceStopAppAsync(string deviceId, string packageName, CancellationToken cancellationToken = default);
    Task<AdbCommandResult> ClearAppDataAsync(string deviceId, string packageName, CancellationToken cancellationToken = default);
    Task<AdbCommandResult> InstallApkAsync(string deviceId, string apkPath, CancellationToken cancellationToken = default);
}
