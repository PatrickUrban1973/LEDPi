using System.Numerics;

namespace LEDPiLib.Modules.Model
{
    public struct Vector2D
    {
        public Vector2 vector;
        public float W;

        public Vector2D(float x, float y, float w = 1)
        {
            this.vector = new Vector2(x, y);
            W = w;
        }

        public Vector2D(Vector2 vector, float w = 1)
        {
            this.vector = vector;
            W = w;
        }

        public static Vector2D operator +(Vector2D v1, Vector2D v2) 
        {
            return new Vector2D(new Vector2(v1.vector.X + v2.vector.X, v1.vector.Y + v2.vector.Y));
        }

        public static Vector2D operator -(Vector2D v1, Vector2D v2)
        {
            return new Vector2D(new Vector2(v1.vector.X - v2.vector.X, v1.vector.Y - v2.vector.Y));
        }

        public static Vector2D operator *(Vector2D v1, float k)
        {
            return new Vector2D(new Vector2(v1.vector.X * k, v1.vector.Y * k));
        }
    }
}
