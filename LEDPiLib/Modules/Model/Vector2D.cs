namespace LEDPiLib.Modules.Model
{
    public class Vector2D
    {
        public Vector2D(float x, float y, float w = 1)
        {
            X = x;
            Y = y;
            W = w;
        }

        public float X { get; set; } = 0;
        public float Y { get; set; } = 0;
        public float W { get; set; } = 1;

        public static Vector2D operator +(Vector2D v1, Vector2D v2) 
        {
            return new Vector2D(v1.X + v2.X, v1.Y + v2.Y);
        }

        public static Vector2D operator -(Vector2D v1, Vector2D v2)
        {
            return new Vector2D(v1.X - v2.X, v1.Y - v2.Y);
        }

        public static Vector2D operator *(Vector2D v1, float k)
        {
            return new Vector2D(v1.X * k, v1.Y * k);
        }

    }
}
