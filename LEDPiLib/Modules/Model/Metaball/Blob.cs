using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using LEDPiLib.Modules.Helper;
using System.Numerics;

namespace LEDPiLib.Modules.Model.Metaball
{
    public class Blob
    {
        public Vector2 pos { get; private set; }
        public float r { get; private set; }

        Vector2 vel;

        public Blob(float x, float y)
        {
            pos = new Vector2(x, y);

            float velX = (float)(MathHelper.GlobalRandom().NextDouble() * MathHelper.GlobalRandom().Next(1, 2));
            float velY = (float)(MathHelper.GlobalRandom().NextDouble() * MathHelper.GlobalRandom().Next(1, 2));
            vel = new Vector2(velX, velY);
            r = MathHelper.GlobalRandom().Next(9, 15);
        }

        public void Update(float width, float height)
        {
            pos += vel;
            if (pos.X > width || pos.X < 0)
            {
                vel.X *= -1;
            }
            if (pos.Y > height || pos.Y < 0)
            {
                vel.Y *= -1;
            }
        }

        public void Draw(Image<Rgba32> image)
        {
            image.Mutate(c => c.Draw(Color.White, .5f,  new ComplexPolygon(new EllipsePolygon(new PointF(pos.X, pos.Y), r))));
        }
    }
}
