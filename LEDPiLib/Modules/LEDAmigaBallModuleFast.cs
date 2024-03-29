﻿using System;
using System.Collections.Generic;
using System.Linq;
using LEDPiLib.DataItems;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using static LEDPiLib.LEDPIProcessorBase;

namespace LEDPiLib.Modules
{
    [LEDModule(LEDModules.AmigaBallFast)]
    public class LEDAmigaBallModuleFast : ModuleBase
    {
        public LEDAmigaBallModuleFast(ModuleConfiguration moduleConfiguration) : base(moduleConfiguration, 1, 15)
        {
            x = LEDWidth * 0.5f;
            dx = LEDWidth * 0.003f;
            xBoundMax = LEDWidth * 0.8f;
            xBoundMin = LEDWidth * 0.2f;
            yBoundMax = LEDHeight * 0.68f;
            yConstante = LEDHeight * 0.40f;

            backGroundImage.Mutate(c => c.BackgroundColor(Color.LightGray));
            draw_wireframe(backGroundImage);
            backGroundImage.Mutate(c => c.Resize(LEDWidth, LEDHeight));
        }

        private float x;
        private readonly float dx;
        private readonly float xBoundMax;
        private readonly float xBoundMin;

        private readonly float yBoundMax;
        private readonly float yConstante;
        private readonly List<int> _ys = new List<int>() { 442, 454, 468 };

        private float phase = 45.0f;
        private const float dp = 2.5f;
        private bool right = true;
        private float y_ang;

        private readonly Image<Rgba32> backGroundImage =
            new Image<Rgba32>(640, 512);

        protected override bool completedRun()
        {
            return base.completedRun() && right && x == LEDWidth * 0.5f;
        }

        protected override Image<Rgba32> RunInternal()
        {
            Image<Rgba32> image = backGroundImage.Clone();

            image.Mutate(c => c.BackgroundColor(Color.LightGray));
            if (right)
            {
                phase -= dp;

                if (phase < 0)
                    phase = 42.5f;

                x += dx;
            }
            else
            {
                phase += dp;

                if (phase > 42.5f)
                    phase = 0f;
                
                x -= dx;
            }

            if (x >= xBoundMax)
                right = false;
            if (x < xBoundMin)
                right = true;

            y_ang = (y_ang + 1.5f) % 360.0f;

            float y = yBoundMax - yConstante * (float)(Math.Abs(Math.Cos(y_ang * Math.PI / 180.0)));
            calc_and_draw(image, phase, 12.0f, x, y);
            
            return image;
        }


        private float get_lat(float localPhase, int i)
        {
            if (i == 0)
                return -90.0f;
            else if (i == 9)
                return 90.0f;
            else
                return -90.0f + localPhase + (i - 1) * 22.5f;
        }

        private Dictionary<PointF, PointF> calc_points(float localPhase)
        {
            Dictionary<PointF, PointF> points = new Dictionary<PointF, PointF>();
            
            for(int i =0; i <= 10; i++)
            {
                float lat = get_lat(localPhase, i);
                double sin_lat = Math.Sin(lat * Math.PI / 180.0);

                for (int j = 0; j <= 9; j++)
                {
                    float lon = -90.0f + j * 22.5f;
                    double y = Math.Sin(lon * Math.PI / 180.0);
                    double l = Math.Cos(lon * Math.PI / 180.0);

                    points.Add(new PointF(i, j), new PointF((float)(sin_lat * l), (float)y));
                }
            }

            return points;
        }

        private void tilt_sphere(Dictionary<PointF, PointF> points, float ang)
        {
            double st = Math.Sin(ang * Math.PI / 180.0);
            double ct = Math.Cos(ang * Math.PI / 180.0);

            foreach(PointF point in points.Keys.ToList())
            {
                PointF otherPoint = points[point];
                otherPoint.X = (float)(otherPoint.X * ct - otherPoint.Y * st);
                otherPoint.Y = (float)(otherPoint.X * st + otherPoint.Y * ct);

                points[point] = otherPoint;
            }

        }

        private void scale_and_translate(Dictionary<PointF, PointF> points, float s, float tx, float ty)
        {
            foreach (PointF point in points.Keys.ToList())
            {
                PointF otherPoint = points[point];
                otherPoint.X = otherPoint.X * s + tx;
                otherPoint.Y = otherPoint.Y * s + ty;

                points[point] = otherPoint;
            }
        }

        private void transform(Dictionary<PointF, PointF> points, float s, float tx, float ty)
        {
            tilt_sphere(points, 17.0f);
            scale_and_translate(points, s, tx, ty);
        }

        private void draw_meridians(Image<Rgba32> image, Dictionary<PointF, PointF> points)
        {
            for (int i = 0; i <= 10; i++)
            {
                for (int j = 0; j <= 8; j++)
                {
                    PointF point1 = points[points.Keys.First(c => c.X == i && c.Y == j)];
                    PointF point2 = points[points.Keys.First(c => c.X == i && c.Y == j + 1)];

                    image.Mutate(c => c.DrawLines(Color.Black, .1f, new[] { point1, point2 }));

                }
            }
        }

        private void draw_parabels(Image<Rgba32> image, Dictionary<PointF, PointF> points)
        {
            for (int i = 0; i <= 7; i++)
            {
                PointF point1 = points[points.Keys.First(c => c.X == 0 && c.Y == i + 1)];
                PointF point2 = points[points.Keys.First(c => c.X == 9 && c.Y == i + 1)];

                image.Mutate(c => c.DrawLines(Color.Black, .1f, new[] { point1, point2 }));
            }
        }

        private void fill_tiles(Image<Rgba32> image, Dictionary<PointF, PointF> points, bool isRed)
        {
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    PointF point1 = points[points.Keys.First(c => c.X == i && c.Y == j)];
                    PointF point2 = points[points.Keys.First(c => c.X == i + 1 && c.Y == j)];
                    PointF point3 = points[points.Keys.First(c => c.X == i + 1 && c.Y == j + 1)];
                    PointF point4 = points[points.Keys.First(c => c.X == i && c.Y == j + 1)];

                    image.Mutate(c => c.FillPolygon(isRed ? Color.Red : Color.White, new[] { point1, point2, point3, point4 }));
                    //image.Mutate(c => c.DrawPolygon(Color.Black, 0.001f, new[] { point1, point2, point3, point4 })); // to small for borders
                    isRed = !isRed;
                }
            }
        }

        private void draw_shadow(Image<Rgba32> image, Dictionary<PointF, PointF> points)
        {
            List<PointF> shadow = new List<PointF>();

            for (int i = 0; i < 9; i++)
            {
                PointF point1 = points[points.Keys.First(c => c.X == 0 && c.Y == i)];
                shadow.Add(new PointF(point1.X * 1.08f, point1.Y));
            }

            for (int i = 0; i < 8; i++)
            {
                PointF point1 = points[points.Keys.FirstOrDefault(c => c.X == 9 && c.Y == 7-i)];
                shadow.Add(new PointF(point1.X * 1.08f, point1.Y));
            }

//            PatternBrush b = Brushes.Percent20(Color.Gray);


            image.Mutate(c => c.FillPolygon(Color.Gray, shadow.ToArray()));
        }

        private void draw_wireframe(Image<Rgba32> image)
        {
            for (int i = 0; i <= 13; i++)
            {
                var i1 = i;
                image.Mutate(c => c.DrawLines(Color.Purple, 1, new[] { new PointF(50, i1 * 36), new PointF(590, i1 * 36) }));
            }

            for (int i = 0; i <= 16; i++)
            {
                var i1 = i;
                image.Mutate(c => c.DrawLines(Color.Purple, 1, new[] { new PointF(50 + i1 * 36, 0), new PointF(50 + i1 * 36, 432) }));
            }

            for (int i = 0; i <= 16; i++)
            {
                var i1 = i;
                image.Mutate(c => c.DrawLines(Color.Purple, 1, new[] { new PointF(50 + i1 * 36, 432), new PointF(i1 * 42.666f, 480) }));
            }

            foreach (int y in _ys)
            {
                float x1 = (50 - 50.0f * (y - 432f) / (480.0f - 432.0f));
                image.Mutate(c => c.DrawLines(Color.Purple, 1, new[] { new PointF(x1, y), new PointF(640 - x1, y) }));
            }
        }

        private void calc_and_draw(Image<Rgba32> image, float localPhase, float scale, float localX, float y)
        {
            Dictionary<PointF, PointF> points = calc_points(localPhase % 22.5f);
            transform(points, scale, localX, y);
            draw_shadow(image, points);
//            draw_wireframe(image);
            fill_tiles(image, points, phase >= 22.5f);

//            draw_meridians(image, points);
//            draw_parabels(image, points);
        }
    }
}
