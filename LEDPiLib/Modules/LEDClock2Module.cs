using System;
using System.Collections.Generic;
using System.Linq;
using LEDPiLib.DataItems;
using LEDPiLib.Modules.Helper;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using static LEDPiLib.LEDPIProcessorBase;

namespace LEDPiLib.Modules
{
    [LEDModule(LEDModules.Clock2)]
    public class LEDClockModule2 : ModuleBase
    {
        private class Line
        {
            public Line(float degree, int length, float thickness)
            {
                Degree = degree;
                Length = length;
                Thickness = thickness;
            }

            public int Length { get; private set; }
            public float Degree { get; private set; }
            public float Thickness { get; private set; }
        }

        private const int _mainHoursLength = 7;
        private const int _betweenHoursLength = 4;
        private const float _padding = 0.5f;

        private readonly List<Line> lines = new List<Line>()
        {
            new Line(0f, _mainHoursLength, 1.5f),
            new Line(30f, _betweenHoursLength, 1.5f),
            new Line(60f, _betweenHoursLength, 1.5f),
            new Line(90f, _mainHoursLength, 1.5f),
            new Line(120f, _betweenHoursLength, 1.5f),
            new Line(150f, _betweenHoursLength, 1.5f),
            new Line(180f, _mainHoursLength, 1.5f),
            new Line(210f, _betweenHoursLength, 1.5f),
            new Line(240f, _betweenHoursLength, 1.5f),
            new Line(270f, _mainHoursLength, 1.5f),
            new Line(300f, _betweenHoursLength, 1.5f),
            new Line(330f, _betweenHoursLength, 1.5f),
        };

        private readonly List<string> colorSeconds = new List<string>()
        {
            "ff8800",
            "ff9c3f",
            "ffb067",
            "ffc38d",
            "ffd7b2",
            "ffebd8",
        };

        public LEDClockModule2(ModuleConfiguration moduleConfiguration) : base(moduleConfiguration, 1f)
        {
        }

        protected override bool completedRun()
        {
            return false;
        }

        protected override Image<Rgba32> RunInternal()
        {
            DateTime actualTime = DateTime.Now;
            float center = renderWidth / 2.0f - _padding;
            Image<Rgba32> clockImage = new Image<Rgba32>(renderWidth, renderHeight);
            SetBackgroundColor(clockImage);

            Dictionary<float, Color> currentSeconds = new Dictionary<float, Color>();
            
            int countMilliseconds = 0;
            int index = 0;

            Color currentColor = Color.ParseHex(colorSeconds.First());
            for (DateTime dateTime = actualTime; actualTime.AddSeconds(-4) < dateTime;)
            {
                int currentSecond = dateTime.Second;

                float angle = Convert.ToSingle(360 * ((currentSecond * 1000) / 60000.0));

                if (!currentSeconds.ContainsKey(angle))
                {
                    currentColor = Color.ParseHex(colorSeconds[index++]);
                    currentSeconds.Add(angle, currentColor);
                }

                countMilliseconds += 50;
                dateTime = dateTime.AddMilliseconds(-50);
                drawSeconds(clockImage, center, dateTime, MathHelper.Map(countMilliseconds, 0, 5050, .75f, 0f), currentColor);
            }

            index = 5;
            for (DateTime dateTime = actualTime.AddSeconds(4); actualTime < dateTime;)
            {
                int currentSecond = dateTime.Second;

                float angle = Convert.ToSingle(360 * ((currentSecond * 1000) / 60000.0));

                if (!currentSeconds.ContainsKey(angle))
                {
                    currentColor = Color.ParseHex(colorSeconds[index--]);
                    currentSeconds.Add(angle, currentColor);
                }

                dateTime = dateTime.AddMilliseconds(-50);
           }

            drawLines(clockImage, center, currentSeconds);
            drawSeconds(clockImage, center, actualTime, 1.5f, Color.ParseHex(colorSeconds.First()));

            drawHour(clockImage, center, actualTime);
            drawMinute(clockImage, center, actualTime);

            return clockImage;
        }

        private void drawCircle(Image<Rgba32> image, float center)
        {
            IPen pen = Pens.Dot(Color.Gray, 0.5f);
            image.Mutate(c => c.Draw(pen, new ComplexPolygon(new EllipsePolygon(new PointF(center, center), center - _padding))));
        }

        private void drawLines(Image<Rgba32> image, float center, Dictionary<float, Color> rednumbers)
        {
            PointF centerPoint = new PointF(center, center);

            foreach (Line line in lines)
            {
                Color color = Color.ParseHex(colorSeconds.Last());

                if (rednumbers.ContainsKey(line.Degree))
                    color = rednumbers[line.Degree];

                PointF pointFirst = pointOnCircle(center - _padding, line.Degree, centerPoint);
                PointF pointSecond = pointOnCircle(center - _padding - line.Length, line.Degree, centerPoint);

                image.Mutate(c => c.DrawLines(color, line.Thickness, new[] { pointFirst, pointSecond }));
            }
        }

        private void drawSeconds(Image<Rgba32> image, float center, DateTime actualTime, float radius, Color color)
        {
            float angle = Convert.ToSingle(360 * ((actualTime.Second * 1000 + actualTime.Millisecond) / 60000.0));

            float border = center - _padding;
            PointF centerPoint = new PointF(center, center);
            PointF point = pointOnCircle(border, angle, centerPoint);

            image.Mutate(c => c.Fill(color, new ComplexPolygon(new EllipsePolygon(point, radius))));
        }

        private void drawMinute(Image<Rgba32> image, float center, DateTime actualTime)
        {
            float angle = Convert.ToSingle(360 / 60 * actualTime.Minute);

            float border = center - _padding;
            PointF centerPoint = new PointF(center, center);
            PointF pointStart = pointOnCircle(5, angle + 180, centerPoint);
            PointF pointEnd = pointOnCircle(border - 4, angle, centerPoint);

            IBrush brush = Brushes.Horizontal(Color.LightGray, Color.LightGray);

            image.Mutate(c => c.DrawLines(brush, 2.5f, new[] { pointStart, pointEnd }));
        }

        private void drawHour(Image<Rgba32> image, float center, DateTime actualTime)
        {
            float angle = Convert.ToSingle((360 / 12 * actualTime.Hour) + (30 / 60f * actualTime.Minute));

            float border = center * .50f;
            PointF centerPoint = new PointF(center, center);
            PointF pointEnd = pointOnCircle(border, angle, centerPoint);
            PointF pointStart = pointOnCircle(5, angle + 180, centerPoint);

            IBrush brush = Brushes.Horizontal(Color.LightGray, Color.LightGray);

            image.Mutate(c => c.DrawLines(brush, 2.5f, new[] { pointStart, pointEnd }));
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
