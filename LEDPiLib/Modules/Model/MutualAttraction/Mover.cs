using LEDPiLib.Modules.Helper;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Numerics;

namespace LEDPiLib.Modules.Model.MutualAttraction
{
    public class Mover
    {
        private Vector2 pos;
        private Vector2 vel;
        private Vector2 acc;
        private readonly float mass;
        private readonly float r;
        private readonly Color color;

        public Mover(float x, float y, float vx, float vy, float m, Color color)
        {
            pos = new Vector2(x, y);
            vel = new Vector2(vx, vy);
            acc = new Vector2(0);
            mass = m;
            r = (float)Math.Sqrt(mass) * 2;
            this.color = color;
        }

        private void applyForce(Vector2 force)
        {
            acc += force/mass;
        }

        public void Attract(Mover mover)
        {
            Vector2 force = Vector2.Subtract(pos, mover.pos);
            float distanceSq = MathHelper.Constrain(force.LengthSquared(), 100, 300);
            float G = 45;
            float strength = (G * (this.mass * mover.mass)) / distanceSq;

            mover.applyForce(force * strength / force.Length());
        }

        public void Update()
        {
            vel = Vector2.Add(vel, acc);
            pos = Vector2.Add(pos, vel);
            acc = new Vector2(0);
        }

        public void Display(Image<Rgba32> image)
        {
            image.Mutate(c => c.Fill(color, new ComplexPolygon(new EllipsePolygon(new PointF(pos.X, pos.Y), r))));
        }
    }
}
