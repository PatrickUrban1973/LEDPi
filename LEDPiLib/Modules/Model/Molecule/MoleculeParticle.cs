using System.Numerics;

namespace LEDPiLib.Modules.Model.Molecule
{
    public class MoleculeParticle
    {
        public Vector2 Pos { get; set; }
        public Vector2 Vel { get; set; }
        public Vector2 Acc { get; private set; }

        public MoleculeParticle(float x, float y)
        {
            Pos = new Vector2(x, y);
            Vel = new Vector2();
            Acc = new Vector2();
        }

        public void ApplyForce(Vector2 force)
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
            if (Pos.X < 0)
            {
                Pos = new Vector2(0, Pos.Y);
                Vel = new Vector2(Vel.X * -1, Vel.Y);
            }
            else if (Pos.X > width)
            {
                Pos = new Vector2(width, Pos.Y);
                Vel = new Vector2(Vel.X * -1, Vel.Y);
            }
            else if (Pos.Y > height)
            {
                Pos = new Vector2(Pos.X, height);
                Vel = new Vector2(Vel.X, Vel.Y * -1);
            }
        }

    }
}
