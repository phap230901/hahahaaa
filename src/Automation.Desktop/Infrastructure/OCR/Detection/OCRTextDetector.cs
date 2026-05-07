using System.Drawing;
using Automation.Desktop.Core.Models;
using Sdcb.PaddleOCR;

namespace Automation.Desktop.Infrastructure.OCR.Detection;

public sealed class OCRTextDetector : IDisposable
{
    private readonly PaddleOcrAll _engine;
    private readonly SemaphoreSlim _gate = new(1, 1);

    public OCRTextDetector()
    {
        _engine = new PaddleOcrAll(PaddleDevice.Mkldnn())
        {
            AllowRotateDetection = true,
            Enable180Classification = true
        };
    }

    public async Task<IReadOnlyList<OcrResult>> DetectAsync(string imagePath, CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            return await Task.Run(() =>
            {
                using var src = File.OpenRead(imagePath);
                var result = _engine.Run(src);
                return result.Regions.Select(r =>
                {
                    var points = r.Rect.Select(p => new Point((int)p.X, (int)p.Y)).ToList();
                    var minX = points.Min(p => p.X);
                    var maxX = points.Max(p => p.X);
                    var minY = points.Min(p => p.Y);
                    var maxY = points.Max(p => p.Y);
                    return new OcrResult(r.Text, Rectangle.FromLTRB(minX, minY, maxX, maxY), (float)r.Score, points);
                }).ToList().AsReadOnly();
            }, cancellationToken);
        }
        finally
        {
            _gate.Release();
        }
    }

    public void Dispose()
    {
        _engine.Dispose();
        _gate.Dispose();
    }
}
