using System;
using LEDPiLib.Modules.Helper;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace LEDPiLib.Modules.Model.Starfield
{
    public class Star
    {
        private float x;
        private float y;
        private float z;

        private float pz;
        private Random random = new Random();

        public Star()
        {
            x = random.Next(-Convert.ToInt32(RenderWidth / 2), Convert.ToInt32(RenderWidth / 2));
            y = random.Next(-Convert.ToInt32(RenderHeight / 2), Convert.ToInt32(RenderHeight / 2));
            z = random.Next(-Convert.ToInt32(RenderWidth / 2), Convert.ToInt32(RenderWidth / 2));
            pz = z;
        }


        public static float RenderWidth { get; set; } = 0;
        public static float RenderHeight { get; set; } = 0;

        public void Update(float speed)
        {
            // In the formula to set the new stars coordinates
            // I'll divide a value for the "z" value and the outcome will be
            // the new x-coordinate and y-coordinate of the star.
            // Which means if I decrease the value of "z" (which is a divisor),
            // the outcome will be bigger.
            // Wich means the more the speed value is bigger, the more the "z" decrease,
            // and the more the x and y coordinates increase.
            // Note: the "z" value is the first value I updated for the new frame.
            z = z - speed;
            // when the "z" value equals to 1, I'm sure the star have passed the
            // borders of the canvas( probably it's already far away from the borders),
            // so i can place it on more time in the canvas, with new x, y and z values.
            // Note: in this way I also avoid a potential division by 0.
            if (z < 1)
            {
                x = random.Next(-Convert.ToInt32(RenderWidth / 2), Convert.ToInt32(RenderWidth / 2));
                y = random.Next(-Convert.ToInt32(RenderHeight / 2), Convert.ToInt32(RenderHeight / 2));
                z = random.Next(-Convert.ToInt32(RenderWidth / 2), Convert.ToInt32(RenderWidth / 2));
                pz = z;
            }
        }

        public void Show(ref Image<Rgba32> image)
        {
            // with theese "map", I get the new star positions
            // the division x / z get a number between 0 and a very high number,
            // we map this number (proportionally to a range of 0 - 1), inside a range of 0 - width/2.
            // In this way we are sure the new coordinates "sx" and "sy" move faster at each frame
            // and which they finish their travel outside of the canvas (they finish when "z" is less than a).

            float sx = MathHelper.Map(x / z, 0, 1, 0, RenderWidth / 2);
            float sy = MathHelper.Map(y / z, 0, 1, 0, RenderHeight / 2);

            // I use the z value to increase the star size between a range from 0 to 16.
            float r = MathHelper.Map(z, 0, RenderWidth / 2, 16, 0);

            IPen pen = Pens.Dot(Color.Gray, 0.5f);
   //         image.Mutate(c => c.Draw(pen, new ComplexPolygon(new EllipsePolygon(new PointF(sx, sy), r))));

            // Here i use the "pz" valute to get the previous position of the stars,
            // so I can draw a line from the previous position to the new (current) one.
            float px = MathHelper.Map(x / pz, 0, 1, 0, RenderWidth / 2);
            float py = MathHelper.Map(y / pz, 0, 1, 0, RenderHeight / 2);

            // Placing here this line of code, I'm sure the "pz" value are updated after the
            // coordinates are already calculated; in this way the "pz" value is always equals
            // to the "z" value of the previous frame.
            pz = z;

            image.Mutate(c => c.DrawLines(Color.White, .1f,
                new[]
                {
                    new PointF((RenderWidth / 2) + px, (RenderHeight / 2) + py),
                    new PointF((RenderWidth / 2) + sx, (RenderHeight / 2) + sy)
                }));
        }
    }
}
