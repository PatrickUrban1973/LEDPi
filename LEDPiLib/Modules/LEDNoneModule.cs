using System;
using LEDPiLib.DataItems;
using LEDPiLib.Properties;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using static LEDPiLib.LEDPIProcessorBase;

namespace LEDPiLib.Modules
{
    [LEDModule(LEDModules.None)]
    public class LEDNoneModule : ModuleBase
    {
        public LEDNoneModule(ModuleConfiguration moduleConfiguration) : base(moduleConfiguration)
        {
        }

        protected override bool completedRun()
        {
            return false;
        }

        protected override Image<Rgba32> RunInternal()
        {
            Image<Rgba32> image = Image.Load(Resources.Title);
            image.Mutate(c => c.Resize(LEDWidth, LEDHeight));
            return image;
        }
    }
}
