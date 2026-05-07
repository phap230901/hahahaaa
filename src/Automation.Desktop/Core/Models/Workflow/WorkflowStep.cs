namespace Automation.Desktop.Core.Models.Workflow;

public sealed class WorkflowStep
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required WorkflowStepType Type { get; init; }
    public Dictionary<string, string> Parameters { get; init; } = [];
    public int DelayAfterMs { get; init; }
    public int TimeoutMs { get; init; } = 15000;
    public int RetryCount { get; init; } = 0;
}
