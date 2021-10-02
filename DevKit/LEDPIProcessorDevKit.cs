using LEDPiLib;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace LEDPiDevKit
{
    class LEDPIProcessorDevKit : LEDPIProcessorBase
    {
        private static Image<Rgba32> _canvas = new Image<Rgba32>(LEDWidth, LEDHeight);

        public LEDPIProcessorDevKit(Image<Rgba32> canvas) :base()
        {
            _canvas = canvas;
        }

        protected override void doProcess(Image<Rgba32> image)
        {
            Image<Rgba32> cloneImage;

            cloneImage = image.CloneAs<Rgba32>();
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
    }
}
