using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using LEDPiLib.Modules.Helper;
using System.Numerics;

namespace LEDPiLib.Modules.Model.SpaceColonization
{
    public class Leaf
    {
        public Vector2 Pos { get; set; }

        public Leaf(int maxWidth, int maxHeight)
        {
            Pos = CalculatePoint(maxWidth/2, maxHeight/3.5f, (maxWidth)-100);
        }

        public bool Reached { get; set; }

        public void Show(Image<Rgba32> image)
        {
            if (Pos.X >= 0 && Pos.X < image.Width && Pos.Y >= 0 && Pos.Y < image.Height)
                image.Mutate(c => c.Draw(Color.Green, 1f, new ComplexPolygon(new EllipsePolygon(new PointF(Pos.X, Pos.Y), 1))));
        }

        private Vector2 CalculatePoint(float originX, float originY, float originRadius)
        {
            var angle = MathHelper.GlobalRandom().NextDouble() * Math.PI * 2;
            var radius = MathHelper.GlobalRandom().NextDouble() * originRadius;
            var x = originX + radius * Math.Cos(angle);
            var y = originY + radius * Math.Sin(angle);
            return new Vector2((float)x, (float)y);
        }
    }
}
