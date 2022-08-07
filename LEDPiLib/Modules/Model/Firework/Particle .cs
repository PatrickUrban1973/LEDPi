using System;
using System.Numerics;
using LEDPiLib.Modules.Helper;
using SixLabors.ImageSharp.PixelFormats;


namespace LEDPiLib.Modules.Model.Firework
{
    class Particle
    {
        private Vector2 velocity;
        private Vector2 acceleration;
        private float lifespan;

        private readonly bool seed;

        private readonly Vector3 hu;

        public Particle(float x, float y, Vector3 h)
        {
            hu = h;

            acceleration = new Vector2(0, 0);
            velocity = new Vector2(0, MathHelper.GlobalRandom().Next(-12, -5));
            Location = new Vector2(x, y);
            seed = true;
            lifespan = 255.0f;
        }

        public Particle(Vector2 l, Vector3 h)
        {
            hu = h;
            acceleration = new Vector2(0, 0);
            velocity = new Vector2((float)((MathHelper.GlobalRandom().NextDouble() * 2) - 1), (float)((MathHelper.GlobalRandom().NextDouble() * 2) - 1));

            velocity *= MathHelper.GlobalRandom().Next(2, 8);
            Location = l;
            lifespan = 255.0f;
        }

        public Vector2 Location { get; private set; }

        public void ApplyForce(Vector2 force)
        {
            acceleration += force;
        }

        public void Run(LEDEngine3D engine3D)
        {
            Update();
            Display(engine3D);
        }

        public bool Explode()
        {
            if (seed && velocity.Y > 0)
            {
                lifespan = 0;
                return true;
            }
            return false;
        }

        // Method to update location
        public void Update()
        {
            velocity += acceleration;
            Location += velocity;
            if (!seed)
            {
                lifespan -= 5.0f;
                velocity *= 0.75f;
            }
            acceleration *= 0;
        }

        // Method to display
        public void Display(LEDEngine3D engine3D)
        {
            if (lifespan < 0)
                return;

            Vector3 lifeTimeHu = MathHelper.Sub(hu, 255 - lifespan, false);

            Rgba32 pixel = new Rgba32(Convert.ToByte(lifeTimeHu.X), Convert.ToByte(lifeTimeHu.Y), Convert.ToByte(lifeTimeHu.Z));

            if (seed)
            {
                engine3D.Draw(Location.X - 1, Location.Y, pixel);
                engine3D.Draw(Location.X, Location.Y - 1, pixel);
                engine3D.Draw(Location.X, Location.Y, pixel);
                engine3D.Draw(Location.X, Location.Y + 1, pixel);
                engine3D.Draw(Location.X + 1, Location.Y, pixel);
            }
            else
            {
                engine3D.Draw(Location.X, Location.Y, pixel);
            }
        }

        // Is the particle still useful?
        public bool IsDead()
        {
            if (lifespan < 0.0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
