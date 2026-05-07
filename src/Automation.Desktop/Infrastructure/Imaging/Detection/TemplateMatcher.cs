using Automation.Desktop.Core.Models;
using OpenCvSharp;
using Point = System.Drawing.Point;
using Rectangle = System.Drawing.Rectangle;
using Size = System.Drawing.Size;

namespace Automation.Desktop.Infrastructure.Imaging.Detection;

public sealed class TemplateMatcher
{
    private static readonly double[] Scales = [0.5, 0.65, 0.8, 1.0, 1.2, 1.4, 1.6];

    public DetectionResult Match(string sourceImagePath, string templateImagePath, double threshold)
    {
        using var sourceColor = Cv2.ImRead(sourceImagePath, ImreadModes.Color);
        using var templateColor = Cv2.ImRead(templateImagePath, ImreadModes.Color);

        if (sourceColor.Empty() || templateColor.Empty())
            throw new InvalidOperationException("Source or template image could not be loaded.");

        using var source = sourceColor.CvtColor(ColorConversionCodes.BGR2GRAY);

        var bestConfidence = double.MinValue;
        Point bestPoint = Point.Empty;
        Size bestSize = Size.Empty;
        var bestScale = 1.0;

        foreach (var scale in Scales)
        {
            using var scaledTemplate = ResizeTemplate(templateColor, scale);
            if (scaledTemplate.Width <= 0 || scaledTemplate.Height <= 0) continue;
            if (scaledTemplate.Width > source.Width || scaledTemplate.Height > source.Height) continue;

            using var tplGray = scaledTemplate.CvtColor(ColorConversionCodes.BGR2GRAY);
            using var result = new Mat();
            Cv2.MatchTemplate(source, tplGray, result, TemplateMatchModes.CCoeffNormed);
            Cv2.MinMaxLoc(result, out _, out var maxVal, out _, out var maxLoc);

            if (maxVal <= bestConfidence) continue;
            bestConfidence = maxVal;
            bestPoint = new Point(maxLoc.X, maxLoc.Y);
            bestSize = new Size(scaledTemplate.Width, scaledTemplate.Height);
            bestScale = scale;
        }

        var found = bestConfidence >= threshold && bestSize != Size.Empty;
        var bounds = found
            ? new Rectangle(bestPoint.X, bestPoint.Y, bestSize.Width, bestSize.Height)
            : Rectangle.Empty;

        return new DetectionResult(found, bestPoint, bestSize, bounds, bestConfidence, bestScale, templateImagePath, sourceImagePath);
    }

    private static Mat ResizeTemplate(Mat template, double scale)
    {
        var width = (int)(template.Width * scale);
        var height = (int)(template.Height * scale);
        if (width < 1 || height < 1) return new Mat();

        var destination = new Mat();
        Cv2.Resize(template, destination, new OpenCvSharp.Size(width, height), interpolation: InterpolationFlags.Area);
        return destination;
    }
}
