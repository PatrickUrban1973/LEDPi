using System;
using System.Diagnostics;
using System.Threading;
using LEDPiLib.DataItems;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using static LEDPiLib.LEDPIProcessorBase;

namespace LEDPiLib.Modules
{
    [LEDModule(LEDModules.Gif)]
    public class LEDShowGifModule : ModuleBase
    {
        private string _fileName;
        private Image<Rgba32> _display;
        private Stopwatch stopwatch = new Stopwatch();
        private Image<Rgba32> _currentPicture = null;
        private int _nextFrame;

        public LEDShowGifModule(ModuleConfiguration moduleConfiguration) : base(moduleConfiguration)
        {
            _fileName = moduleConfiguration.Parameter;
            _nextFrame = 0;
        }

        protected override bool completedRun()
        {
            return false;
        }

        protected override Image<Rgba32> RunInternal()
        {
            if (_currentPicture == null)
            {
                _currentPicture = Image.Load(_fileName).CloneAs<Rgba32>();
                _currentPicture.Mutate(c => c.Resize(LEDPIProcessorBase.LEDWidth, LEDPIProcessorBase.LEDHeight));
            }

            if (!stopwatch.IsRunning)
            {
                _display = _currentPicture.Clone();

                var frame = _currentPicture.Frames[_nextFrame++];
                Thread.Sleep(new TimeSpan(0, 0, 0, 0, frame.Metadata.GetGifMetadata().FrameDelay * 10));

                for (int y = 0; y < frame.Height; y++)
                {
                    var row = frame.GetPixelRowSpan(y);

                    for (int x = 0; x < frame.Width; x++)
                    {
                        _display.GetPixelRowSpan(y)[x] = row[x];
                    }
                }
            }

            if (_nextFrame >= _currentPicture.Frames.Count)
                _nextFrame = 0;

            return _display;
        }
    }
}
