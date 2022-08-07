using SixLabors.ImageSharp;
using System.Collections.Generic;
using LEDPiLib.Modules.Model.Common;

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
