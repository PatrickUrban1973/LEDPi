using System;
using System.Numerics;
using LEDPiLib.Modules.Helper;
using LEDPiLib.Modules.Model.Common;
using SixLabors.ImageSharp;

namespace LEDPiLib.Modules.Model.Museum
{
    public class Bullet : DrawObjectBase
    {
        private readonly Vector2 size;
        
        public Bullet(Vector2 position, Vector2 maxBounds):base(position, maxBounds)
        {
            this.size = new Vector2(2, 2);
        }

        public override void Move()
        {
            position.Y -= 1;
        }

        public Tuple<Vector2, Vector2> GetSize()
        {
            return new Tuple<Vector2, Vector2>(position, size);
        }

        public override void Draw(LEDEngine3D engine3D)
        {
            engine3D.DrawFilledRectangle(new Rectangle(new Vector2(position.X, position.Y),
                size, Color.Green));
        }
    }
}