using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Numerics;
using LEDPiLib.DataItems;
using LEDPiLib.Modules.Helper;
using static LEDPiLib.LEDPIProcessorBase;
using System.Collections.Generic;
using System.Linq;

namespace LEDPiLib.Modules
{
    [LEDModule(LEDModules.TimesTablesCardioid)]
    public class LEDTimesTablesCardioidModule : ModuleBase
    {
        private const int totalLines = 100;

        private readonly float r;
        private readonly float constFactor;
        private float factor;
        private const float TwoPi = 6.283185f;
        private readonly LEDEngine3D engine3D = new LEDEngine3D();
        private readonly Vector2 offset;

        public LEDTimesTablesCardioidModule(ModuleConfiguration moduleConfiguration) : base(moduleConfiguration, 5f, 25)
        {
            r = (renderHeight / 2 );
            offset = new Vector2(renderHeight / 2f, renderWidth / 2f);
            constFactor = MathHelper.GlobalRandom().Next(1, 10) / 100f;
        }

        protected override bool completedRun()
        {
            return false;
        }

        protected override Image<Rgba32> RunInternal()
        {
            Image<Rgba32> image = GetNewImage();
            engine3D.Canvas = image;

            factor += constFactor;

            List<Tuple<Vector2, Vector2>> lines = new List<Tuple<Vector2, Vector2>>();
            for (int i = 0; i < totalLines; i++)
            {
                Vector2 a = getVector(i, totalLines);
                Vector2 b = getVector(i * factor, totalLines);
                lines.Add(new Tuple<Vector2, Vector2>(a, b));
            }

            engine3D.DrawLines(lines, Color.White);
            return image;
        }

        private Vector2 getVector(float index, float total)
        {
            float angle = MathHelper.Map(index % total, 0, total, 0, TwoPi);
            Vector2 v = new Vector2((float) Math.Cos(angle), (float) Math.Sin(angle));
            v *= r;
            v += offset;

            return v;
        }
    }
}
