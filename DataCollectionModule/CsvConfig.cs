using VRCFaceTracking.Core.Params.Expressions;

namespace DataCollectionModule;

public class CsvConfig
{
    public string FilePath { get; set; } = "default.csv";
    public int CollectionInterval { get; set; } = 200; // In milliseconds
    public HashSet<UnifiedExpressions> ExpressionFilter { get; set; } = new();
    public bool CaptureEyeImages { get; set; } = true;
    public bool CaptureFaceImages { get; set; } = true;
}