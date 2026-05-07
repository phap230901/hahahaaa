using System.Collections.Concurrent;
using Automation.Desktop.Core.Interfaces;
using Automation.Desktop.Core.Models.Workflow;

namespace Automation.Desktop.Application.Workflow;

public sealed class WorkflowManager(WorkflowExecutor executor) : IAutomationEngine
{
    private readonly ConcurrentDictionary<string, DeviceWorker> _workers = new();

    public Task EnqueueAsync(string deviceId, WorkflowDefinition workflow, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var worker = _workers.GetOrAdd(deviceId, id =>
        {
            var w = new DeviceWorker(id, executor);
            w.Start();
            return w;
        });
        return worker.EnqueueAsync(workflow);
    }

    public Task PauseAsync(string deviceId)
    {
        if (_workers.TryGetValue(deviceId, out var worker)) worker.Pause();
        return Task.CompletedTask;
    }

    public Task ResumeAsync(string deviceId)
    {
        if (_workers.TryGetValue(deviceId, out var worker)) worker.Resume();
        return Task.CompletedTask;
    }

    public Task StopAsync(string deviceId)
    {
        if (_workers.TryRemove(deviceId, out var worker)) worker.Stop();
        return Task.CompletedTask;
    }

    public IReadOnlyCollection<WorkflowStepExecution> GetStepStatuses(string deviceId) =>
        _workers.TryGetValue(deviceId, out var worker) ? worker.GetStatuses() : Array.Empty<WorkflowStepExecution>();
}
