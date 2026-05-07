using Automation.Desktop.Core.Interfaces;
using Automation.Desktop.Core.Models;
using Automation.Desktop.Infrastructure.OCR.Detection;
using Microsoft.Extensions.Logging;

namespace Automation.Desktop.Infrastructure.OCR;

public sealed class OcrService(
    OCRTextDetector detector,
    IAdbService adbService,
    ILogger<OcrService> logger) : IOcrService
{
    public Task<IReadOnlyList<OcrResult>> DetectTextAsync(string imagePath, CancellationToken cancellationToken = default) =>
        detector.DetectAsync(imagePath, cancellationToken);

    public async Task<OcrResult?> FindTextAsync(string imagePath, string targetText, StringComparison comparison = StringComparison.OrdinalIgnoreCase, CancellationToken cancellationToken = default)
    {
        var all = await DetectTextAsync(imagePath, cancellationToken);
        return all.FirstOrDefault(x => x.Text.Contains(targetText, comparison));
    }

    public async Task<bool> ClickTextAsync(string deviceId, string imagePath, string targetText, CancellationToken cancellationToken = default)
    {
        var text = await FindTextAsync(imagePath, targetText, StringComparison.OrdinalIgnoreCase, cancellationToken);
        if (text is null)
        {
            logger.LogInformation("Target text '{Text}' not found on image {Image} for {DeviceId}", targetText, imagePath, deviceId);
            return false;
        }

        var x = text.Bounds.Left + (text.Bounds.Width / 2);
        var y = text.Bounds.Top + (text.Bounds.Height / 2);
        await adbService.TapAsync(deviceId, x, y, cancellationToken);
        logger.LogInformation("Clicked text '{Text}' at ({X}, {Y}) on {DeviceId}", text.Text, x, y, deviceId);
        return true;
    }

    public async Task<bool> ClickTextFromDeviceScreenAsync(string deviceId, string targetText, CancellationToken cancellationToken = default)
    {
        var screenshotPath = Path.Combine(Path.GetTempPath(), $"{deviceId}_{DateTime.UtcNow:yyyyMMdd_HHmmssfff}.png");
        try
        {
            await adbService.CaptureScreenAsync(deviceId, screenshotPath, cancellationToken);
            return await ClickTextAsync(deviceId, screenshotPath, targetText, cancellationToken);
        }
        finally
        {
            if (File.Exists(screenshotPath)) File.Delete(screenshotPath);
        }
    }
}
