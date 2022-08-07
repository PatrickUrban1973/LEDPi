using System;
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
        private readonly LEDPIProcessorBase _lEDPIProcessorBase;
        private readonly Font _font;
        private long lastFrameRate;
        private long currentFrameRate;
        private long counter;

        public LEDFrameRateModule(ModuleConfiguration moduleConfiguration, LEDPIProcessorBase lEDPIProcessorBase) : base(moduleConfiguration)
        {
            IsLayer = true;
            _lEDPIProcessorBase = lEDPIProcessorBase;

            SystemFonts.TryGet("Arial", out FontFamily fo);
            _font = new Font(fo, 12, FontStyle.Regular);
        }

        protected override bool completedRun()
        {
            return false;
        }

        protected override Image<Rgba32> RunInternal()
        {
            currentFrameRate = Math.Max(currentFrameRate, _lEDPIProcessorBase.LastFrameRate);
            if (counter % 10 == 0)
            {
                lastFrameRate = currentFrameRate;
                currentFrameRate = 0;
            }

            Image<Rgba32> image = GetNewImage();

            image.Mutate(c =>
                c.DrawText(
                    lastFrameRate.ToString(),
                    _font, Color.White, new PointF(0,0)));
            
            counter++;
            return image;
        }
    }
}
