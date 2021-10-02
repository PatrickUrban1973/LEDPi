using System;
using System.Collections.Generic;
using LEDPiLib.DataItems;
using LEDPiLib.Modules.Helper;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using static LEDPiLib.LEDPIProcessorBase;

namespace LEDPiLib.Modules
{
    [LEDModule(LEDModules.Rain)]
    public class LEDMatrixRainModule : ModuleBase
    {
        private class Point
        {
            public Point(int x, int y)
            {
                this.x = x;
                this.y = y;
            }
            public int x;
            public int y;
            public bool recycled;
        }

        const int COLOR_STEP = 3;
        const int FRAME_STEP = 2;

        int frame = 0;
        List<Point> points = new List<Point>();
        Stack<Point> recycled = new Stack<Point>();

        public LEDMatrixRainModule(ModuleConfiguration moduleConfiguration) : base(moduleConfiguration, 1f, 10)
        {
        }

        protected override bool completedRun()
        {
            return false;
        }

        protected override Image<Rgba32> RunInternal()
        {
            var rnd = new Random();
            Image<Rgba32> image = new Image<Rgba32>(renderWidth, renderHeight);

            if (frame % FRAME_STEP == 0)
            {
                if (recycled.Count == 0)
                    points.Add(new Point(rnd.Next(0, renderWidth - 1), 0));
                else
                {
                    var point = recycled.Pop();
                    point.x = rnd.Next(0, renderWidth - 1);
                    point.y = 0;
                    point.recycled = false;
                }
            }

            SetBackgroundColor(image);

            frame++;

            foreach (var point in points)
            {
                if (!point.recycled)
                {
                    point.y++;

                    if (point.y - renderHeight > image.Height)
                    {
                        point.recycled = true;
                        recycled.Push(point);
                    }

                    for (var i = 0; i < renderHeight; i++)
                    {
                        if (point.y - i > 0 && point.y - i < renderHeight)
                        {
                            Rgba32 pixel = image.GetPixelRowSpan(point.y - i)[point.x];

                            pixel.R = 0;
                            pixel.G = Convert.ToByte(
                                MathHelper.Map((renderHeight + 1 - i), 0, renderHeight + 1, 0, 85) * COLOR_STEP);
                            pixel.B = 0;

                            image.GetPixelRowSpan(point.y - i)[point.x] = pixel;
                        }
                    }
                }
            }

            return image;
        }
    }
}
