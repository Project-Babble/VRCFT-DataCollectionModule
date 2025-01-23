using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Reflection;
using System.Text;
using VRCFaceTracking;
using VRCFaceTracking.Core.Params.Expressions;

namespace DataCollectionModule;

public class DataCollector : ExtTrackingModule
{
    private static readonly UnifiedExpressions[] AllExpressions = Enum.GetValues<UnifiedExpressions>();

    private CsvConfig _config;
    private StreamWriter? _writer;
    private Thread? _collectionThread;
    private StringBuilder? _sb;
    private bool _isRunning = true;
    
    public override (bool SupportsEye, bool SupportsExpression) Supported => (true, true);

    public override (bool eyeSuccess, bool expressionSuccess) Initialize(bool eyeAvailable, bool expressionAvailable)
    {
        Logger.LogInformation("Initializing DataCollector module...");

        if (!eyeAvailable && !expressionAvailable)
        {
            Logger.LogWarning("Both eye and expression tracking are unavailable.");
            return (false, false);
        }

        var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        var configPath = Path.Combine(path, "DataCollectionConfig.json");

        if (!File.Exists(configPath))
        {
            Logger.LogInformation("DataCollectionConfig.json not found. Creating default configuration.");
            var emptyConfig = new CsvConfig();
            var emptyConfigJson = JsonConvert.SerializeObject(emptyConfig);
            File.WriteAllText(configPath, emptyConfigJson);
        }

        _sb = new StringBuilder();
        _config = JsonConvert.DeserializeObject<CsvConfig>(File.ReadAllText(configPath))!;
        _writer = new StreamWriter(Path.Combine(path, _config.FilePath), _config.Append);
        _writer.WriteLine(string.Join(',', AllExpressions) + ",EyeImageX,EyeImageY,EyeImageData,FaceImageX,FaceImageY,FaceImageData");

        List<Stream> staticImages = new List<Stream>();
        Stream resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("DataCollectionModule.Spreadsheet.png")!;
        staticImages.Add(resourceStream);

        ModuleInformation = new ModuleMetadata
        {
            Name = "Data Collection Module",
            StaticImages = staticImages
        };

        _isRunning = true;
        _collectionThread = new Thread(CollectDataLoop);
        _collectionThread.Start();

        Logger.LogInformation("DataCollector module initialized successfully.");
        return Supported;
    }

    private void CollectDataLoop()
    {
        Logger.LogInformation("Data collection loop started.");
        while (_isRunning)
        {
            CollectData();
            Thread.Sleep(_config.CollectionInterval);
        }
    }

    private void CollectData()
    {
        var expressions = UnifiedTracking.Data.Shapes;
        var eyeImage = UnifiedTracking.EyeImageData;
        var faceImage = UnifiedTracking.LipImageData;

        _sb?.Clear();
        if (_config.ExpressionFilter.Count > 0)
        {
            foreach (var key in _config.ExpressionFilter)
            {
                _sb?.Append($"{expressions[(int)key].Weight},");
            }
        }
        else
        {
            foreach (var key in AllExpressions)
            {
                _sb?.Append($"{expressions[(int)key].Weight},");
            }
        }

        if (_config.CaptureEyeImages && eyeImage.SupportsImage)
        {
            _sb?.Append($"{eyeImage.ImageSize.x},{eyeImage.ImageSize.y},{Convert.ToBase64String(eyeImage.ImageData)}");
        }
        else
        {
            _sb?.Append("-1,-1,-1,");
        }

        if (_config.CaptureFaceImages && faceImage.SupportsImage)
        {
            _sb?.Append($"{faceImage.ImageSize.x},{faceImage.ImageSize.y},{Convert.ToBase64String(faceImage.ImageData)},");
        }
        else
        {
            _sb?.Append("-1,-1,-1,");
        }

        // Write the collected data as a single line
        _writer?.WriteLine(_sb?.ToString().TrimEnd(','));
    }

    public override void Update()
    {
        // Not used directly as data collection is thread-driven.
    }

    public override void Teardown()
    {
        Logger.LogInformation("Tearing down DataCollector module...");
        _isRunning = false;
        _collectionThread?.Join();
        _writer?.Dispose();
        Logger.LogInformation("DataCollector module teardown complete.");
    }
}
