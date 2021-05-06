using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace LEDPiLib.Modules.Model
{
    public struct Vector3D
    {
        public Vector3 vector;
        public float W;

        public Vector3D(Vector3 vector, float w = 1)
        {
            this.vector = vector;
            W = w;
        }

        public Vector3D(float x, float y, float z, float w = 1)
        {
            vector = new Vector3(x, y, z);
            W = w;
        }

        public static Vector3D operator +(Vector3D v1, float k)
        {
            return new Vector3D(v1.vector.X + k, v1.vector.Y + k, v1.vector.Z + k);
        }

        public static Vector3D operator +(Vector3D v1, Vector3D v2)
        {
            return new Vector3D(v1.vector + v2.vector);
        }

        public static Vector3D operator -(Vector3D v1, Vector3D v2)
        {
            return new Vector3D(v1.vector - v2.vector);
        }

        public static Vector3D operator *(Vector3D v1, float k)
        {
            return new Vector3D(v1.vector * k);
        }
        
        public static float operator *(Vector3D v1, Vector3D v2)
        {
            return Vector3.Dot(v1.vector, v2.vector);
        }

        public static Vector3D operator /(Vector3D v1, float k)
        {
            return new Vector3D(v1.vector / k);
        }

        public Vector3D Sub(float f, bool allowNegativ = true)
        {
            Vector3D ret = new Vector3D(this.vector.X - f, this.vector.Z - f, this.vector.Z - f);

            if (!allowNegativ)
            {
                if (ret.vector.X < 0) ret.vector.X = 0;
                if (ret.vector.Y < 0) ret.vector.Y = 0;
                if (ret.vector.Z < 0) ret.vector.Z = 0;
            }

            return ret;
        }

        internal float Length()
        {
            return vector.Length();
        }

        internal Vector3D Normalise()
        {
            return new Vector3D(Vector3.Normalize(this.vector));
        }

        internal Vector3D CrossProduct(Vector3D v2)
        {
            return new Vector3D(Vector3.Cross(this.vector, v2.vector));
        }
    }
}
