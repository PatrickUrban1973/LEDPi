using System.Numerics;
using LEDPiLib.Modules.Helper;

namespace LEDPiLib.Modules.Model.Common
{
    public abstract class DrawObjectBase
    {
        protected Vector2 position;
        protected Vector2 maxBounds;

        protected DrawObjectBase(Vector2 position, Vector2 maxBounds)
        {
            this.position = position;
            this.maxBounds = maxBounds;
        }

        public abstract void Draw(LEDEngine3D engine3D);

        public abstract void Move();

        public bool OutOfBounds()
        {
            return (position.X < 0 || position.X > maxBounds.X || position.Y < 0 || position.Y > maxBounds.Y);
        }
    }
}
