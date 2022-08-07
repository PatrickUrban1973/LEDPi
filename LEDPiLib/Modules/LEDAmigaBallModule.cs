using System;
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
    public class LEDAmigaBallModule : ModuleBase
    {
        public LEDAmigaBallModule(ModuleConfiguration moduleConfiguration) : base(moduleConfiguration)
        {
        }

        private float phase = 45.0f;
        private float dp = 2.5f;
        private float x = 320f;
        private float dx = 2.1f;
        private bool right = true;
        private float y_ang;

        protected override bool completedRun()
        {
            return base.completedRun() && right && x == 320f;
        }

        protected override Image<Rgba32> RunInternal()
        {
            Image<Rgba32> image = new Image<Rgba32>(640, 512);
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

            if (x >= 505)
                right = false;
            if (x < 135)
                right = true;

            y_ang = (y_ang + 1.5f) % 360.0f;

            float y = 350.0f - 200.0f * (float) Math.Abs(Math.Cos(y_ang * Math.PI / 180.0));
            calc_and_draw(image, phase, 120.0f, x, y);
            image.Mutate(c => c.Resize(LEDWidth, LEDHeight));

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

                    points.Add(new PointF(i, j), new PointF((float)(sin_lat * l), (float) y));
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

        private void fill_tiles(Image<Rgba32> image, Dictionary<PointF, PointF> points, bool isRed)
        {
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    bool localIsRed = isRed;
                    PointF point1 = points[points.Keys.First(c => c.X == i && c.Y == j)];
                    PointF point2 = points[points.Keys.First(c => c.X == i + 1 && c.Y == j)];
                    PointF point3 = points[points.Keys.First(c => c.X == i + 1 && c.Y == j + 1)];
                    PointF point4 = points[points.Keys.First(c => c.X == i && c.Y == j + 1)];

                    image.Mutate(c => c.FillPolygon(localIsRed ? Color.Red : Color.White, new[] { point1, point2, point3, point4 }));
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
                shadow.Add(new PointF(point1.X + 50, point1.Y));
            }

            for (int i = 0; i < 8; i++)
            {
                PointF point1 = points[points.Keys.FirstOrDefault(c => c.X == 9 && c.Y == 7-i)];
                shadow.Add(new PointF(point1.X + 50, point1.Y));
            }

            image.Mutate(c => c.FillPolygon(Color.Gray, shadow.ToArray()));
        }

        private List<int> _ys = new List<int>() { 442, 454, 468 };

        private void draw_wireframe(Image<Rgba32> image)
        {
            for (int i = 0; i <= 13; i++)
            {
                image.Mutate(c => c.DrawLines(Color.Purple, 1, new[] { new PointF(50, i * 36), new PointF(590, i * 36)}));
            }
 
            for (int i = 0; i <= 16; i++)
            {
                image.Mutate(c => c.DrawLines(Color.Purple, 1, new[] { new PointF(50 + i * 36, 0), new PointF(50 + i * 36, 432) }));
            }

            for (int i = 0; i <= 16; i++)
            {
                image.Mutate(c => c.DrawLines(Color.Purple, 1, new[] { new PointF(50 + i * 36, 432), new PointF(i * 42.666f, 480) }));
            }

            foreach(int y in _ys)
            {
                float x1 = (50 - 50.0f * (y - 432) / (480.0f - 432.0f));
                image.Mutate(c => c.DrawLines(Color.Purple, 1, new[] { new PointF(x1, y), new PointF(640 - x1, y) }));
            }
        }

        private void calc_and_draw(Image<Rgba32> image, float localPhase, float scale, float x, float y)
        {
            Dictionary<PointF, PointF> points = calc_points(localPhase % 22.5f);
            transform(points, scale, x, y);
            draw_shadow(image, points);
            draw_wireframe(image);
            fill_tiles(image, points, localPhase >= 22.5f);

            //draw_meridians(image, points);
            //draw_parabels(image, points);
        }
    }
}
