using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using LEDPiLib.DataItems;
using LEDPiLib.Modules.Helper;
using LEDPiLib.Modules.Model.Firework;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using static LEDPiLib.LEDPIProcessorBase;

namespace LEDPiLib.Modules
{
    [LEDModule(LEDModules.Firework)]
    public class LEDFireworkModule : ModuleBase
    {
        private readonly LEDEngine3D engine3D = new LEDEngine3D();
        private readonly List<Firework> fireworks = new List<Firework>();
        private readonly Vector2 gravity = new Vector2(0, 0.4f);
        private Image<Rgba32> image;

        public LEDFireworkModule(ModuleConfiguration moduleConfiguration) : base(moduleConfiguration, 2f, 30)
        {
            Firework.Gravity = gravity;
        }

        protected override bool completedRun()
        {
            return false;
        }

        protected override Image<Rgba32> RunInternal()
        {
            image = GetNewImage();
            UseBlend(image, 0.75f);

            engine3D.Canvas = image;

            if (MathHelper.GlobalRandom().NextDouble() < 0.08)
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

            SetLastPictureBlend(image);
            return image;
        }
    }
}
