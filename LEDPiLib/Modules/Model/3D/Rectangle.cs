using SixLabors.ImageSharp;
using System.Collections.Generic;

namespace LEDPiLib.Modules.Model
{
    public struct Rectangle
    {
        public Rectangle(Vector2D pos, Vector2D size, Color col)
        {
            this.Pos = pos;
            this.Size = size;
            color = col;
        }

        public Vector2D Pos;
        public Vector2D Size;
        public Color color;
    }
}
