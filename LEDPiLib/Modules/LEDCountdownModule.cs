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

            if (moduleConfiguration.Duration > 0)
            {
                timeSpan = new TimeSpan(0, 0, moduleConfiguration.Duration);
            }
            else
            {
                DateTime configurationTime = DateTime.ParseExact(moduleConfiguration.Parameter, "yyyy-MM-dd HH:mm:ss",
                                           System.Globalization.CultureInfo.InvariantCulture);
                timeSpan = configurationTime - DateTime.Now;
            }
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

            SystemFonts.TryGet("Times New Roman", out var fo);
            Font font = new Font(fo, 20, FontStyle.Regular);

            if (countDownSpan.TotalSeconds > 11)
            {
                font = new Font(fo, 20, FontStyle.Regular);
                string hours = string.Empty;

                if (countDownSpan.Hours > 0)
                {
                    hours = "0" + countDownSpan.Hours.ToString();
                    hours = hours.Substring(hours.Length - 2) + ":";
                }

                string minutes = "0" + countDownSpan.Minutes.ToString();
                minutes = minutes.Substring(minutes.Length - 2) + ":";

                string seconds = "0" + countDownSpan.Seconds.ToString();
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
                new TextOptions(font));

            image = GetNewImage();

            image.Mutate(c =>
                c.DrawText(
                        displayText,
                        font, color, new PointF((image.Width - size.Width) / 2, (image.Height - size.Height) / 2)));

            return image;
        }
    }
}
