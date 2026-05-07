namespace Automation.Desktop.Core.Models;

public sealed class AdbCommandResult
{
    public required string Command { get; init; }
    public required string DeviceId { get; init; }
    public required bool IsSuccess { get; init; }
    public required int ExitCode { get; init; }
    public string StandardOutput { get; init; } = string.Empty;
    public string StandardError { get; init; } = string.Empty;
    public bool IsTimeout { get; init; }
    public TimeSpan Duration { get; init; }

    public string ErrorSummary => IsTimeout
        ? $"ADB command timed out: {Command}"
        : string.IsNullOrWhiteSpace(StandardError) ? $"ADB command failed with exit code {ExitCode}." : StandardError.Trim();
}
