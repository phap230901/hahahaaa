using Automation.Desktop.Core.Interfaces;
using Automation.Desktop.Core.Models;
using Automation.Desktop.Infrastructure.Imaging.Detection;
using Microsoft.Extensions.Logging;

namespace Automation.Desktop.Infrastructure.Imaging;

public sealed class ImageDetectionService(
    TemplateMatcher matcher,
    IAdbService adbService,
    ILogger<ImageDetectionService> logger) : IImageDetectionService
{
    public Task<DetectionResult> DetectTemplateAsync(string sourceImagePath, string templateImagePath, double threshold = 0.85, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.Run(() => matcher.Match(sourceImagePath, templateImagePath, threshold), cancellationToken);
    }

    public async Task<DetectionResult> DetectTemplateFromDeviceScreenAsync(string deviceId, string templateImagePath, double threshold = 0.85, CancellationToken cancellationToken = default)
    {
        var screenshotPath = Path.Combine(Path.GetTempPath(), $"{deviceId}_{DateTime.UtcNow:yyyyMMdd_HHmmssfff}_detect.png");
        try
        {
            await adbService.CaptureScreenAsync(deviceId, screenshotPath, cancellationToken);
            return await DetectTemplateAsync(screenshotPath, templateImagePath, threshold, cancellationToken);
        }
        finally
        {
            if (File.Exists(screenshotPath)) File.Delete(screenshotPath);
        }
    }

    public async Task<bool> ClickDetectedImageAsync(string deviceId, string templateImagePath, double threshold = 0.85, CancellationToken cancellationToken = default)
    {
        var detection = await DetectTemplateFromDeviceScreenAsync(deviceId, templateImagePath, threshold, cancellationToken);
        if (!detection.Found)
        {
            logger.LogInformation("Template not found on device {DeviceId}. template={Template}", deviceId, templateImagePath);
            return false;
        }

        var x = detection.Bounds.Left + detection.Bounds.Width / 2;
        var y = detection.Bounds.Top + detection.Bounds.Height / 2;
        await adbService.TapAsync(deviceId, x, y, cancellationToken);

        logger.LogInformation("Clicked detected template on {DeviceId}. center=({X},{Y}) confidence={Confidence:F4} scale={Scale:F2}", deviceId, x, y, detection.Confidence, detection.Scale);
        return true;
    }
}
