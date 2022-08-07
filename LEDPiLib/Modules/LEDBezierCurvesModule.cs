using System;
using System.Collections.Generic;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using LEDPiLib.DataItems;
using LEDPiLib.Modules.Helper;
using LEDPiLib.Modules.Model;
using LEDPiLib.Modules.Model.BezierCurves;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;
using static LEDPiLib.LEDPIProcessorBase;
using System.Numerics;

namespace LEDPiLib.Modules
{
    [LEDModule(LEDModules.BezierCurves)]
    public class LEDBezierCurvesModule : ModuleBase
    {
        private readonly List<BezierCurvesParticle> particles = new List<BezierCurvesParticle>();
        private const float delta = 0.03f;

        public LEDBezierCurvesModule(ModuleConfiguration moduleConfiguration) : base(moduleConfiguration, 2f)
        {
            BezierCurvesParticle.RenderHeight = renderHeight;
            BezierCurvesParticle.RenderWidth = renderWidth;

            particles.Add(new BezierCurvesParticle(0, renderHeight / 2f));
            particles.Add(new BezierCurvesParticle(renderWidth / 4f, 0));
            particles.Add(new BezierCurvesParticle((renderWidth * 3) / 4f, renderHeight));
            particles.Add(new BezierCurvesParticle(renderWidth, renderHeight / 2f));
        }

        protected override bool completedRun()
        {
            return false;
        }

        protected override Image<Rgba32> RunInternal()
        {
            Image<Rgba32> image = GetNewImage();

            particles[1].Update();
            particles[2].Update();

            for (float t = 0; t <= 1.00001; t += delta)
            {
                cubic(image, particles[0], particles[1], particles[2], particles[3], t);
            }

            return image;
        }

        private void cubic(Image<Rgba32> image, BezierCurvesParticle p0, BezierCurvesParticle p1,
            BezierCurvesParticle p2, BezierCurvesParticle p3, float t)
        {
            Vector2 v1 = quadratic(image, p0, p1, p2, t);
            Vector2 v2 = quadratic(image, p1, p2, p3, t);

            image.Mutate(c => c.DrawLines(Colors[Convert.ToInt32(MathHelper.Map(t, 0, 1, 0, 255))], 1f,
                new[] { new PointF(v1.X, v1.Y), new PointF(v2.X, v2.Y) }));
        }

        private Vector2 quadratic(Image<Rgba32> image, BezierCurvesParticle p0, BezierCurvesParticle p1,
            BezierCurvesParticle p2, float t)
        {
            float x1 = MathHelper.Lerp(p0.X, p1.X, t);
            float y1 = MathHelper.Lerp(p0.Y, p1.Y, t);
            float x2 = MathHelper.Lerp(p1.X, p2.X, t);
            float y2 = MathHelper.Lerp(p1.Y, p2.Y, t);
            float x = MathHelper.Lerp(x1, x2, t);
            float y = MathHelper.Lerp(y1, y2, t);

            image.Mutate(c => c.DrawLines(Colors[Convert.ToInt32(MathHelper.Map(t, 0, 1, 0, 255))], 1f,
                new[] { new PointF(x1, y1), new PointF(x2, y2) }));
            return new Vector2(x, y);
        }
    }
}
