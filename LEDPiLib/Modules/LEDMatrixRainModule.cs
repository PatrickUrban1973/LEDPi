using System;
using System.Collections.Generic;
using LEDPiLib.DataItems;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
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

        public LEDMatrixRainModule(ModuleConfiguration moduleConfiguration) : base(moduleConfiguration)
        {
        }

        protected override bool completedRun()
        {
            return false;
        }

        protected override Image<Rgba32> Run()
        {
            var rnd = new Random();
            Image<Rgba32> image = new Image<Rgba32>(LEDPIProcessorBase.LEDWidth, LEDPIProcessorBase.LEDHeight);

            if (frame % FRAME_STEP == 0)
            {
                if (recycled.Count == 0)
                    points.Add(new Point(rnd.Next(0, LEDPIProcessorBase.LEDWidth - 1), 0));
                else
                {
                    var point = recycled.Pop();
                    point.x = rnd.Next(0, LEDPIProcessorBase.LEDWidth - 1);
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

                    if (point.y - LEDPIProcessorBase.LEDHeight > image.Height)
                    {
                        point.recycled = true;
                        recycled.Push(point);
                    }

                    for (var i = 0; i < LEDPIProcessorBase.LEDHeight; i++)
                    {
                        if (point.y - i > 0 && point.y - i < LEDPIProcessorBase.LEDHeight)
                        {
                            Rgba32 pixel = image.GetPixelRowSpan(point.y - i)[point.x];

                            pixel.R = 0;
                            pixel.G = Convert.ToByte((LEDPIProcessorBase.LEDHeight + 1 - i) * COLOR_STEP);
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
