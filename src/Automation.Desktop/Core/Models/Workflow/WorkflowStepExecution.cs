namespace Automation.Desktop.Core.Models.Workflow;

public sealed class WorkflowStepExecution
{
    public required string StepId { get; init; }
    public required string StepName { get; init; }
    public required string DeviceId { get; init; }
    public WorkflowStepStatus Status { get; set; } = WorkflowStepStatus.Pending;
    public string? Message { get; set; }
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
}
