using System;
using System.Collections.Generic;
using System.Linq;
using LEDPiLib.DataItems;
using LEDPiLib.Modules.Model;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using static LEDPiLib.LEDPIProcessorBase;

namespace LEDPiLib.Modules
{
    [LEDModule(LEDModules.MandelbrotSet)]
    public class LEDMandelbrotSetModule : LEDEngine3DModuleBase
    {
        // Maximum number of iterations:
        int maxIt = 0;
        int[] maxIts = new[] { 250, 500, 1000, 2000, 4000, 8000, 16000, 32000 };

        private static readonly List<List<Vector2D>> startPositions = new List<List<Vector2D>>()
        {
            new List<Vector2D>(){new Vector2D(-2.45001f, -1.74958f), new Vector2D(-2.45001f + 3f, -1.74958f + 3f)},
            new List<Vector2D>(){new Vector2D(0.004262943452388759f, -0.8573139509059547f), new Vector2D(0.02764703388273181f, -0.8397266853043609f) },
            new List<Vector2D>(){new Vector2D(-1.8117365482139756f, -0.03578317143893926f), new Vector2D(-1.7350283152532877f, 0.02322876542393005f) },
            new List<Vector2D>(){new Vector2D(-1.8642935976039046f, -0.038465499230750225f), new Vector2D(-1.70746920437141f, 0.07492689029625492f) },
            new List<Vector2D>(){new Vector2D(-2.812041983604565f, -1.0296090520405379f), new Vector2D(1.6876367405584882f, 2.3152433501860856f) },
        };
        
        double scale;
        double posX1;
        double posX2;
        double posY1;
        double posY2;


        private bool useCamera = false;

        public LEDMandelbrotSetModule(ModuleConfiguration moduleConfiguration) : base(moduleConfiguration, 2.5f, 30)
        {
            useCamera = true; // moduleConfiguration.OneTime;

            int startPosition = new Random().Next(0, startPositions.Count - 1);
            posX1 = startPositions[startPosition].First().vector.X;
            posY1 = startPositions[startPosition].First().vector.Y;
            posX2 = startPositions[startPosition].Last().vector.X;
            posY2 = startPositions[startPosition].Last().vector.Y;

            scale = (posX2 - posX1) / renderWidth;
        }

        protected override bool completedRun()
        {
            return false;
        }


        protected override Image<Rgba32> RunInternal()
        {
            image = new Image<Rgba32>(renderWidth, renderHeight);

            // Mandelbrot and Julia sets always have their
            // minimum along the edge of the field, so there
            // is a much smaller area to search.
            double minIt = maxIts[maxIt];

            for (var x = 0; x < renderWidth; ++x)
            {
                var it1 = iterate(x, 0);
                var it2 = iterate(x, renderHeight - 1);
                minIt = Math.Min(minIt, Math.Min(it1, it2));
            }

            for (var y = 0; y < renderHeight; ++y)
            {
                var it1 = iterate(0, y);
                var it2 = iterate(renderWidth - 1, y);
                minIt = Math.Min(minIt, Math.Min(it1, it2));
            }

            --minIt;

            // Draw fractal:
            for (var y = 0; y < renderHeight; ++y)
            {
                var row = image.GetPixelRowSpan(y);

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

            if (useCamera)
            {
                var dx = 0.02f * (posX2 - posX1);
                var dy = 0.02f * (posY2 - posY1);
                posX1 += dx;
                posX2 -= dx;
                posY1 += dy;
                posY2 -= dy;

                scale = (posX2 - posX1) / renderWidth;
            }

            return image;
        }

		private double iterate(double x, double y)
		{
            double x0 = x * scale + posX1;
            double y0 = y * scale + posY1;  // Multiply by -1 to flip vertically

            double i = 0.0f;
            double j = 0.0f;
			double it = 0;

            while (i * i + j * j <= 4.0f && it < maxIts[maxIt])
            {
                var i2 = i * i - j * j + x0;
                j = 2.0f * i * j + y0;
                i = i2;
                ++it;
            }

            //for (var k = 0; k < 3; ++k)
            //{
            //    var i2 = i * i - j * j + x0;
            //    j = 2.0 * i * j + y0;
            //    i = i2;
            //    ++it;
            //}

            //it += 1.0 - Math.Log(Math.Log(Math.Sqrt(i * i + j * j))) / Math.Log(2.0);
            ////it += 2 - Math.Log(i * i + j * j) / Math.Log(2);

            return it;
		}
	}
}
