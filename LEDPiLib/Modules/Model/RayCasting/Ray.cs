using System;
using System.Numerics;
using LEDPiLib.Modules.Helper;
using SixLabors.ImageSharp;

namespace LEDPiLib.Modules.Model.RayCasting
{
    public class Ray
    {
        private Vector2D pos;
        private Vector2D dir;

        public Ray(Vector2D pos, float angle)
        {
            this.pos = pos;
            this.dir = new Vector2D((float) Math.Cos(angle), (float) Math.Sin(angle));
        }

        void lookAt(float x, float y)
        {
            dir.vector.X = x - pos.vector.X;
            dir.vector.Y = y - pos.vector.Y;
            dir = new Vector2D(Vector2.Normalize(dir.vector));
        }

        public void Update(float x, float y)
        {
            pos.vector.X = x;
            pos.vector.Y = y;
        }

        public Vector2D? Cast(Boundary wall)
        {
            float x1 = wall.A.vector.X;
            float y1 = wall.A.vector.Y;
            float x2 = wall.B.vector.X;
            float y2 = wall.B.vector.Y;

            float x3 = this.pos.vector.X;
            float y3 = this.pos.vector.Y;
            float x4 = this.pos.vector.X + this.dir.vector.X;
            float y4 = this.pos.vector.Y + this.dir.vector.Y;

            float den = (x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4);
            if (den == 0)
            {
                return null;
            }

            float t = ((x1 - x3) * (y3 - y4) - (y1 - y3) * (x3 - x4)) / den;
            float u = -((x1 - x2) * (y1 - y3) - (y1 - y2) * (x1 - x3)) / den;
            if (t > 0 && t < 1 && u > 0)
            {
                return new Vector2D(x1 + t * (x2 - x1), y1 + t * (y2 - y1));
            }
            else
            {
                return null;
            }
        }
    }
}
