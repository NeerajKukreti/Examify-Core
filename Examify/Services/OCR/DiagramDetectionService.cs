using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Examify.Services.OCR;

public class DiagramDetectionService
{
    public List<DiagramBounds> DetectDiagrams(byte[] imageBytes, int minWidth = 150, int minHeight = 150)
    {
        // Try multiple strategies with different kernel sizes
        var results = new List<List<DiagramBounds>>
        {
            DetectWithStrategy(imageBytes, 3, minWidth, minHeight),  // Tiny kernel - best for separate diagrams
            DetectWithStrategy(imageBytes, 5, minWidth, minHeight),  // Very small kernel
            DetectWithStrategy(imageBytes, 8, minWidth, minHeight),  // Small kernel
            DetectWithStrategy(imageBytes, 10, minWidth, minHeight), // Medium kernel
            DetectWithStrategy(imageBytes, 15, minWidth, minHeight)  // Large kernel
        };

        using var mat = new Mat();
        CvInvoke.Imdecode(imageBytes, ImreadModes.Color, mat);
        var imageArea = mat.Width * mat.Height;

        // Pick best result: prefer more diagrams (2-5), then 1, reject if any too large
        var validResults = results
            .Where(r => r.Count >= 1 && r.Count <= 5 && r.All(d => (d.Width * d.Height) < imageArea * 0.2))
            .OrderByDescending(r => r.Count)
            .ToList();

        return validResults.FirstOrDefault() ?? new List<DiagramBounds>();
    }

    private List<DiagramBounds> DetectWithStrategy(byte[] imageBytes, int kernelSize, int minWidth, int minHeight)
    {
        using var mat = new Mat();
        CvInvoke.Imdecode(imageBytes, ImreadModes.Color, mat);

        using var gray = new Mat();
        CvInvoke.CvtColor(mat, gray, ColorConversion.Bgr2Gray);

        using var binary = new Mat();
        CvInvoke.Threshold(gray, binary, 250, 255, ThresholdType.BinaryInv); // Higher threshold for cleaner separation

        using var kernel = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(kernelSize, kernelSize), new Point(-1, -1));
        CvInvoke.MorphologyEx(binary, binary, MorphOp.Close, kernel, new Point(-1, -1), 1, BorderType.Default, new MCvScalar());

        using var labels = new Mat();
        using var stats = new Mat();
        using var centroids = new Mat();
        int numLabels = CvInvoke.ConnectedComponentsWithStats(binary, labels, stats, centroids);

        var diagrams = new List<DiagramBounds>();
        var imageWidth = mat.Width;
        var imageHeight = mat.Height;

        for (int i = 1; i < numLabels; i++)
        {
            var row = stats.Row(i);
            var data = new int[5];
            Marshal.Copy(row.DataPointer, data, 0, 5);

            int x = data[0];
            int y = data[1];
            int w = data[2];
            int h = data[3];
            int area = data[4];

            double aspectRatio = (double)w / h;

            bool isValidSize = w >= minWidth && h >= minHeight && w < imageWidth * 0.5 && h < imageHeight * 0.4;
            bool hasGoodArea = area > 10000 && area < (imageWidth * imageHeight * 0.25);
            bool notAtEdge = x > 80 && y > 80 && (x + w) < (imageWidth - 80) && (y + h) < (imageHeight - 80);
            bool isCompact = aspectRatio >= 0.2 && aspectRatio <= 4.0;

            // Reject text areas: high aspect ratio (wide and short)
            bool isTextArea = aspectRatio > 2.8;

            if (isValidSize && hasGoodArea && notAtEdge && isCompact && !isTextArea)
            {
                diagrams.Add(new DiagramBounds
                {
                    X = x,
                    Y = y,
                    Width = w,
                    Height = h,
                    XPercent = (double)x / imageWidth * 100,
                    YPercent = (double)y / imageHeight * 100,
                    WidthPercent = (double)w / imageWidth * 100,
                    HeightPercent = (double)h / imageHeight * 100
                });
            }
        }

        return diagrams.OrderBy(d => d.Y).ToList();
    }
}

public class DiagramBounds
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public double XPercent { get; set; }
    public double YPercent { get; set; }
    public double WidthPercent { get; set; }
    public double HeightPercent { get; set; }
}

