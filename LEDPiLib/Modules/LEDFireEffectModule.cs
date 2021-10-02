using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using LEDPiLib.DataItems;
using LEDPiLib.Modules.Helper;
using static LEDPiLib.LEDPIProcessorBase;

namespace LEDPiLib.Modules
{
    [LEDModule(LEDModules.FireEffect)]
    public class LEDFireEffectModule : ModuleBase
    {
        private Dictionary<int, Dictionary<int, float>> buffer1 = new Dictionary<int, Dictionary<int, float>>();
        private Dictionary<int, Dictionary<int, float>> buffer2 = new Dictionary<int, Dictionary<int, float>>();
        private Dictionary<int, Dictionary<int, float>> cooling = new Dictionary<int, Dictionary<int, float>>();
        private float ystart = 0.0f;
        private readonly float randomZ;
        private Perlin perlin = new Perlin();

        public LEDFireEffectModule(ModuleConfiguration moduleConfiguration) : base(moduleConfiguration, 1.5f)
        {
            initDictionary(buffer1);
            initDictionary(buffer2);
            initDictionary(cooling);

            randomZ = Convert.ToSingle(new Random().NextDouble());
        }

        protected override bool completedRun()
        {
            return false;
        }

        protected override Image<Rgba32> RunInternal()
        {
            Image<Rgba32> image = new Image<Rgba32>(renderWidth, renderHeight);
            SetBackgroundColor(image);

            fire(5);
            cool();

            for (int x = 1; x < renderWidth - 1; x++)
            {
                for (int y = 1; y < renderHeight - 1; y++)
                {
                    float c1 = buffer1[y][x + 1];
                    float c2 = buffer1[y][x - 1];
                    float c3 = buffer1[y + 1][x];
                    float c4 = buffer1[y - 1][x];

                    float c5 = cooling[y][x];

                    float newC = 0;

                    int bufC = Convert.ToInt32((c1 + c2 + c3 + c4) * .25);

                    if (bufC >= c5)
                        newC = Convert.ToByte(bufC - c5);

                    buffer2[y - 1][x] = newC;
                }
            }

            for (int y = 0; y < renderHeight; y++)
            {
                var row = image.GetPixelRowSpan(y);

                for (int x = 0; x < renderWidth; x++)
                {
                    row[x] = fire_gradient(buffer2[y][x]);
                    buffer1[y][x] = buffer2[y][x];
                }
            }

            return image;
        }

        private void initDictionary(Dictionary<int, Dictionary<int, float>> dict)
        {
            for (int y = 0; y < renderHeight; y++)
            {
                dict.Add(y, new Dictionary<int, float>());

                for (int x = 0; x < renderWidth; x++)
                {
                    dict[y].Add(x, 0);
                }
            }
        }

        private void cool()
        {
            float xoff = 0.0f; // Start xoff at 0
            float increment = 0.02f;

            // For every x,y coordinate in a 2D space, calculate a noise value and produce a brightness value
            for (int x = 0; x < renderWidth; x++)
            {
                xoff += increment;   // Increment xoff 
                float yoff = ystart;   // For every xoff, start yoff at 0
                for (int y = 0; y < renderHeight; y++)
                {
                    yoff += increment; // Increment yoff

                    // Calculate noise and scale by 255
                    float bright = perlin.perlin(xoff, yoff, randomZ) * 15f;

                    // Try using this line instead
                    //float bright = random(0,255);

                    // Set each pixel onscreen to a grayscale value
                    cooling[y][x]=bright;
                }
            }

            ystart += increment;
        }

        private void fire(int rows)
        {
            for (int j = 0; j < rows; j++)
            {
                int y = renderHeight - (j + 1);

                for (int x = 0; x < renderWidth; x++)
                {
                    buffer1[y][x] = 254;
                }
            }
        }

        private Rgba32 fire_gradient(float bright)
        {
            if (bright == 0)
            {
                return Color.Black;
            }
            else if (bright >= 0 && bright < 25)
            {
                return new Rgba32(Convert.ToByte(MathHelper.Map(bright, 0, 25, 29, 251)),
                    Convert.ToByte(MathHelper.Map(bright, 0, 25, 87, 243)), Convert.ToByte(MathHelper.Map(bright, 0, 25, 118, 206)));
            }
            else if (bright >= 25 && bright < 100)
            {
                return new Rgba32(251, 243, 206);
            }
            else if (bright >= 100 && bright < 200)
            {
                return new Rgba32(Convert.ToByte(MathHelper.Map(bright, 100, 200, 251, 181)),
                    Convert.ToByte(MathHelper.Map(bright, 100, 200, 243, 90)), Convert.ToByte(MathHelper.Map(bright, 100, 200, 206, 9)));
            }
            else
            {
                return new Rgba32(Convert.ToByte(MathHelper.Map(bright, 200, 255, 181, 0)),
                    Convert.ToByte(MathHelper.Map(bright, 200, 255, 90, 0)), Convert.ToByte(MathHelper.Map(bright, 200, 255, 9, 0)));
            }
        }
    }
}
