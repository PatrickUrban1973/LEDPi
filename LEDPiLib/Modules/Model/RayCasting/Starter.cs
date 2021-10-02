using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using LEDPiLib.Modules.Helper;
using SixLabors.ImageSharp;

namespace LEDPiLib.Modules.Model.RayCasting
{
    class Starter
    {
        private Vector2D pos;
        private List<Ray> rays;

        public Starter(float x, float y)
        {
            pos = new Vector2D(x, y);
            rays = new List<Ray>();

            for (int i = 0; i < 360; i+=5)
            {
                rays.Add(new Ray(pos, i));
            }
        }

        public void Update(float x, float y)
        {
            this.pos.vector.X = x;
            this.pos.vector.Y = y;

            rays.ForEach(c => c.Update(x,y));
        }

        public void Show(LEDEngine3D engine3D)
        {
            engine3D.Draw(pos.vector.X, pos.vector.Y, Color.Blue);
        }

        public void Look(List<Boundary> walls, LEDEngine3D engine3D)
        {
            List<Vector2> lineVectors = new List<Vector2>();
            foreach (Ray ray in rays)
            {
                Vector2D? closest = null;
                float record = 500000000;

                foreach (Boundary wall in walls)
                {
                    Vector2D? pt = ray.Cast(wall);
                    if (pt != null)
                    {
                        float d = Vector2.Distance(pos.vector, pt.Value.vector);
                        if (d < record)
                        {
                            record = d;
                            closest = pt.Value;
                        }
                    }
                }
                if (closest != null)
                {
                    lineVectors.AddRange(engine3D.GetLineVectors(pos.vector.X, pos.vector.Y, closest.Value.vector.X, closest.Value.vector.Y));
                }
            }

            engine3D.Draw(lineVectors, Color.White);
        }
    }
}
