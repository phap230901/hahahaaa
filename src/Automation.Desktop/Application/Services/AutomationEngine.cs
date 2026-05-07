using Automation.Desktop.Application.Workflow;
using Automation.Desktop.Core.Interfaces;
using Automation.Desktop.Core.Models.Workflow;

namespace Automation.Desktop.Application.Services;

public sealed class AutomationEngine(WorkflowManager manager) : IAutomationEngine
{
    public Task EnqueueAsync(string deviceId, WorkflowDefinition workflow, CancellationToken cancellationToken = default) => manager.EnqueueAsync(deviceId, workflow, cancellationToken);
    public Task PauseAsync(string deviceId) => manager.PauseAsync(deviceId);
    public Task ResumeAsync(string deviceId) => manager.ResumeAsync(deviceId);
    public Task StopAsync(string deviceId) => manager.StopAsync(deviceId);
    public IReadOnlyCollection<WorkflowStepExecution> GetStepStatuses(string deviceId) => manager.GetStepStatuses(deviceId);
}
