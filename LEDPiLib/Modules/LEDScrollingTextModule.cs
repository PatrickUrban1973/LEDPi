using System;
using System.Threading;
using LEDPiLib.DataItems;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using static LEDPiLib.LEDPIProcessorBase;

namespace LEDPiLib.Modules
{
    [LEDModule(LEDModules.ScrollingText)]
    public class LEDScrollingTextModule : ModuleBase
    {
        private bool _init = false;
        private Image<Rgba32> _wholeTextImage;
        private int _offset = 0;
        private string _text;

        public LEDScrollingTextModule(ModuleConfiguration moduleConfiguration) : base(moduleConfiguration, 1, 20)
        {
            _text = moduleConfiguration.Parameter;
        }

        protected override bool completedRun()
        {
            return base.completedRun() && _offset - 1 == 0;
        }

        protected override Image<Rgba32> RunInternal()
        {
            if (!_init)
            {
                FontFamily fo;

                SystemFonts.TryFind("Times New Roman", out fo);
                var font = new Font(fo, 30, FontStyle.Regular);
                FontRectangle size = TextMeasurer.Measure(
                    _text,
                    new RendererOptions(font));

                Image<Rgba32> loadImage =
                    new Image<Rgba32>(Convert.ToInt32(size.Width) + (2 * LEDPIProcessorBase.LEDWidth),
                        Math.Max(Convert.ToInt32(size.Height), LEDPIProcessorBase.LEDHeight));

                SetBackgroundColor(loadImage);

                loadImage.Mutate(c =>
                    c.DrawText(
                            _text,
                            font, Color.LightYellow, new PointF(LEDPIProcessorBase.LEDWidth, (loadImage.Height - size.Height) / 2)));

                _wholeTextImage = loadImage.Clone();
                _init = true;
            }
            
            if (_offset + LEDPIProcessorBase.LEDWidth > _wholeTextImage.Width)
                _offset = 0;

            var cropedImage = _wholeTextImage.Clone();

            cropedImage.Mutate(c =>
                c.Crop(new Rectangle(_offset++, 0, LEDPIProcessorBase.LEDWidth, LEDPIProcessorBase.LEDHeight)));

            return cropedImage;
        }
    }
}
