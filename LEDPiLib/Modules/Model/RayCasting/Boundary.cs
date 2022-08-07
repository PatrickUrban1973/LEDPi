using LEDPiLib.Modules.Helper;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;
using System.Numerics;

namespace LEDPiLib.Modules.Model.RayCasting
{
    public class Boundary
    {
        public Vector2 A { get; set; }
        public Vector2 B { get; set; }

        public Boundary(float x1, float y1, float x2, float y2)
        {
            A = new Vector2(x1, y1);
            B = new Vector2(x2, y2);
        }

        public void Show(LEDEngine3D engine3D)
        {
            engine3D.Canvas.Mutate(c => c.DrawLines(Color.Blue, 1,
                new PointF[] {new PointF(A.X, A.Y), new PointF(B.X, B.Y)}));
        }
    }
}
