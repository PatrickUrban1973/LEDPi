using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace LEDPiLib.Modules.Model.Cloth
{
    public abstract class ClothPointBase
    {
        public float X { get; set; }
        public float Y { get; set; }
        public bool Pinned { get; set; }

        protected ClothPointBase(float x, float y, bool pinned = false)
        {
            X = x;
            Y = y;
            Pinned = pinned;
        }

        public abstract void Update();


        public virtual void ConstrainPoint()
        {
        }

        public abstract void Display(Image<Rgba32> image);
    }
}
