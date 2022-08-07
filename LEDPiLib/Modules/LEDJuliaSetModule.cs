using System;
using LEDPiLib.DataItems;
using LEDPiLib.Modules.Helper;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using static LEDPiLib.LEDPIProcessorBase;

namespace LEDPiLib.Modules
{
    [LEDModule(LEDModules.JuliaSet)]
    public class LEDJuliaSetModule : LEDEngine3DModuleBase
    {
        // Maximum number of iterations:
        private const int maxIt = 1;
        private readonly int[] maxIts = new[] { 250, 500, 1000, 2000, 4000, 8000, 16000, 32000 };
        private readonly double scale;
        private readonly double posX1;
        private readonly double posY1;

        private double angle;

        // Complex constant for Julia set:
        private double jsX;
        private double jsY;

        public LEDJuliaSetModule(ModuleConfiguration moduleConfiguration) : base(moduleConfiguration, 2f, 30)
        {
            posX1 = -2.0f;
            posY1 = -1.5f;
            var posX2 = posX1 + 4f;

            scale = (posX2 - posX1) / renderWidth;

            angle = MathHelper.GlobalRandom().NextDouble();
        }

        protected override bool completedRun()
        {
            return false;
        }


        protected override Image<Rgba32> RunInternal()
        {
            image = GetNewImage();

            jsX = Math.Cos(angle * 3.213);//sin(angle);
            jsY = Math.Sin(angle);

            // Mandelbrot and Julia sets always have their
            // minimum along the edge of the field, so there
            // is a much smaller area to search.
            var minIt = 0;

            // Draw fractal:
            image.ProcessPixelRows(accessor =>
            {
                for (var y = 0; y < renderHeight; ++y)
                {
                    var row = accessor.GetRowSpan(y);

                    for (var x = 0; x < renderWidth; ++x)
                    {
                        var it = iterate(x, y);
                        //var index = 4 * (x + y * width);

                        if (it < maxIts[maxIt])
                        {
                            double colour = Math.Floor(255.0 * Math.Log(it - minIt) / Math.Log(maxIts[maxIt] - minIt));

                            row[x] = Colors[Convert.ToInt32(colour)];
                        }
                        else
                        {
                            row[x] = Color.Black;
                        }
                    }
                }
            });

            angle += 0.0075;

            return image;
        }

		private int iterate(int x, int y)
		{
            double x0 = x * scale + posX1;
            double y0 = y * scale + posY1;  // Multiply by -1 to flip vertically

            double i = 0.0f;
            double j = 0.0f;
			int it = 0;

            while (i + j <= 16.0 && it < maxIts[maxIt])
            {
                i = x0 * x0;
                j = y0 * y0;
                var xx = i - j;
                var yy = (2.0 * x0 * y0);
                x0 = xx + jsX;
                y0 = yy + jsY;
                ++it;
            }

            return it;
		}
	}
}
