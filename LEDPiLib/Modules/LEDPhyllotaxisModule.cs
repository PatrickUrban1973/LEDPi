using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using LEDPiLib.DataItems;
using LEDPiLib.Modules.Helper;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;
using static LEDPiLib.LEDPIProcessorBase;

namespace LEDPiLib.Modules
{
    [LEDModule(LEDModules.Phyllotaxis)]
    public class LEDPhyllotaxisModule : ModuleBase
    {
        private int n = 0;
        private int c = 3;
        private int start = 0;
        private readonly double degrees;

        public LEDPhyllotaxisModule(ModuleConfiguration moduleConfiguration) : base(moduleConfiguration, 2f, 0)
        {
            degrees = 137 + new Random().NextDouble();
        }

        protected override bool completedRun()
        {
            return false;
        }

        protected override Image<Rgba32> RunInternal()
        {
            Image<Rgba32> image = new Image<Rgba32>(renderWidth, renderHeight);
            SetBackgroundColor(image);

            for (var i = 0; i < n; i++)
            {
                double a = i * degrees;
                double r = c * Math.Sqrt(i);
                float x = Convert.ToSingle(r * Math.Cos(a));
                float y = Convert.ToSingle(r * Math.Sin(a));
                float hu = Convert.ToSingle(Math.Sin(start + i * 0.5));
                int colorByte =  Convert.ToInt32(MathHelper.Map(hu, -1, 1, 0, 254));
                image.Mutate(c => c.Fill(Colors[colorByte],
                    new ComplexPolygon(new EllipsePolygon(new PointF((renderWidth / 2) + x, (renderHeight / 2) + y),
                        2))));
            }

            n += 5;
            start += 5;
            
            return image;
        }
    }
}
