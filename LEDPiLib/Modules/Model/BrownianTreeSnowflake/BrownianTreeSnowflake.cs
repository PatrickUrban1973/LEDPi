using System.Collections.Generic;
using System.Numerics;
using LEDPiLib.Modules.Helper;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace LEDPiLib.Modules.Model.BrownianTreeSnowflake
{
    public class BrownianTreeSnowflake
    {
        private Vector2 pos;
        private readonly float r;

        public BrownianTreeSnowflake(float radius, float angle)
        {
            pos = MathHelper.RadianToVector2D(angle);
            pos *= radius;
            r = 3;
        }

        public void Update()
        {
            pos.X -= 1;
            pos.Y +=  MathHelper.GlobalRandom().Next(-3, 4);
        }

        public void Render(ref Image<Rgba32> img, float xOffset, float yOffset)
        {
            img.Mutate(c => c.Fill(Color.White, new ComplexPolygon(new EllipsePolygon(new PointF(pos.X + xOffset, pos.Y + yOffset), r))));
        }

        public bool Intersects(List<BrownianTreeSnowflake> particles)
        {
            bool result = false;
            foreach (BrownianTreeSnowflake part in particles)
            {
                float d = Vector2.Distance(pos, part.pos);
                if (d < r * 2)
                {
                    result = true;
                    break;
                }
            }
            return result;
        }

        public bool Finished()
        {
            return pos.X < 1;
        }

    }
}
