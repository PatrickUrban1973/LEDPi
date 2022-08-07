using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using LEDPiLib.DataItems;
using LEDPiLib.Modules.Helper;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using static LEDPiLib.LEDPIProcessorBase;

namespace LEDPiLib.Modules
{
    [LEDModule(LEDModules.MandelbrotSet)]
    public class LEDMandelbrotSetModule : LEDEngine3DModuleBase
    {
        // Maximum number of iterations:
        int maxIt = 1250;

        private static readonly List<List<Vector2>> startPositions = new List<List<Vector2>>()
        {
            new List<Vector2>(){new Vector2(-2.45001f, -1.74958f), new Vector2(-2.45001f + 3f, -1.74958f + 3f)},
            new List<Vector2>(){new Vector2(0.004262943452388759f, -0.8573139509059547f), new Vector2(0.02764703388273181f, -0.8397266853043609f) },
            new List<Vector2>(){new Vector2(-1.8117365482139756f, -0.03578317143893926f), new Vector2(-1.7350283152532877f, 0.02322876542393005f) },
            new List<Vector2>(){new Vector2(-1.8642935976039046f, -0.038465499230750225f), new Vector2(-1.70746920437141f, 0.07492689029625492f) },
            new List<Vector2>(){new Vector2(-2.812041983604565f, -1.0296090520405379f), new Vector2(1.6876367405584882f, 2.3152433501860856f) },
        };
        
        private double scale;
        private double posX1;
        private double posX2;
        private double posY1;
        private double posY2;

        private readonly Dictionary<int, Dictionary<int, double>> mapIteration = new Dictionary<int, Dictionary<int, double>>();

        private readonly bool useCamera;

        public LEDMandelbrotSetModule(ModuleConfiguration moduleConfiguration) : base(moduleConfiguration, 1.75f, 30)
        {
            useCamera = true; // moduleConfiguration.OneTime;

            int startPosition = MathHelper.GlobalRandom().Next(0, startPositions.Count);
            posX1 = startPositions[startPosition].First().X;
            posY1 = startPositions[startPosition].First().Y;
            posX2 = startPositions[startPosition].Last().X;
            posY2 = startPositions[startPosition].Last().Y;

            scale = (posX2 - posX1) / renderWidth;
            
            for (var y = 0; y < renderHeight; ++y)
            {
                Dictionary<int, double> subMap = new Dictionary<int, double>();
                mapIteration.Add(y, subMap);
                for (var x = 0; x < renderWidth; ++x)
                {
                    subMap.Add(x, 0);
                }
            }
        }

        protected override bool completedRun()
        {
            return false;
        }


        protected override Image<Rgba32> RunInternal()
        {
            image = GetNewImage();
            
            double minIt = maxIt;

            for (var y = 0; y < renderHeight; ++y)
            {
                Dictionary<int, double> rowMap = mapIteration[y];

                for (var x = 0; x < renderWidth; ++x)
                {

                    var it1 = iterate(x, y);
                    rowMap[x] = it1;
                    minIt = Math.Min(minIt, it1);
                }
            }

            --minIt;

            // Draw fractal:
            image.ProcessPixelRows(accessor =>
            {
                for (var y = 0; y < renderHeight; ++y)
                {
                    var row = accessor.GetRowSpan(y);

                    for (var x = 0; x < renderWidth; ++x)
                    {
                        var it = mapIteration[y][x];

                        if (it < maxIt)
                        {
                            double color = 0;
                            if (Math.Abs(it - minIt) > 0)
                            {
                                color = Math.Floor(255.0 * Math.Log(it - minIt) / Math.Log(maxIt - minIt));
                            }
                            row[x] = Colors[(int) color];
                        }
                        else
                        {
                            row[x] = Color.Black;
                        }
                    }
                }
            });

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

            while (i * i + j * j <= 4.0f && it < maxIt)
            {
                var i2 = i * i - j * j + x0;
                j = 2.0f * i * j + y0;
                i = i2;
                ++it;
            }

            return it;
		}
	}
}
