using Automation.Desktop.Core.Models;

namespace Automation.Desktop.Core.Interfaces;

public interface IOcrService
{
    Task<IReadOnlyList<OcrResult>> DetectTextAsync(string imagePath, CancellationToken cancellationToken = default);
    Task<OcrResult?> FindTextAsync(string imagePath, string targetText, StringComparison comparison = StringComparison.OrdinalIgnoreCase, CancellationToken cancellationToken = default);
    Task<bool> ClickTextAsync(string deviceId, string imagePath, string targetText, CancellationToken cancellationToken = default);
}
