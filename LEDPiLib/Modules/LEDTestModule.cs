using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using LEDPiLib.DataItems;
using static LEDPiLib.LEDPIProcessorBase;

namespace LEDPiLib.Modules
{
    [LEDModule(LEDModules.Test)]
    public class LEDTestModule : ModuleBase
    {
        private int offset = 1;

        public LEDTestModule(ModuleConfiguration moduleConfiguration) : base(moduleConfiguration, 1f, 30)
        {
        }

        protected override bool completedRun()
        {
            return base.completedRun() && offset - 1 == 1;
        }

        protected override Image<Rgba32> RunInternal()
        {
            Image<Rgba32> image = GetNewImage();

            int center = LEDHeight / 2;

            if (offset > 20)
                offset = 1;

            image.Mutate(c => c.Draw(Color.BlueViolet, 5, new ComplexPolygon(new EllipsePolygon(new PointF(center, center), offset++))));

            return image;
        }
    }
}
