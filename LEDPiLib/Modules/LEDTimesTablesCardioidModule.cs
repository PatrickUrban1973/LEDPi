using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Numerics;
using LEDPiLib.DataItems;
using LEDPiLib.Modules.Helper;
using LEDPiLib.Modules.Model;
using static LEDPiLib.LEDPIProcessorBase;

namespace LEDPiLib.Modules
{
    [LEDModule(LEDModules.TimesTablesCardioid)]
    public class LEDTimesTablesCardioidModule : ModuleBase
    {
        int totalLines = 100;

        private float r;
        private float factor = 0;
        public const float TwoPi = 6.283185f;
        private LEDEngine3D engine3D = new LEDEngine3D();
        private Vector2 offset;

        public LEDTimesTablesCardioidModule(ModuleConfiguration moduleConfiguration) : base(moduleConfiguration, 3f)
        {
            r = (renderHeight / 2 );
            offset = new Vector2(renderHeight / 2f, renderWidth / 2f);
        }

        protected override bool completedRun()
        {
            return false;
        }

        protected override Image<Rgba32> RunInternal()
        {
            Image<Rgba32> image = new Image<Rgba32>(renderWidth, renderHeight);
            SetBackgroundColor(image);
            engine3D.Canvas = image;

            factor += 0.01f;

            for (int i = 0; i < totalLines; i++)
            {
                Vector2D a = new Vector2D(getVector(i, totalLines));
                Vector2D b = new Vector2D(getVector(i * factor, totalLines));

                engine3D.DrawLine(a,b,Color.White);
            }

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
