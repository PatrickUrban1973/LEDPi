using System;
using System.Diagnostics;
using LEDPiLib.DataItems;
using LEDPiLib.Modules.Helper;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using static LEDPiLib.LEDPIProcessorBase;

namespace LEDPiLib.Modules
{
    [LEDModule(LEDModules.Countdown)]
    public class LEDCountdownModule : ModuleBase
    {
        private readonly Stopwatch stopwatch;
        private readonly TimeSpan timeSpan;
        private Image<Rgba32> image;

        public LEDCountdownModule(ModuleConfiguration moduleConfiguration) : base(moduleConfiguration)
        {
            stopwatch = new Stopwatch();
            stopwatch.Start();
            timeSpan = new TimeSpan(0, 0, 0, Convert.ToInt32(moduleConfiguration.Parameter));
        }

        protected override bool completedRun()
        {
            return base.completedRun() && stopwatch.Elapsed > timeSpan;
        }

        protected override Image<Rgba32> RunInternal()
        {
            TimeSpan countDownSpan = timeSpan - stopwatch.Elapsed;
            string displayText;

            Color color = Color.White;
            FontFamily fo;

            SystemFonts.TryFind("Times New Roman", out fo);
            Font font = new Font(fo, 20, FontStyle.Regular);

            if (countDownSpan.TotalSeconds > 11)
            {
                font = new Font(fo, 20, FontStyle.Regular);
                string hours = string.Empty;
                string minutes = string.Empty;
                string seconds = string.Empty;

                if (countDownSpan.Hours > 0)
                {
                    hours = "0" + countDownSpan.Hours.ToString();
                    hours = hours.Substring(hours.Length - 2) + ":";
                }

                minutes = "0" + countDownSpan.Minutes.ToString();
                minutes = minutes.Substring(minutes.Length - 2) + ":";

                seconds = "0" + countDownSpan.Seconds.ToString();
                seconds = seconds.Substring(seconds.Length - 2);

                displayText = hours + minutes + seconds;
            }
            else if (countDownSpan.TotalSeconds <= 1)
            {
                displayText = string.Empty;
            }
            else
            {
                font = new Font(fo, 35, FontStyle.Regular);
                displayText = countDownSpan.Seconds.ToString();
                color = color.WithAlpha(MathHelper.Map(countDownSpan.Milliseconds, 1, 1000, 0f, 1f));
            }

            FontRectangle size = TextMeasurer.Measure(
                displayText,
                new RendererOptions(font));

            image = new Image<Rgba32>(LEDWidth, LEDHeight);

            SetBackgroundColor(image);

            image.Mutate(c =>
                c.DrawText(
                        displayText,
                        font, color, new PointF((image.Width - size.Width) / 2, (image.Height - size.Height) / 2)));

            return image;
        }
    }
}
