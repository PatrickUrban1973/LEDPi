using System.Numerics;
using LEDPiLib.Modules.Helper;
using LEDPiLib.Modules.Model.Common;
using SixLabors.ImageSharp;

namespace LEDPiLib.Modules.Model.Museum
{
    public class Explosion : DrawObjectBase
    {
        private Vector2 velocity;
        private Vector2 acceleration;
        private readonly Vector2 size;
        private readonly Vector2 gravity = new Vector2(0, 0.25f);
        private readonly Color color;
        
        public Explosion(Vector2 position, float size, Color color, Vector2 maxBounds):base(position, maxBounds)
        {
            this.size = new Vector2(size, size);
            this.color = color;
            acceleration = new Vector2(0, 0);
            velocity = new Vector2((float)((MathHelper.GlobalRandom().NextDouble() * 2) - 1), (float)((MathHelper.GlobalRandom().NextDouble() * 2) - 1));

            velocity *= new Vector2(MathHelper.GlobalRandom().Next(1, 6), MathHelper.GlobalRandom().Next(1, 5));
        }

        public override void Move()
        {
            velocity += acceleration;
            position += velocity;
            
            velocity *= 0.75f;
            acceleration *= 0;
            
            acceleration += gravity;
        }

        public override void Draw(LEDEngine3D engine3D)
        {
            engine3D.DrawFilledRectangle(new Rectangle(new Vector2(position.X, position.Y),
                size, color));
        }
    }
}