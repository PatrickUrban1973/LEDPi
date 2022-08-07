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
        private int n;
        private const int c = 3;
        private int start;
        private readonly double degrees;

        public LEDPhyllotaxisModule(ModuleConfiguration moduleConfiguration) : base(moduleConfiguration, 2f)
        {
            degrees = 137 + MathHelper.GlobalRandom().NextDouble();
        }

        protected override bool completedRun()
        {
            return false;
        }

        protected override Image<Rgba32> RunInternal()
        {
            Image<Rgba32> image = GetNewImage();

            for (var i = 0; i < n; i++)
            {
                double a = i * degrees;
                double r = c * Math.Sqrt(i);
                float x = (float)(r * Math.Cos(a));
                float y = (float)(r * Math.Sin(a));
                float hu = (float)(Math.Sin(start + i * 0.5));
                int colorByte =  Convert.ToInt32(MathHelper.Map(hu, -1, 1, 0, 254));
                image.Mutate(b => b.Fill(Colors[colorByte],
                    new ComplexPolygon(new EllipsePolygon(new PointF((renderWidth / 2) + x, (renderHeight / 2) + y),
                        2))));
            }

            n += 5;
            start += 5;
            
            return image;
        }
    }
}
