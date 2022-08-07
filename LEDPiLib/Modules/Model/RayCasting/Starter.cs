using System.Collections.Generic;
using System.Numerics;
using LEDPiLib.Modules.Helper;
using SixLabors.ImageSharp;

namespace LEDPiLib.Modules.Model.RayCasting
{
    class Starter
    {
        private Vector2 pos;
        private List<Ray> rays;

        public Starter(float x, float y)
        {
            pos = new Vector2(x, y);
            rays = new List<Ray>();

            for (int i = 0; i < 360; i+=5)
            {
                rays.Add(new Ray(pos, i));
            }
        }

        public void Update(float x, float y)
        {
            this.pos.X = x;
            this.pos.Y = y;

            rays.ForEach(c => c.Update(x,y));
        }

        public void Show(LEDEngine3D engine3D)
        {
            engine3D.Draw(pos.X, pos.Y, Color.Blue);
        }

        public void Look(List<Boundary> walls, LEDEngine3D engine3D)
        {
            List<Vector2> lineVectors = new List<Vector2>();
            foreach (Ray ray in rays)
            {
                Vector2? closest = null;
                float record = 500000000;

                foreach (Boundary wall in walls)
                {
                    Vector2? pt = ray.Cast(wall);
                    if (pt != null)
                    {
                        float d = Vector2.Distance(pos, pt.Value);
                        if (d < record)
                        {
                            record = d;
                            closest = pt.Value;
                        }
                    }
                }
                if (closest != null)
                {
                    lineVectors.AddRange(engine3D.GetLineVectors(pos.X, pos.Y, closest.Value.X, closest.Value.Y));
                }
            }

            engine3D.Draw(lineVectors, Color.White);
        }
    }
}
