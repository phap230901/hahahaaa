namespace Automation.Desktop.Core.Interfaces;

public interface ILogService
{
    Task InfoAsync(string message, CancellationToken cancellationToken = default);
    Task ErrorAsync(string message, Exception exception, CancellationToken cancellationToken = default);
}
