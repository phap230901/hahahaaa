using Automation.Desktop.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace Automation.Desktop.Infrastructure.Logging;

public sealed class LogService(ILogger<LogService> logger) : ILogService
{
    public Task InfoAsync(string message, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(message);
        return Task.CompletedTask;
    }

    public Task ErrorAsync(string message, Exception exception, CancellationToken cancellationToken = default)
    {
        logger.LogError(exception, message);
        return Task.CompletedTask;
    }
}
