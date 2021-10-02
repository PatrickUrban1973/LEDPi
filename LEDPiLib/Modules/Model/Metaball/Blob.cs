using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Text;

namespace LEDPiLib.Modules.Model.Metaball
{
    public class Blob
    {
        public Vector2D pos { get; private set; }
        public float r { get; private set; }

        Vector2D vel;

        public Blob(float x, float y)
        {
            Random random = new Random();
            pos = new Vector2D(x, y);

            float velX = Convert.ToSingle(Convert.ToSingle(random.NextDouble()) * random.Next(1, 2));
            float velY = Convert.ToSingle(Convert.ToSingle(random.NextDouble()) * random.Next(1, 2));
            vel = new Vector2D(velX, velY);
            r = random.Next(9, 15);
        }

        public void Update(float width, float height)
        {
            pos += vel;
            if (pos.vector.X > width || pos.vector.X < 0)
            {
                vel.vector.X *= -1;
            }
            if (pos.vector.Y > height || pos.vector.Y < 0)
            {
                vel.vector.Y *= -1;
            }
        }

        public void Draw(Image<Rgba32> image)
        {
            image.Mutate(c => c.Draw(Color.White, .5f,  new ComplexPolygon(new EllipsePolygon(new PointF(pos.vector.X, pos.vector.Y), r))));
        }
    }
}
