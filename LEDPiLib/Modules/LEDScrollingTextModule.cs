using System;
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
        private enum Position { Top, Middle, Bottom};

        private bool _init;
        private Image<Rgba32> _wholeTextImage;
        private int _offset;
        private readonly string _text;
        private readonly string _color = string.Empty;
        private readonly Position _position = Position.Middle;

             
        public LEDScrollingTextModule(ModuleConfiguration moduleConfiguration) : base(moduleConfiguration, 1, 20)
        {
            string[] parameters = moduleConfiguration.Parameter.Split(';');

            if (parameters.Length > 0)
            {
                _text = parameters[0];
            }
            if (parameters.Length > 1)
            {
                _position = (Position)Enum.Parse(typeof(Position), parameters[1]);
            }
            if (parameters.Length > 2)
            {
                _color = parameters[2];
            }
        }

        protected override bool completedRun()
        {
            return base.completedRun() && _offset - 1 == 0;
        }

        protected override Image<Rgba32> RunInternal()
        {
            if (!_init)
            {
                SystemFonts.TryGet("Times New Roman", out var fo);
                var font = new Font(fo, 30, FontStyle.Regular);
                FontRectangle size = TextMeasurer.Measure(
                    _text,
                    new TextOptions(font));

                Image<Rgba32> loadImage =
                    new Image<Rgba32>(Convert.ToInt32(size.Width) + (2 * LEDWidth),
                        Math.Max(Convert.ToInt32(size.Height), LEDHeight), GetBackground());

                float position = 0;
                if (_position == Position.Middle)
                    position = (loadImage.Height - size.Height) / 2;
                else if (_position == Position.Bottom)
                    position = (loadImage.Height - size.Height);

                Color color = Color.LightYellow;

                if (!string.IsNullOrEmpty(_color))
                    color = Color.ParseHex(_color.Trim());

                loadImage.Mutate(c =>
                    c.DrawText(
                            _text,
                            font, color, new PointF(LEDPIProcessorBase.LEDWidth, position)));

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
