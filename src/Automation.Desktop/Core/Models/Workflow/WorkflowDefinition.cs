namespace Automation.Desktop.Core.Models.Workflow;

public sealed class WorkflowDefinition
{
    public required string WorkflowId { get; init; }
    public required string Name { get; init; }
    public List<WorkflowStep> Steps { get; init; } = [];
}
