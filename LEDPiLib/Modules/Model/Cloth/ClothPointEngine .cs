using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace LEDPiLib.Modules.Model.Cloth
{
    public class ClothPointEngine : ClothPointBase
    {
        public float BaseX { get; set; }
        public float BaseY { get; set; }
        public float Range { get; set; }
        public float Angle { get; set; }
        public float Speed { get; set; }


        public ClothPointEngine(float x, float y, float basex, float basey, float range, float angle, float speed):base(x,y,true)
        {
            BaseX = basex;
            BaseY = basey;
            Range = range;
            Angle = angle;
            Speed = speed;
        }

        public override void Update()
        {
            X = Convert.ToSingle(BaseX + Math.Cos(Angle) * Range);
            Angle += Speed;
        }

        public override void Display(Image<Rgba32> image)
        {
            image.Mutate(c => c.DrawLines(Color.LightBlue, .1f, new[] { new PointF(BaseX, BaseY), new PointF(BaseX + Range, BaseY) }));
        }
    }
}
