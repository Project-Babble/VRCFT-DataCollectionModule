using Newtonsoft.Json;
using System.Reflection;
using System.Text;
using VRCFaceTracking;
using VRCFaceTracking.Core.Params.Expressions;

namespace DataCollectionModule;

public class DataCollector : ExtTrackingModule
{
    private CsvConfig _config;
    private StreamWriter? _writer;
    private Thread? _collectionThread;
    private bool _isRunning = true;

    private static readonly UnifiedExpressions[] AllExpressions = Enum.GetValues<UnifiedExpressions>();
    private static readonly StringBuilder sb = new StringBuilder();

    public override (bool SupportsEye, bool SupportsExpression) Supported => (true, true);

    public override (bool eyeSuccess, bool expressionSuccess) Initialize(bool eyeAvailable, bool expressionAvailable)
    {
        if (!eyeAvailable && !expressionAvailable)
            return (false, false);

        var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        var ConfigName = Path.Combine(path, "DataCollectionConfig.json");

        if (!File.Exists(ConfigName))
        {
            var emptyConfig = new CsvConfig();
            var emptyConfigJson = JsonConvert.SerializeObject(emptyConfig);
            File.WriteAllText(ConfigName, emptyConfigJson);
        }

        _config = JsonConvert.DeserializeObject<CsvConfig>(File.ReadAllText(ConfigName))!;
        _writer = new StreamWriter(Path.Combine(path, _config.FilePath), _config.Append);
        _writer?.WriteLine(string.Join(',', AllExpressions) + ",EyeImageX,EyeImageY,EyeImageData,FaceImageX,FaceImageY,FaceImageData");

        List<Stream> list = new List<Stream>();
        Assembly executingAssembly = Assembly.GetExecutingAssembly();
        Stream manifestResourceStream = executingAssembly.GetManifestResourceStream("DataCollectionModule.Spreadsheet.png")!;
        list.Add(manifestResourceStream);
        ModuleInformation = new ModuleMetadata
        {
            Name = "Data Collection Module",
            StaticImages = list
        };

        _isRunning = true;
        _collectionThread = new Thread(CollectDataLoop);
        _collectionThread.Start();

        return Supported;
    }

    private void CollectDataLoop()
    {
        while (_isRunning)
        {
            try
            {
                CollectData();
                Thread.Sleep(_config.CollectionInterval);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in data collection loop: {ex.Message}");
            }
        }
    }

    private void CollectData()
    {
        var expressions = UnifiedTracking.Data.Shapes;
        var eyeImage = UnifiedTracking.EyeImageData;
        var faceImage = UnifiedTracking.LipImageData;

        try
        {
            sb.Clear();
            if (_config.ExpressionFilter.Count > 0)
            {
                foreach (var key in _config.ExpressionFilter)
                {
                    sb.Append($"{expressions[(int)key].Weight},");
                }
            }
            else
            {
                foreach (var key in AllExpressions)
                {
                    sb.Append($"{expressions[(int)key].Weight},");
                }
            }

            if (_config.CaptureEyeImages && eyeImage.SupportsImage)
            {
                sb.Append($"{eyeImage.ImageSize.x}");
                sb.Append($"{eyeImage.ImageSize.y}");
                sb.Append($"{Convert.ToBase64String(eyeImage.ImageData)}");
            }
            else
            {
                sb.Append("-1,");
                sb.Append("-1,");
                sb.Append("-1,");
            }

            if (_config.CaptureFaceImages && faceImage.SupportsImage)
            {
                sb.Append($"{eyeImage.ImageSize.x}");
                sb.Append($"{eyeImage.ImageSize.y}");
                sb.Append($"{Convert.ToBase64String(faceImage.ImageData)},");
            }
            else
            {
                sb.Append("-1,");
                sb.Append("-1,");
                sb.Append("-1,");
            }

            // Write the collected data as a single line
            _writer?.WriteLine(sb.ToString().TrimEnd(','));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error collecting data: {ex.Message}");
        }
    }

    public override void Update()
    {
        // Not used directly as data collection is thread-driven.
    }

    public override void Teardown()
    {
        _isRunning = false;
        _collectionThread?.Join();
        _writer?.Dispose();
    }
}
