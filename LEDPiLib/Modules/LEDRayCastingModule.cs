using System.Collections.Generic;
using LEDPiLib.DataItems;
using LEDPiLib.Modules.Helper;
using LEDPiLib.Modules.Model.RayCasting;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using static LEDPiLib.LEDPIProcessorBase;

namespace LEDPiLib.Modules
{
    [LEDModule(LEDModules.RayCasting)]
    public class LEDRayCastingModule : ModuleBase
    {
        private readonly LEDEngine3D engine3D = new LEDEngine3D();
        private readonly Image<Rgba32> backgroundImage;

        private readonly List<Boundary> walls = new List<Boundary>();
        private readonly Starter starter;
        private float xoff;
        private float yoff = 10000;
        private const int countWalls = 5;

        private readonly Perlin perlin = new Perlin();
        
        public LEDRayCastingModule(ModuleConfiguration moduleConfiguration) : base(moduleConfiguration, 5f, 15)
        {
            backgroundImage = GetNewImage();
            engine3D.Canvas = backgroundImage;

            for (int i = 0; i < countWalls; i++)
            {
                float x1 = MathHelper.GlobalRandom().Next(renderWidth);
                float x2 = MathHelper.GlobalRandom().Next(renderWidth);
                float y1 = MathHelper.GlobalRandom().Next(renderHeight);
                float y2 = MathHelper.GlobalRandom().Next(renderHeight);
                walls.Add(new Boundary(x1, y1, x2, y2));
            }

            foreach (Boundary wall in walls)
            {
                wall.Show(engine3D);
            }

            walls.Add(new Boundary(0, 0, renderWidth, 0));
            walls.Add(new Boundary(renderWidth, 0, renderWidth, renderHeight));
            walls.Add(new Boundary(renderWidth, renderHeight, 0, renderHeight));
            walls.Add(new Boundary(0, renderHeight, 0, 0));
            starter = new Starter(renderWidth/2, renderHeight/2);

        }

        protected override bool completedRun()
        {
            return false;
        }

        protected override Image<Rgba32> RunInternal()
        {
            Image<Rgba32> image = backgroundImage.Clone();
            engine3D.Canvas = image;

            starter.Update(perlin.perlin(xoff, 1F) * renderWidth, perlin.perlin(1F, yoff) * renderHeight);
            starter.Show(engine3D);
            starter.Look(walls, engine3D);

            xoff += 0.01f;
            yoff += 0.01f;

            return image;
        }
    }
}
