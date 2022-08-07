using LEDPiLib.Modules.Helper;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Numerics;

namespace LEDPiLib.Modules.Model.Snowflake
{
    public class Snowflake
    {
        private readonly int width;
        private readonly int height;

        private Vector2 pos;
        private Vector2 vel;
        private Vector2 acc;
        private float r;


        public Snowflake(float? sx, float? sy, int width, int height)
        {
            this.width = width;
            this.height = height;
            float x = sx ?? MathHelper.GlobalRandom().Next(width);
            float y = sy ?? MathHelper.GlobalRandom().Next(-100, -10);

            pos = new Vector2(x, y);
            vel = new Vector2(0, 0);
            acc = new Vector2();
            r = getRandomSize();
        }

        public Vector2 Pos { get { return pos; } }

        public void ApplyForce(Vector2 force)
        {
            // Parallax Effect hack
            Vector2 f = new Vector2(force.X, force.Y);
            f *= r;

            // let f = force.copy();
            // f.div(this.mass);
            acc += f;
        }

        public void Update()
        {
            vel += this.acc;
            vel = Vector2.Clamp(vel, new Vector2(0, 0), new Vector2(r * 0.2f));

            if (MathHelper.Mag(vel) < 1)
            {
                vel = Vector2.Normalize(vel);
            }

            pos += vel;
            acc *= 0;

            if (pos.Y > height + r)
            {
                randomize();
            }

            // Wrapping Left and Right
            if (pos.X < -r)
            {
                pos.X = width + r;
            }
            if (pos.X > width + r)
            {
                pos.X = -r;
            }
        }

        public void Render(ref Image<Rgba32> img)
        {
            float internalR = r / 4;

            if (pos.Y - internalR < 0)
                return;

            img.Mutate(c => c.Fill(Color.White, new ComplexPolygon(new EllipsePolygon(new PointF(pos.X, pos.Y), internalR))));
        }

        private float getRandomSize()
        {
            float localR = (float)Math.Pow(MathHelper.GlobalRandom().NextDouble(), 3);
            return MathHelper.Constrain(localR * 16, 1, 16);
        }

        private void randomize()
        {
            float x = MathHelper.GlobalRandom().Next(width);
            float y = MathHelper.GlobalRandom().Next(-100, -10);
            this.pos = new Vector2(x, y);
            this.vel = new Vector2(0, 0);
            this.acc = new Vector2();
            this.r = getRandomSize();
        }


    }
}
