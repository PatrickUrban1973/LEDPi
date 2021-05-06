using System;
using System.Collections.Generic;
using System.Linq;
using LEDPiLib.DataItems;
using LEDPiLib.Modules.Helper;
using LEDPiLib.Modules.Model;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using static LEDPiLib.LEDPIProcessorBase;

namespace LEDPiLib.Modules
{
    [LEDModule(LEDModules.Firework)]
    public class LEDFireworkModule : ModuleBase
    {
        private LEDEngine3D engine3D = new LEDEngine3D();
        List<Firework> fireworks = new List<Firework>();
        private Random random = new Random();
        private Vector2D gravity = new Vector2D(0, 0.4f);
        private float renderOffset = 2f;

        private Image<Rgba32> image;

        public LEDFireworkModule(ModuleConfiguration moduleConfiguration) : base(moduleConfiguration)
        {
            Firework.Gravity = gravity;
            renderWidth = Convert.ToInt32(LEDWidth * renderOffset);
            renderHeight = Convert.ToInt32(LEDHeight * renderOffset);
        }

        protected override bool completedRun()
        {
            return false;
        }

        protected override Image<Rgba32> Run()
        {
            image = new Image<Rgba32>(renderWidth, renderHeight);
            SetBackgroundColor(image);
            engine3D.Canvas = image;

            if (random.NextDouble() < 0.08)
            {
                fireworks.Add(new Firework(renderWidth, renderHeight));
            }

            for (int i = fireworks.Count() - 1; i >= 0; i--)
            {
                Firework f = fireworks[i];
                f.Run(engine3D);
                if (f.Done())
                {
                    fireworks.RemoveAt(i);
                }
            }

            image.Mutate(c => c.Resize(LEDWidth, LEDHeight));
            return image;
        }
    }
}
