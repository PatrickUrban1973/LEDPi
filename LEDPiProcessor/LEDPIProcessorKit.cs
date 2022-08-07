using System;
using SixLabors.ImageSharp;
using LEDPiLib;
using rpi_rgb_led_matrix_sharp;
using SixLabors.ImageSharp.PixelFormats;
using System.IO;

namespace LEDPiProcessor
{
    class LEDPIProcessorKit : LEDPIProcessorBase
    {
        private static readonly RGBLedMatrix matrix = new RGBLedMatrix(new RGBLedMatrixOptions { ChainLength = 1, Rows = LEDPIProcessorBase.LEDHeight, Cols = LEDPIProcessorBase.LEDWidth, HardwareMapping = "adafruit-hat-pwm" });
        private static RGBLedCanvas canvas;

        public LEDPIProcessorKit()
        {
            canvas = matrix.CreateOffscreenCanvas();
            BasePath = Path.Combine("/home/pi/LEDPi/Processor");
        }

        protected override void doProcess(Image<Rgba32> image)
        {
            image.ProcessPixelRows(accessor =>
            {
                for (int y = 0; y < canvas.Height; y++)
                {
                    Span<Rgba32> rowSpan = accessor.GetRowSpan(y);

                    for (int x = 0; x < canvas.Width; x++)
                    {
                        Rgba32 pixel = rowSpan[x];

                        canvas.SetPixel(x, y, new rpi_rgb_led_matrix_sharp.Color(pixel.R, pixel.G, pixel.B));
                    }
                }
            });

            canvas = matrix.SwapOnVsync(canvas);
        }
    }
}
