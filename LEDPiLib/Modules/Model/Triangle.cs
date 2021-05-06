using SixLabors.ImageSharp;
using System.Collections.Generic;

namespace LEDPiLib.Modules.Model
{
    public class Triangle
    {
        public Triangle(List<Vector3D> p, List<Vector2D> t)
        {
            this.P = p;
            this.T = t;
        }

        public List<Vector3D> P { get; set; } = new List<Vector3D>(3);
        public List<Vector2D> T { get; set; } = new List<Vector2D>(3);
        public Color color { get; set; }
    }
}
