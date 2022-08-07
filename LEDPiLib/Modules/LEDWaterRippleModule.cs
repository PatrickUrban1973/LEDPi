using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Collections.Generic;
using LEDPiLib.DataItems;
using LEDPiLib.Modules.Helper;
using static LEDPiLib.LEDPIProcessorBase;

namespace LEDPiLib.Modules
{
    [LEDModule(LEDModules.WaterRipple)]
    public class LEDWaterRippleModule : ModuleBase
    {
        private readonly Dictionary<int, Dictionary<int, float>> buffer1 = new Dictionary<int, Dictionary<int, float>>();
        private readonly Dictionary<int, Dictionary<int, float>> buffer2 = new Dictionary<int, Dictionary<int, float>>();

        private readonly float dumplining = 0.9f;
        private int counter;

        public LEDWaterRippleModule(ModuleConfiguration moduleConfiguration) : base(moduleConfiguration, 2f)
        {
            initDictionary(buffer1);
            initDictionary(buffer2);
        }

        protected override bool completedRun()
        {
            return false;
        }

        protected override Image<Rgba32> RunInternal()
        {
            Image<Rgba32> image = GetNewImage();

            if (counter == 0)
            {
                dropRain(1);
                counter = 40;
            }

            for (int x = 1; x < renderWidth - 1; x++)
            {
                for (int y = 1; y < renderHeight - 1; y++)
                {
                    float c1 = buffer1[y][x + 1];
                    float c2 = buffer1[y][x - 1];
                    float c3 = buffer1[y + 1][x];
                    float c4 = buffer1[y - 1][x];

                    float c5 = buffer2[y][x];

                    float newC = ((c1 + c2 + c3 + c4) * .25f) - c5 * dumplining;

                    if (newC < 0)
                        newC = 0;

                    buffer2[y][x] = newC;
                }
            }

            image.ProcessPixelRows(accessor =>
            {
                for (int y = 0; y < renderHeight; y++)
                {
                    var row = accessor.GetRowSpan(y);

                    for (int x = 0; x < renderWidth; x++)
                    {
                        float color = buffer2[y][x];

                        row[x] = new Rgba32(color, color, color);
                        buffer1[y][x] = buffer2[y][x];
                    }
                }
            });

            counter--;
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

        private void dropRain(int drops)
        {
            for(int i = 0; i < drops; i++)
            {
                buffer1[MathHelper.GlobalRandom().Next(1, renderHeight)][MathHelper.GlobalRandom().Next(1, renderWidth)] = 255;
            }
        }
    }
}
