using System.Collections.Concurrent;
using Automation.Desktop.Core.Models.Workflow;

namespace Automation.Desktop.Application.Workflow;

public sealed class DeviceWorker(string deviceId, WorkflowExecutor executor)
{
    private readonly ConcurrentQueue<WorkflowDefinition> _queue = new();
    private readonly SemaphoreSlim _queueSignal = new(0);
    private readonly ManualResetEventSlim _pauseEvent = new(true);
    private readonly ConcurrentDictionary<string, WorkflowStepExecution> _statuses = new();
    private readonly CancellationTokenSource _cts = new();
    private Task? _runner;

    public string DeviceId => deviceId;

    public void Start() => _runner ??= Task.Run(RunAsync);

    public Task EnqueueAsync(WorkflowDefinition workflow)
    {
        _queue.Enqueue(workflow);
        _queueSignal.Release();
        return Task.CompletedTask;
    }

    public void Pause() => _pauseEvent.Reset();
    public void Resume() => _pauseEvent.Set();
    public void Stop() => _cts.Cancel();
    public IReadOnlyCollection<WorkflowStepExecution> GetStatuses() => _statuses.Values.OrderBy(x => x.StartedAt).ToList().AsReadOnly();

    private async Task RunAsync()
    {
        while (!_cts.IsCancellationRequested)
        {
            await _queueSignal.WaitAsync(_cts.Token);
            if (!_queue.TryDequeue(out var workflow)) continue;

            foreach (var step in workflow.Steps)
            {
                _pauseEvent.Wait(_cts.Token);
                var attempts = 0;
                WorkflowStepExecution execution;
                do
                {
                    execution = await executor.ExecuteStepAsync(deviceId, step, _cts.Token);
                    _statuses[$"{workflow.WorkflowId}:{step.Id}:{attempts}"] = execution;
                    attempts++;
                }
                while (execution.Status == WorkflowStepStatus.Failed && attempts <= step.RetryCount && !_cts.IsCancellationRequested);
            }
        }
    }
}
