using System.Drawing;

namespace Automation.Desktop.Core.Models;

public sealed record OcrResult(
    string Text,
    Rectangle Bounds,
    float Confidence,
    IReadOnlyList<Point> PolygonPoints);
