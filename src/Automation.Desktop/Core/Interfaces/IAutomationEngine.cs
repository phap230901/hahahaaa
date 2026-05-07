using Automation.Desktop.Core.Models.Workflow;

namespace Automation.Desktop.Core.Interfaces;

public interface IAutomationEngine
{
    Task EnqueueAsync(string deviceId, WorkflowDefinition workflow, CancellationToken cancellationToken = default);
    Task PauseAsync(string deviceId);
    Task ResumeAsync(string deviceId);
    Task StopAsync(string deviceId);
    IReadOnlyCollection<WorkflowStepExecution> GetStepStatuses(string deviceId);
}
