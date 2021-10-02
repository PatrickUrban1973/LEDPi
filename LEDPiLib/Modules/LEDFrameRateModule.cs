using LEDPiLib.DataItems;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace LEDPiLib.Modules
{
    public class LEDFrameRateModule : ModuleBase
    {
        private LEDPIProcessorBase _lEDPIProcessorBase;
        private Font _font;

        public LEDFrameRateModule(ModuleConfiguration moduleConfiguration, LEDPIProcessorBase lEDPIProcessorBase) : base(moduleConfiguration)
        {
            FontFamily fo;
            IsLayer = true;
            _lEDPIProcessorBase = lEDPIProcessorBase;

            SystemFonts.TryFind("Times New Roman", out fo);
            _font = new Font(fo, 12, FontStyle.Regular);
        }

        protected override bool completedRun()
        {
            return false;
        }

        protected override Image<Rgba32> RunInternal()
        {
            Image<Rgba32> image =
                new Image<Rgba32>(renderWidth, renderHeight);
            SetBackgroundColor(image);

            image.Mutate(c =>
                c.DrawText(
                    _lEDPIProcessorBase.LastFrameRate.ToString(),
                    _font, Color.White, new PointF(0,0)));

            return image;
        }
    }
}
