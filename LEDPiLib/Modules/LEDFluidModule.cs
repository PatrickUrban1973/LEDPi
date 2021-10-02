using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using LEDPiLib.DataItems;
using LEDPiLib.Modules.Model;
using static LEDPiLib.LEDPIProcessorBase;

namespace LEDPiLib.Modules
{
    [LEDModule(LEDModules.Fluid)]
    public class LEDFluidModule : ModuleBase
    {
        private Fluid _fluid;
        private readonly int N ;
        private const int dropFrames = 10;
        private int currentFrame = 0;

        private Random _random = new Random();

        public LEDFluidModule(ModuleConfiguration moduleConfiguration) : base(moduleConfiguration, 1f, 30)
        {
            N = renderWidth;
            _fluid = new Fluid(N, 0.2f, 0, 0.0001f);
        }

        protected override bool completedRun()
        {
            return false;
        }

        protected override Image<Rgba32> RunInternal()
        {
            Image<Rgba32> image = new Image<Rgba32>(N, N);
            SetBackgroundColor(image);

            if (currentFrame % dropFrames == 0)
            {
                int cx = Convert.ToInt32(0.5 * N / renderOffset);
                int cy = Convert.ToInt32(0.5 * N / renderOffset);
                for (int i = -1; i <= 1; i++)
                {
                    for (int j = -1; j <= 1; j++)
                    {
                        _fluid.addDensity(cx + i, cy + j, _random.Next(50, 150));
                    }
                }

                for (int i = 0; i < 2; i++)
                {
                    float angle = Convert.ToSingle(((_random.NextDouble()) * 360 % 360) * Math.PI * 2 * 2);
                    PointF v = pointOnCircle(150f, angle);
                    _fluid.addVelocity(cx, cy, v.X, v.Y);
                }
            }

            currentFrame++;
            _fluid.step();

            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    float d = _fluid.density[Fluid.IX(i, j)];
                    image.GetPixelRowSpan(j)[i] = new Rgba32(0, Convert.ToByte((d * 5) % 255), 0 );
                }
            }

            return image;
        }


        private PointF pointOnCircle(float radius, float angleInDegrees)
        {
            angleInDegrees -= 90;
            // Convert from degrees to radians via multiplication by PI/180        
            float x = (float) (radius * Math.Cos(angleInDegrees * Math.PI / 180F)) +
                      Convert.ToSingle(renderWidth / 2);
            float y = (float) (radius * Math.Sin(angleInDegrees * Math.PI / 180F)) +
                      Convert.ToSingle(renderHeight / 2);

            return new PointF(x, y);
        }

    }
}
