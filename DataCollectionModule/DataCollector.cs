using System.Timers;
using VRCFaceTracking;
using Timer = System.Timers.Timer;

namespace DataCollectionModule
{
    public class DataCollector : ExtTrackingModule
    {
        private readonly StreamWriter _writer;
        private readonly CsvConfig _config;
        private Timer? _collectionTimer;
        

        public DataCollector(CsvConfig config)
        {
            _config = config;
            _writer = new StreamWriter(_config.FilePath, append: true);
        }
        
        public override (bool SupportsEye, bool SupportsExpression) Supported => (true, true);

        public override (bool eyeSuccess, bool expressionSuccess) Initialize(bool eyeAvailable, bool expressionAvailable)
        {
            if (!eyeAvailable && !expressionAvailable)
                return (false, false);

            _collectionTimer = new Timer(_config.CollectionInterval * 1000);
            _collectionTimer.Elapsed += CollectData;
            _collectionTimer.Start();

            return Supported;
        }

        private void CollectData(object? sender, ElapsedEventArgs e)
        {
            try
            {
                var expressions = UnifiedTracking.Data.Shapes;
                var eyeImageData = UnifiedTracking.EyeImageData;
                var lipImageData = UnifiedTracking.LipImageData;
                
                foreach (var key in _config.ExpressionFilter)
                {
                    _writer.Write($"{nameof(key)}, {expressions[(int)key].Weight},");
                }
                
                if (_config.CaptureEyeImages && eyeImageData.SupportsImage)
                {
                    _writer.Write($"EyeImageX,{eyeImageData.ImageSize.x},");
                    _writer.Write($"EyeImageY,{eyeImageData.ImageSize.y},");
                    _writer.Write($"EyeImageData,{Convert.ToBase64String(eyeImageData.ImageData)},");
                }

                if (_config.CaptureFaceImages && lipImageData.SupportsImage)
                {
                    _writer.Write($"FaceImageX,{lipImageData.ImageSize.x},");
                    _writer.Write($"FaceImageY,{lipImageData.ImageSize.y},");
                    _writer.Write($"FaceImageData,{Convert.ToBase64String(lipImageData.ImageData)},");
                }

                _writer.WriteLine();
                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error collecting data: {ex.Message}");
            }
        }

        public override void Update()
        {
            // Not used directly as data collection is timer-driven.
        }

        public override void Teardown()
        {
            _collectionTimer?.Stop();
            _collectionTimer?.Dispose();
            _writer.Dispose();
        }
    }
}
