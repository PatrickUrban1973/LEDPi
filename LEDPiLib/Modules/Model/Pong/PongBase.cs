using System.Numerics;
using LEDPiLib.Modules.Helper;
using LEDPiLib.Modules.Model.Common;
using SixLabors.ImageSharp;

namespace LEDPiLib.Modules.Model.Pong
{
    abstract class PongBase : DrawObjectBase
    {
        protected Rectangle rectangle;

        protected PongBase(Vector2 pos, Vector2 size, Vector2 maxBounds):base(pos, maxBounds)
        {
            rectangle = new Rectangle(position, size, Color.White);
        }
        
        public override void Draw(LEDEngine3D engine3D)
        {
            engine3D.DrawFilledRectangle(rectangle);
        }
        
    }
}