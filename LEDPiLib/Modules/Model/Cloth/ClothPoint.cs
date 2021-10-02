using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace LEDPiLib.Modules.Model.Cloth
{
    public class ClothPoint : ClothPointBase
    {
        private static float friction;
        private static float gravity;
        private static float bounce;
        private static float width;
        private static float height;

        public float OldX { get; set; }
        public float OldY { get; set; }

        public static void InitPoints(float f, float g, float b, float w, float h)
        {
            friction = f;
            gravity = g;
            bounce = b;
            width = w;
            height = h;
        }

        public ClothPoint(float x, float y, float oldx, float oldy, bool pinned = false):base(x,y,pinned)
        {
            OldX = oldx;
            OldY = oldy;
        }

        public override void Update()
        {
            if (!Pinned)
            {
                float vx = (X - OldX) * friction;
                float vy = (Y - OldY) * friction;

                OldX = X;
                OldY = Y;
                X += vx;
                Y += vy;
                Y += gravity;
            }
        }

        public override void ConstrainPoint()
        {
            if (!Pinned)
            {
                float vx = (X - OldX) * friction;
                float vy = (Y - OldY) * friction;

                if (X > width)
                {
                    X = width;
                    OldX = X + vx * bounce;
                }
                else if (X < 0)
                {
                    X = 0;
                    OldX = X + vx * bounce;
                }
                if (Y > height)
                {
                    Y = height;
                    OldY = Y + vy * bounce;
                }
                else if (Y < 0)
                {
                    Y = 0;
                    OldY = Y + vy * bounce;
                }
            }
        }

        public override void Display(Image<Rgba32> image)
        {
            image.Mutate(c => c.Draw(Color.White, 1, new ComplexPolygon(new EllipsePolygon(new PointF(X, Y), 1))));
        }
    }
}
