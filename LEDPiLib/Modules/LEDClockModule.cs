using System;
using LEDPiLib.DataItems;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using static LEDPiLib.LEDPIProcessorBase;

namespace LEDPiLib.Modules
{
    [LEDModule(LEDModules.Clock)]
    public class LEDClockModule : ModuleBase
    {
        private bool _init = false;
        private readonly int size;
        private Image<Rgba32> _clockImage;

        private const float _padding = 0.5f;

        public LEDClockModule(ModuleConfiguration moduleConfiguration) : base(moduleConfiguration)
        {
            size = LEDPIProcessorBase.LEDHeight;
        }

        protected override bool completedRun()
        {
            return false;
        }

        protected override Image<Rgba32> Run()
        {
            DateTime actualTime = DateTime.Now;
            float center = size / 2.0f - _padding;

            if (!_init)
            {
                _clockImage = new Image<Rgba32>(size, size);
                SetBackgroundColor(_clockImage);

                drawCircle(_clockImage, center);
                drawQuarterHours(_clockImage, center);
                drawInBetweenHours(_clockImage, center);

                _init = true;
            }

            Image<Rgba32> image = _clockImage.Clone();
            drawSeconds(image, center, actualTime);
            drawMinute(image, center, actualTime);
            drawHour(image, center, actualTime);

            return image;
        }

        private void drawCircle(Image<Rgba32> image, float center)
        {
            image.Mutate(c => c.Draw(Color.BlueViolet, .5f, new ComplexPolygon(new EllipsePolygon(new PointF(center, center), center - _padding))));
        }

        private void drawQuarterHours(Image<Rgba32> image, float center)
        {
            float[] intBetweenDegress = new float[] { 0, 90, 180, 270 };

            int lineLength = 7;
            drawHours(image, center, intBetweenDegress, lineLength);
        }

        private void drawInBetweenHours(Image<Rgba32> image, float center)
        {
            float[] intBetweenDegress = new float[] { 30, 60, 120, 150, 210, 240, 300, 330 };

            int lineLength = 4;
            drawHours(image, center, intBetweenDegress, lineLength);
        }

        private void drawHours(Image<Rgba32> image, float center, float[] intBetweenDegress, int lineLength)
        {
            PointF centerPoint = new PointF(center, center);

            foreach (float angle in intBetweenDegress)
            {
                PointF pointFirst = pointOnCircle(center - _padding, angle, centerPoint);
                PointF pointSecond = pointOnCircle(center - _padding - lineLength, angle, centerPoint);

                image.Mutate(c => c.DrawLines(Color.White, 1, new []{pointFirst, pointSecond}));
            }
        }

        private void drawSeconds(Image<Rgba32> image, float center, DateTime actualTime)
        {
            float angle = Convert.ToSingle(360 * ((actualTime.Second * 1000 + actualTime.Millisecond) / 60000.0));

            float border = center - _padding;
            PointF centerPoint = new PointF(center, center);
            PointF point = pointOnCircle(border, angle, centerPoint);

            image.Mutate(c => c.Fill(Color.Red, new ComplexPolygon(new EllipsePolygon(point, 2))));
        }

        private void drawMinute(Image<Rgba32> image, float center, DateTime actualTime)
        {
            float angle = Convert.ToSingle(360 / 60 * actualTime.Minute);

            float border = center - _padding;
            PointF centerPoint = new PointF(center, center);
            PointF point = pointOnCircle(border, angle, centerPoint);

            image.Mutate(c => c.DrawLines(Color.Yellow, 1, new[] { centerPoint, point }));
        }

        private void drawHour(Image<Rgba32> image, float center, DateTime actualTime)
        {
            float angle = Convert.ToSingle((360 / 12 * actualTime.Hour) + (30 / 60f * actualTime.Minute));

            float border = center * .50f;
            PointF centerPoint = new PointF(center, center);
            PointF point = pointOnCircle(border, angle, centerPoint);

            image.Mutate(c => c.DrawLines(Color.Yellow, 1, new[] { centerPoint, point }));
        }


        private PointF pointOnCircle(float radius, float angleInDegrees, PointF origin)
        {
            angleInDegrees -= 90;
            // Convert from degrees to radians via multiplication by PI/180        
            float x = (float)(radius * Math.Cos(angleInDegrees * Math.PI / 180F)) + origin.X;
            float y = (float)(radius * Math.Sin(angleInDegrees * Math.PI / 180F)) + origin.Y;

            return new PointF(x, y);
        }
    }
}
