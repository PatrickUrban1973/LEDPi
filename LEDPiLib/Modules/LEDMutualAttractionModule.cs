using System;
using System.Collections.Generic;
using LEDPiLib.DataItems;
using LEDPiLib.Modules.Helper;
using LEDPiLib.Modules.Model.MutualAttraction;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using static LEDPiLib.LEDPIProcessorBase;

namespace LEDPiLib.Modules
{
    [LEDModule(LEDModules.MutualAttraction)]
    public class LEDMutualAttractionModule : ModuleBase
    {
        private readonly List<Mover> movers = new List<Mover>();
        private readonly Mover sun;

        private Image<Rgba32> image;

        public LEDMutualAttractionModule(ModuleConfiguration moduleConfiguration) : base(moduleConfiguration, 3f, 15)
        {
            int maxMovers = MathHelper.GlobalRandom().Next(3, 15);
            
            for(int i = 0; i < maxMovers; i++)
            {
                movers.Add(
                    new Mover(  MathHelper.GlobalRandom().Next(0, renderWidth), 
                                MathHelper.GlobalRandom().Next(0, renderHeight),
                                MathHelper.GlobalRandom().Next(5, 15), 
                                MathHelper.GlobalRandom().Next(5, 15), 
                                MathHelper.GlobalRandom().Next(1, 4), Colors[Convert.ToInt32(MathHelper.Map(i,0,15,0,254))]));
            }

            sun = new Mover(renderWidth/2, renderHeight/2, 0, 0, 15, Color.Yellow);
        }

        protected override bool completedRun()
        {
            return false;
        }

        protected override Image<Rgba32> RunInternal()
        {
            image = GetNewImage();
            UseBlend(image, 0.85f);

            movers.ForEach(mover => sun.Attract(mover));
            movers.ForEach(mover =>
            {
                mover.Update();
                mover.Display(image);
            });

           sun.Display(image);

            SetLastPictureBlend(image);
            return image;
        }
    }
}
