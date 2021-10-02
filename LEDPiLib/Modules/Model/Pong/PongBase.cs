using LEDPiLib.Modules.Helper;
using SixLabors.ImageSharp;

namespace LEDPiLib.Modules.Model.Pong
{
    abstract class PongBase
    {
        protected Rectangle rectangle;
        protected Vector2D maxBounds;

        public PongBase(Vector2D pos, Vector2D size, Vector2D maxBounds)
        {
            this.rectangle = new Rectangle(pos, size, Color.White);
            this.maxBounds = maxBounds;
        }

        public void Draw(LEDEngine3D engine3D)
        {
            engine3D.DrawFilledRectangle(rectangle);
        }

        public abstract void Move();
    }
}
