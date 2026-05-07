using System.Drawing;

namespace Automation.Desktop.Core.Models;

public sealed record DetectionResult(
    bool Found,
    Point Location,
    Size MatchSize,
    Rectangle Bounds,
    double Confidence,
    double Scale,
    string TemplatePath,
    string SourceImagePath);
