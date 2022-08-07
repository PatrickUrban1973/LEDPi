using System;
using System.Collections.Generic;
using System.Numerics;
using LEDPiLib.DataItems;
using LEDPiLib.Modules.Helper;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using static LEDPiLib.LEDPIProcessorBase;

namespace LEDPiLib.Modules
{
    [LEDModule(LEDModules.WorleyNoise)]
    public class LEDWorleyNoiseModule : ModuleBase
    {
        private readonly int maxDist;
        private const decimal CELL_SIZE = 10;
        
        private List<Vector2> points = new List<Vector2>();
        private List<Vector2> dirs = new List<Vector2>();
        private Dictionary<int, Dictionary<int, Color>> pointsToColor = new Dictionary<int, Dictionary<int, Color>>();
        
        public LEDWorleyNoiseModule(ModuleConfiguration moduleConfiguration) : base(moduleConfiguration, 2f, 10)
        {
            maxDist = 1600;
            for (int y = 0; y < renderHeight; y++)
            {
                Dictionary<int, Color> row2Color = new Dictionary<int, Color>();
                pointsToColor.Add(y, row2Color);

                for (int x = 0; x < renderWidth; x++)
                {
                    row2Color.Add(x, Color.Black);
                }
            }

            for (int i = 0; i < 12; i++) {
                Vector2 p = new Vector2(
                    MathHelper.GlobalRandom().Next(0, renderWidth), MathHelper.GlobalRandom().Next(0, renderHeight));
                points.Add(p);
                dirs.Add(new Vector2(MathHelper.GlobalRandom().Next(-1000, 1000) / 1000f, MathHelper.GlobalRandom().Next(-1000, 1000) / 1000f));
            } 
        }

        protected override bool completedRun()
        {
            return false;
        }

        protected override Image<Rgba32> RunInternal()
        {
            DateTime start = DateTime.Now;

            Image<Rgba32> image = GetNewImage();

            for (int y = 0; y < renderHeight; y++)
            {
                for (int x = 0; x < renderWidth; x++)
                {
                    float gx = (float)Math.Floor(x / CELL_SIZE);

                    float dist = maxDist;
                    float gy = (float)Math.Floor(y / CELL_SIZE);

                    for (float x0 = gx - 1; x0 <= gx + 1; x0++)
                    {
                        for (float y0 = gy - 1; y0 <= gy + 1; y0++)
                        {
                            if (x0 < 0 || x0 >= renderWidth ||
                                y0 < 0 || y0 >= renderHeight)
                                continue;

                            for (int i = 0; i < points.Count; i++)
                                dist = Math.Min(dist,
                                          distSq(x, y, points[i].X, points[i].Y));
                        }
                    }

                    pointsToColor[y][x] = (maxDist == dist) ? (Rgba32)Color.Black : Colors[(int)MathHelper.Map(dist, 0, maxDist, 0, 205)];
                }
            }

            image.ProcessPixelRows(accessor =>
            {
                for (int y = 0; y < renderHeight; y++)
                {
                    var row = accessor.GetRowSpan(y);

                    for (int x = 0; x < renderWidth; x++)
                    {
                        row[x] = pointsToColor[y][x];
                    }
                }
            });

            for (int i = 0; i < points.Count; i++)
            {
                Vector2 p = points[i];
                Vector2 direction = dirs[i];
                p += direction;
                if (p.X <= 0 || p.X >= renderWidth)
                    direction = new Vector2(direction.X * -1, direction.Y);
                if (p.Y <= 0 || p.Y >= renderHeight)
                    direction = new Vector2(direction.X, direction.Y * -1);

                dirs[i] = direction;
                points[i] = p;
            }

            return image;
        }

        private float distSq(float x0, float y0, float x1, float y1)
        {
            float dx = (x1 - x0);
            float dy = (y1 - y0);
            return dx * dx + dy * dy;
        }

    }
}
