using System;
using System.Collections.Generic;
using LEDPiLib.DataItems;
using LEDPiLib.Modules.Helper;
using LEDPiLib.Modules.Model.Rain;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using static LEDPiLib.LEDPIProcessorBase;

namespace LEDPiLib.Modules
{
    [LEDModule(LEDModules.Rain)]
    public class LEDRainModule : ModuleBase
    {
        private const int COLOR_STEP = 3;
        private const int FRAME_STEP = 2;

        private int frame;
        private readonly List<RainDrop> points = new List<RainDrop>();
        private readonly Stack<RainDrop> recycled = new Stack<RainDrop>();

        public LEDRainModule(ModuleConfiguration moduleConfiguration) : base(moduleConfiguration, 1f, 10)
        {
        }

        protected override bool completedRun()
        {
            return false;
        }

        protected override Image<Rgba32> RunInternal()
        {
            Image<Rgba32> image = GetNewImage();

            if (frame % FRAME_STEP == 0)
            {
                if (recycled.Count == 0)
                    points.Add(new RainDrop(MathHelper.GlobalRandom().Next(0, renderWidth), 0));
                else
                {
                    var point = recycled.Pop();
                    point.X = MathHelper.GlobalRandom().Next(0, renderWidth);
                    point.Y = 0;
                    point.Recycled = false;
                }
            }

            frame++;

            foreach (var point in points)
            {
                if (!point.Recycled)
                {
                    point.Y++;

                    if (point.Y - renderHeight > image.Height)
                    {
                        point.Recycled = true;
                        recycled.Push(point);
                    }

                    image.ProcessPixelRows(accessor =>
                    {
                        for (var i = 0; i < renderHeight; i++)
                        {
                            if (point.Y - i > 0 && point.Y - i < renderHeight)
                            {
                                Rgba32 pixel = accessor.GetRowSpan(point.Y - i)[point.X];

                                pixel.R = 0;
                                pixel.G = Convert.ToByte(
                                    MathHelper.Map((renderHeight + 1 - i), 0, renderHeight + 1, 0, 85) * COLOR_STEP);
                                pixel.B = 0;

                                accessor.GetRowSpan(point.Y - i)[point.X] = pixel;
                            }
                        }
                    });
                }
            }

            return image;
        }
    }
}
