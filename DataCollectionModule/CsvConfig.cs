using VRCFaceTracking.Core.Params.Expressions;

namespace DataCollectionModule;

public class CsvConfig
{
    public string FilePath { get; set; } = "default.csv";
    public double CollectionInterval { get; set; } = 1.0; // In seconds
    public HashSet<UnifiedExpressions> ExpressionFilter { get; set; } = new();
    public bool CaptureEyeImages { get; set; } = true;
    public bool CaptureFaceImages { get; set; } = true;
}