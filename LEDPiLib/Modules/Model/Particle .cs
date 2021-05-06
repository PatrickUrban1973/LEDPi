
using LEDPiLib.Modules.Helper;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;


namespace LEDPiLib.Modules.Model
{
    class Particle
    {
        private Random random = new Random();

        private List<Vector2D> pastLocations = new List<Vector2D>();

        private Vector2D velocity;
        private Vector2D acceleration;
        float lifespan;

        private bool seed = false;

        Vector3D hu;

        public Particle(float x, float y, Vector3D h)
        {
            hu = h;

            acceleration = new Vector2D(0, 0);
            velocity = new Vector2D(0, random.Next(-12, -5));
            Location = new Vector2D(x, y);
            seed = true;
            lifespan = 255.0f;
        }

        public Particle(Vector2D l, Vector3D h)
        {
            hu = h;
            acceleration = new Vector2D(0, 0);
            velocity = new Vector2D(Convert.ToSingle((random.NextDouble() * 2) - 1), Convert.ToSingle((random.NextDouble() * 2) - 1));

            velocity = velocity * Convert.ToSingle(random.Next(2, 8));
            Location = l;
            lifespan = 255.0f;
        }

        public Vector2D Location { get; set; }

        public void ApplyForce(Vector2D force)
        {
            acceleration = acceleration + force;
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
            velocity = velocity + acceleration;
            Location = Location + velocity;
            if (!seed)
            {
                lifespan -= 5.0f;
                pastLocations.Add(Location);
                velocity = velocity * 0.75f;
            }
            acceleration = acceleration * 0;
        }

        // Method to display
        public void Display(LEDEngine3D engine3D)
        {
            if (lifespan < 0)
                return;

            Vector3D lifeTimeHu = hu.Sub(255 - lifespan, false);

            Rgba32 pixel = new Rgba32(Convert.ToByte(lifeTimeHu.vector.X), Convert.ToByte(lifeTimeHu.vector.Y), Convert.ToByte(lifeTimeHu.vector.Z));

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
                Vector3D trailHu = lifeTimeHu;

                for (int i = pastLocations.Count - 1; i >= 0; i--)
                {
                    bool outOfColor = trailHu.vector.X < 0 && trailHu.vector.Y < 0 && trailHu.vector.Z < 0;

                    if (outOfColor)
                        break;

                    Vector2D pastLocation = pastLocations[i];
                    engine3D.Draw(pastLocation.X, pastLocation.Y, new Rgba32(Convert.ToByte(trailHu.vector.X), Convert.ToByte(trailHu.vector.Y), Convert.ToByte(trailHu.vector.Z)));
                    
                    trailHu = trailHu.Sub(5f, false);
                }

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
