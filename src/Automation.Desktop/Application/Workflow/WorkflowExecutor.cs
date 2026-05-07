using Automation.Desktop.Core.Interfaces;
using Automation.Desktop.Core.Models.Workflow;

namespace Automation.Desktop.Application.Workflow;

public sealed class WorkflowExecutor(IAdbService adbService, IOcrService ocrService, IImageDetectionService imageDetectionService, ILogService logService)
{
    public async Task<WorkflowStepExecution> ExecuteStepAsync(string deviceId, WorkflowStep step, CancellationToken cancellationToken)
    {
        var execution = new WorkflowStepExecution { StepId = step.Id, StepName = step.Name, DeviceId = deviceId, StartedAt = DateTimeOffset.UtcNow, Status = WorkflowStepStatus.Running };

        try
        {
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(step.TimeoutMs);
            var ct = timeoutCts.Token;

            await ExecuteByTypeAsync(deviceId, step, ct);
            execution.Status = WorkflowStepStatus.Success;
            execution.Message = "Completed";
        }
        catch (OperationCanceledException)
        {
            execution.Status = WorkflowStepStatus.Failed;
            execution.Message = "Canceled or timeout";
        }
        catch (Exception ex)
        {
            execution.Status = WorkflowStepStatus.Failed;
            execution.Message = ex.Message;
            await logService.ErrorAsync($"Workflow step failed: {step.Name} on {deviceId}", ex, cancellationToken);
        }
        finally
        {
            execution.CompletedAt = DateTimeOffset.UtcNow;
        }

        return execution;
    }

    private async Task ExecuteByTypeAsync(string deviceId, WorkflowStep step, CancellationToken ct)
    {
        switch (step.Type)
        {
            case WorkflowStepType.Tap:
                await adbService.TapAsync(deviceId, int.Parse(step.Parameters["x"]), int.Parse(step.Parameters["y"]), ct);
                break;
            case WorkflowStepType.Swipe:
                await adbService.SwipeAsync(deviceId, int.Parse(step.Parameters["x1"]), int.Parse(step.Parameters["y1"]), int.Parse(step.Parameters["x2"]), int.Parse(step.Parameters["y2"]), int.Parse(step.Parameters.GetValueOrDefault("durationMs", "350")), ct);
                break;
            case WorkflowStepType.Input:
                await adbService.InputTextAsync(deviceId, step.Parameters["text"], ct);
                break;
            case WorkflowStepType.OcrSearch:
                var screenshot1 = step.Parameters["screenshotPath"];
                var target = step.Parameters["targetText"];
                var foundText = await ocrService.FindTextAsync(screenshot1, target, cancellationToken: ct);
                if (foundText is null) throw new InvalidOperationException($"Text '{target}' not found.");
                break;
            case WorkflowStepType.ImageDetect:
                var detection = await imageDetectionService.DetectTemplateAsync(step.Parameters["screenshotPath"], step.Parameters["templatePath"], double.Parse(step.Parameters.GetValueOrDefault("threshold", "0.85")), ct);
                if (!detection.Found) throw new InvalidOperationException("Template not found.");
                break;
            case WorkflowStepType.Wait:
            case WorkflowStepType.Delay:
                await Task.Delay(int.Parse(step.Parameters.GetValueOrDefault("ms", "1000")), ct);
                break;
            case WorkflowStepType.Condition:
                if (!bool.Parse(step.Parameters.GetValueOrDefault("value", "true"))) throw new InvalidOperationException("Condition false.");
                break;
            case WorkflowStepType.Retry:
                await Task.Delay(10, ct);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        if (step.DelayAfterMs > 0)
            await Task.Delay(step.DelayAfterMs, ct);
    }
}
