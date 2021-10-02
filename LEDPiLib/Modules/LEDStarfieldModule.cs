using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Collections.Generic;
using LEDPiLib.DataItems;
using LEDPiLib.Modules.Model.Starfield;
using static LEDPiLib.LEDPIProcessorBase;

namespace LEDPiLib.Modules
{
    [LEDModule(LEDModules.Starfield)]
    public class LEDStarfieldModule : ModuleBase
    {
        private List<Star> stars = new List<Star>();
        private float speed = 2.5f;

        public LEDStarfieldModule(ModuleConfiguration moduleConfiguration) : base(moduleConfiguration, 1f, 30)
        {
            Star.RenderHeight = renderHeight;
            Star.RenderWidth = renderWidth;

            for (int i = 0; i < 500; i++)
            {
                stars.Add(new Star());
            }
        }

        protected override bool completedRun()
        {
            return false;
        }

        protected override Image<Rgba32> RunInternal()
        {
            Image<Rgba32> image = new Image<Rgba32>(renderWidth, renderHeight);
            SetBackgroundColor(image);

            foreach (Star star in stars)
            {
                star.Update(speed);
                star.Show(ref image);
            }

            return image;
        }
    }
}
