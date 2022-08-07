using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using LEDPiLib.DataItems;
using static LEDPiLib.LEDPIProcessorBase;
using LEDPiLib.Modules.Model.SpaceColonization;

namespace LEDPiLib.Modules
{
    [LEDModule(LEDModules.SpaceColonization)]
    public class LEDSpaceColonizationModule : ModuleBase
    {
        private readonly Tree tree;
        private const float min_dist = 2;
        private const float max_dist = 15;


        public LEDSpaceColonizationModule(ModuleConfiguration moduleConfiguration) : base(moduleConfiguration, 4f)
        {
            tree = new Tree(renderWidth, renderHeight, max_dist);
        }

        protected override bool completedRun()
        {
            return false;
        }

        protected override Image<Rgba32> RunInternal()
        {
            Image<Rgba32> image = GetNewImage();

            tree.Show(image);
            tree.Grow(min_dist, max_dist);
            return image;
        }
    }
}
