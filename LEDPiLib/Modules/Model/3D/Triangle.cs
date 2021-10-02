using SixLabors.ImageSharp;
using System.Collections.Generic;

namespace LEDPiLib.Modules.Model
{
    public struct Triangle
    {
        public Triangle(List<Vector3D> p)
        {
            this.P = p;
            color = Color.Transparent;
        }

        public List<Vector3D> P;
        public Color color;
    }
}
