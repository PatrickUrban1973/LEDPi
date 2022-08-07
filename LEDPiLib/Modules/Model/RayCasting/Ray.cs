using System;
using System.Numerics;

namespace LEDPiLib.Modules.Model.RayCasting
{
    public class Ray
    {
        private Vector2 pos;
        private Vector2 dir;

        public Ray(Vector2 pos, float angle)
        {
            this.pos = pos;
            this.dir = new Vector2((float) Math.Cos(angle), (float) Math.Sin(angle));
        }

        void lookAt(float x, float y)
        {
            dir.X = x - pos.X;
            dir.Y = y - pos.Y;
            dir = Vector2.Normalize(dir);
        }

        public void Update(float x, float y)
        {
            pos.X = x;
            pos.Y = y;
        }

        public Vector2? Cast(Boundary wall)
        {
            float x1 = wall.A.X;
            float y1 = wall.A.Y;
            float x2 = wall.B.X;
            float y2 = wall.B.Y;

            float x3 = this.pos.X;
            float y3 = this.pos.Y;
            float x4 = this.pos.X + this.dir.X;
            float y4 = this.pos.Y + this.dir.Y;

            float den = (x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4);
            if (den == 0)
            {
                return null;
            }

            float t = ((x1 - x3) * (y3 - y4) - (y1 - y3) * (x3 - x4)) / den;
            float u = -((x1 - x2) * (y1 - y3) - (y1 - y2) * (x1 - x3)) / den;
            if (t > 0 && t < 1 && u > 0)
            {
                return new Vector2(x1 + t * (x2 - x1), y1 + t * (y2 - y1));
            }
            else
            {
                return null;
            }
        }
    }
}
