using Automation.Desktop.Core.Models;

namespace Automation.Desktop.Core.Interfaces;

public interface IImageDetectionService
{
    Task<DetectionResult> DetectTemplateAsync(string sourceImagePath, string templateImagePath, double threshold = 0.85, CancellationToken cancellationToken = default);
    Task<DetectionResult> DetectTemplateFromDeviceScreenAsync(string deviceId, string templateImagePath, double threshold = 0.85, CancellationToken cancellationToken = default);
    Task<bool> ClickDetectedImageAsync(string deviceId, string templateImagePath, double threshold = 0.85, CancellationToken cancellationToken = default);
}
