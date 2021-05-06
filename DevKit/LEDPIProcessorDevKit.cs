using LEDPiLib;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace LEDPiDevKit
{
    class LEDPIProcessorDevKit : LEDPIProcessorBase
    {
        private static Image<Rgba32> _canvas = new Image<Rgba32>(64, 64);

        public LEDPIProcessorDevKit(Image<Rgba32> canvas) :base()
        {
            _canvas = canvas;
            processor = Process;
        }

        static bool Process(Image<Rgba32> currentImage)
        {
            Image<Rgba32> cloneImage;

            lock (currentImage)
            {
                cloneImage = currentImage.CloneAs<Rgba32>();
                cloneImage.Mutate(x => x
                    .Resize(_canvas.Width, _canvas.Height));

                for (int y = 0; y < _canvas.Height; y++)
                {
                    for (int x = 0; x < _canvas.Width; x++)
                    {
                        _canvas[x, y] = cloneImage[x, y];
                    }
                }
            }

            return true;
        }
    }
}
