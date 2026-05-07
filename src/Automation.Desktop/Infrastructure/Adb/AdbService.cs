using System.Diagnostics;
using System.Text;
using Automation.Desktop.Core.Interfaces;
using Automation.Desktop.Core.Models;
using Microsoft.Extensions.Logging;

namespace Automation.Desktop.Infrastructure.Adb;

public sealed class AdbService(ILogger<AdbService> logger) : IAdbService
{
    private const string AdbExecutable = "adb";
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan InstallTimeout = TimeSpan.FromMinutes(4);

    public async Task<IReadOnlyList<DeviceInfo>> GetDevicesAsync(CancellationToken cancellationToken = default)
    {
        var result = await ExecuteAdbAsync("devices -l", "host", DefaultTimeout, cancellationToken);
        EnsureSuccess(result);

        var lines = result.StandardOutput
            .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
            .Skip(1);

        return lines
            .Select(ParseDeviceLine)
            .Where(device => device is not null)
            .Cast<DeviceInfo>()
            .ToList();
    }

    public Task<AdbCommandResult> TapAsync(string deviceId, int x, int y, CancellationToken cancellationToken = default) =>
        ExecuteAndValidateAsync(deviceId, $"shell input tap {x} {y}", DefaultTimeout, cancellationToken);

    public Task<AdbCommandResult> SwipeAsync(string deviceId, int x1, int y1, int x2, int y2, int durationMs, CancellationToken cancellationToken = default) =>
        ExecuteAndValidateAsync(deviceId, $"shell input swipe {x1} {y1} {x2} {y2} {durationMs}", DefaultTimeout, cancellationToken);

    public Task<AdbCommandResult> InputTextAsync(string deviceId, string text, CancellationToken cancellationToken = default)
    {
        var escaped = EscapeForAdbInput(text);
        return ExecuteAndValidateAsync(deviceId, $"shell input text {escaped}", DefaultTimeout, cancellationToken);
    }

    public async Task<AdbCommandResult> CaptureScreenAsync(string deviceId, string outputPath, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath) ?? AppContext.BaseDirectory);
        var args = $"-s {Quote(deviceId)} exec-out screencap -p";
        var result = await ExecuteAdbAsync(args, deviceId, TimeSpan.FromSeconds(45), cancellationToken, outputPath);
        EnsureSuccess(result);
        return result;
    }

    public Task<AdbCommandResult> OpenAppAsync(string deviceId, string packageName, string? activityName = null, CancellationToken cancellationToken = default)
    {
        var command = string.IsNullOrWhiteSpace(activityName)
            ? $"shell monkey -p {Quote(packageName)} -c android.intent.category.LAUNCHER 1"
            : $"shell am start -n {Quote(packageName + "/" + activityName)}";
        return ExecuteAndValidateAsync(deviceId, command, DefaultTimeout, cancellationToken);
    }

    public Task<AdbCommandResult> ForceStopAppAsync(string deviceId, string packageName, CancellationToken cancellationToken = default) =>
        ExecuteAndValidateAsync(deviceId, $"shell am force-stop {Quote(packageName)}", DefaultTimeout, cancellationToken);

    public Task<AdbCommandResult> ClearAppDataAsync(string deviceId, string packageName, CancellationToken cancellationToken = default) =>
        ExecuteAndValidateAsync(deviceId, $"shell pm clear {Quote(packageName)}", DefaultTimeout, cancellationToken);

    public Task<AdbCommandResult> InstallApkAsync(string deviceId, string apkPath, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(apkPath))
            throw new FileNotFoundException("APK file not found.", apkPath);

        return ExecuteAndValidateAsync(deviceId, $"install -r {Quote(apkPath)}", InstallTimeout, cancellationToken);
    }

    private async Task<AdbCommandResult> ExecuteAndValidateAsync(string deviceId, string command, TimeSpan timeout, CancellationToken cancellationToken)
    {
        var result = await ExecuteAdbAsync($"-s {Quote(deviceId)} {command}", deviceId, timeout, cancellationToken);
        EnsureSuccess(result);
        return result;
    }

    private async Task<AdbCommandResult> ExecuteAdbAsync(string args, string deviceId, TimeSpan timeout, CancellationToken cancellationToken, string? stdoutToFile = null)
    {
        var startedAt = Stopwatch.StartNew();

        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = AdbExecutable,
                Arguments = args,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardErrorEncoding = Encoding.UTF8,
                StandardOutputEncoding = Encoding.UTF8
            },
            EnableRaisingEvents = true
        };

        logger.LogDebug("ADB command start [{DeviceId}]: {Command}", deviceId, args);

        if (!process.Start())
            throw new InvalidOperationException("Failed to start adb process.");

        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        linkedCts.CancelAfter(timeout);

        try
        {
            string output;
            var stdErrTask = process.StandardError.ReadToEndAsync(linkedCts.Token);
            if (!string.IsNullOrWhiteSpace(stdoutToFile))
            {
                await using var fs = File.Create(stdoutToFile);
                await process.StandardOutput.BaseStream.CopyToAsync(fs, linkedCts.Token);
                output = string.Empty;
            }
            else
            {
                output = await process.StandardOutput.ReadToEndAsync(linkedCts.Token);
            }

            await process.WaitForExitAsync(linkedCts.Token);
            var error = await stdErrTask;

            var result = new AdbCommandResult
            {
                DeviceId = deviceId,
                Command = args,
                IsSuccess = process.ExitCode == 0,
                ExitCode = process.ExitCode,
                StandardOutput = output,
                StandardError = error,
                IsTimeout = false,
                Duration = startedAt.Elapsed
            };

            logger.LogInformation("ADB command finished [{DeviceId}] ExitCode={ExitCode} in {Duration}ms", deviceId, process.ExitCode, result.Duration.TotalMilliseconds);
            return result;
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            TryKillProcess(process);
            var timeoutResult = new AdbCommandResult
            {
                DeviceId = deviceId,
                Command = args,
                IsSuccess = false,
                ExitCode = -1,
                StandardError = $"Timeout after {timeout.TotalSeconds:F0}s",
                IsTimeout = true,
                Duration = startedAt.Elapsed
            };
            logger.LogError("ADB command timeout [{DeviceId}]: {Command}", deviceId, args);
            return timeoutResult;
        }
    }

    private static void EnsureSuccess(AdbCommandResult result)
    {
        if (!result.IsSuccess)
            throw new InvalidOperationException(result.ErrorSummary);
    }

    private static DeviceInfo? ParseDeviceLine(string line)
    {
        var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2) return null;

        return new DeviceInfo
        {
            DeviceId = parts[0],
            Status = parts[1],
            Model = parts.FirstOrDefault(x => x.StartsWith("model:", StringComparison.OrdinalIgnoreCase))?.Replace("model:", string.Empty, StringComparison.OrdinalIgnoreCase) ?? string.Empty
        };
    }

    private static string EscapeForAdbInput(string text) => Quote(text.Replace(" ", "%s"));

    private static string Quote(string value) => $"\"{value.Replace("\"", "\\\"")}\"";

    private static void TryKillProcess(Process process)
    {
        try
        {
            if (!process.HasExited) process.Kill(true);
        }
        catch
        {
            // no-op
        }
    }
}
