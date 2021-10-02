using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace LEDPiLib.Modules.Model.Molecule
{
    public class MoleculeParticle
    {
        public Vector2D Pos { get; set; }
        public Vector2D Vel { get; set; }
        public Vector2D Acc { get; private set; }

        public MoleculeParticle(float x, float y)
        {
            Pos = new Vector2D(x, y);
            Vel = new Vector2D();
            Acc = new Vector2D();
        }

        public void ApplyForce(Vector2D force)
        {
            Acc += force;
        }

        public void Update()
        {
            Vel += Acc;
            Pos += Vel;
            Acc *= 0;
        }

        public void Edges(float width, float height)
        {
            if (Pos.vector.X < 0)
            {
                Pos = new Vector2D(0, Pos.vector.Y);
                Vel = new Vector2D(Vel.vector.X * -1, Vel.vector.Y);
            }
            else if (Pos.vector.X > width)
            {
                Pos = new Vector2D(width, Pos.vector.Y);
                Vel = new Vector2D(Vel.vector.X * -1, Vel.vector.Y);
            }
            else if (Pos.vector.Y > height)
            {
                Pos = new Vector2D(Pos.vector.X, height);
                Vel = new Vector2D(Vel.vector.X, Vel.vector.Y * -1);
            }
        }

    }
}
