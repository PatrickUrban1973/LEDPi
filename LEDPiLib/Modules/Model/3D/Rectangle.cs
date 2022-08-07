using SixLabors.ImageSharp;
using System.Numerics;

namespace LEDPiLib.Modules.Model
{
    public struct Rectangle
    {
        public Rectangle(Vector2 pos, Vector2 size, Color col)
        {
            this.Pos = pos;
            this.Size = size;
            color = col;
        }

        public Vector2 Pos;
        public Vector2 Size;
        public Color color;
    }
}
