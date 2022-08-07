using System;
using System.Collections.Generic;
using LEDPiLib.Modules.Model.Common;

namespace LEDPiLib.Modules.Model
{
    public struct Mesh
    {
        public List<Triangle> Tris;

        public bool LoadFromObjectFile(string sFilename)
        {
            // Local cache of verts
            Stack<Vector3D> verts = new Stack<Vector3D>();
            Tris = new List<Triangle>();

            string[] lines = System.IO.File.ReadAllLines(sFilename);

            foreach (string line in lines)
            {
                string[] seperated = line.Split(' ');

                if (seperated[0] == "v")
                {
                    Vector3D v = new Vector3D(Convert.ToSingle(seperated[1]), Convert.ToSingle(seperated[2]), Convert.ToSingle(seperated[3]));
                    verts.Push(v);
                }

                if (seperated[0] == "f")
                {
                    Tris.Add(new Triangle(new List<Vector3D>()
                        {
                            verts.ToArray()[Convert.ToInt32(seperated[1]) - 1],
                            verts.ToArray()[Convert.ToInt32(seperated[2]) - 1],
                            verts.ToArray()[Convert.ToInt32(seperated[3]) - 1],
                        }));
                }
            }

            return true;
        }
    }
}
