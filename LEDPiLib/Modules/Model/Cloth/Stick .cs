using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace LEDPiLib.Modules.Model.Cloth
{
    public class Stick
    {
        private ClothPointBase P0 { get; set; }
        private ClothPointBase P1 { get; set; }

        private bool Hidden { get; set; }

        private float Length { get; set; }

    public Stick(ClothPointBase p0, ClothPointBase p1, float length, bool hidden = false)
        {
            P0 = p0;
            P1 = p1;
            Hidden = hidden;
            Length = length;
        }

        public void UpdateStick()
        {
            float dx = P1.X - P0.X;
            float dy = P1.Y - P0.Y;
            float distance = (float)Math.Sqrt(dx * dx + dy * dy);
            float difference = Length - distance;
            float percent = difference / distance / 2f;
            float offsetX = dx * percent;
            float offsetY = dy * percent;

            if (!P0.Pinned)
            {
                P0.X -= offsetX;
                P0.Y -= offsetY;
            }
            if (!P1.Pinned)
            {
                P1.X += offsetX;
                P1.Y += offsetY;
            }
        }

        public void Display(Image<Rgba32> image)
        {
            if (Hidden)
                return;

            image.Mutate(c => c.DrawLines(Color.White, .1f, new[] { new PointF(P0.X, P0.Y), new PointF(P1.X, P1.Y) }));
        }
    }
}
