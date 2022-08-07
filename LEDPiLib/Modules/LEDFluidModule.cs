using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using LEDPiLib.DataItems;
using LEDPiLib.Modules.Helper;
using LEDPiLib.Modules.Model.Fluid;
using static LEDPiLib.LEDPIProcessorBase;

namespace LEDPiLib.Modules
{
    [LEDModule(LEDModules.Fluid)]
    public class LEDFluidModule : ModuleBase
    {
        private readonly Fluid _fluid;
        private const int dropFrames = 10;
        private readonly int cx;
        private readonly int cy;
        private int currentFrame;

        public LEDFluidModule(ModuleConfiguration moduleConfiguration) : base(moduleConfiguration, 1f, 30)
        {
            _fluid = new Fluid(renderWidth, 0.2f, 0, 0.0001f);
            cx = Convert.ToInt32(0.5 * renderWidth / renderOffset);
            cy = Convert.ToInt32(0.5 * renderWidth / renderOffset);
        }

        protected override bool completedRun()
        {
            return false;
        }

        protected override Image<Rgba32> RunInternal()
        {
            Image<Rgba32> image = GetNewImage();

            if (currentFrame % dropFrames == 0)
            {
                for (int i = -1; i <= 1; i++)
                {
                    for (int j = -1; j <= 1; j++)
                    {
                        _fluid.addDensity(cx + i, cy + j, MathHelper.GlobalRandom().Next(50, 150 + 1));
                    }
                }

                for (int i = 0; i < 2; i++)
                {
                    float angle = (float)(((MathHelper.GlobalRandom().NextDouble()) * 360 % 360) * Math.PI * 2 * 2);
                    PointF v = pointOnCircle(150f, angle);
                    _fluid.addVelocity(cx, cy, v.X, v.Y);
                }
            }

            currentFrame++;
            _fluid.step();

            image.ProcessPixelRows(accessor =>
            {
                for (int j = 0; j < image.Height; j++)
                {
                    var row = accessor.GetRowSpan(j);

                    for (int i = 0; i < image.Width; i++)
                    {
                        float d = _fluid.density[Fluid.IX(i, j)];
                        ref Rgba32 pixel = ref row[i];
                        pixel = new Rgba32(0, Convert.ToByte((d * 5) % 255), 0);
                    }
                }
            });

            return image;
        }


        private PointF pointOnCircle(float radius, float angleInDegrees)
        {
            angleInDegrees -= 90;
            // Convert from degrees to radians via multiplication by PI/180        
            float x = (float) (radius * Math.Cos(angleInDegrees * Math.PI / 180F)) +
                      (renderWidth / 2f);
            float y = (float) (radius * Math.Sin(angleInDegrees * Math.PI / 180F)) +
                      (renderHeight / 2f);

            return new PointF(x, y);
        }

    }
}
