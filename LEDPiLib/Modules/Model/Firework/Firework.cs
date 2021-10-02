using LEDPiLib.Modules.Helper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LEDPiLib.Modules.Model
{
    public class Firework
    {
        List<Particle> particles;    // An arraylist for all the particles
        Particle firework;
        Vector3D hu;

        private Random random = new Random();

        public static Vector2D Gravity { get; set; }

        public Firework(int width, int height)
        {
            hu = new Vector3D(random.Next(255), random.Next(255), random.Next(255));
            firework = new Particle(random.Next(width), height, hu);
            particles = new List<Particle>();   // Initialize the arraylist
        }

        public bool Done()
        {
            if (firework == null && !particles.Any())
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void Run(LEDEngine3D engine3D)
        {
            if (firework != null)
            {
                firework.ApplyForce(Gravity);
                firework.Update();
                firework.Display(engine3D);

                if (firework.Explode())
                {
                    for (int i = 0; i < 100; i++)
                    {
                        particles.Add(new Particle(firework.Location, hu));    // Add "num" amount of particles to the arraylist
                    }
                    firework = null;
                }
            }

            for (int i = particles.Count - 1; i >= 0; i--)
            {
                Particle p = particles[i];
                p.ApplyForce(Gravity);
                p.Run(engine3D);
                if (p.IsDead())
                {
                    particles.RemoveAt(i);
                }
            }
        }


        // A method to test if the particle system still has particles
        private bool dead()
        {
            if (!particles.Any())
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
