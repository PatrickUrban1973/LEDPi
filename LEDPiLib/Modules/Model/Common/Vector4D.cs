using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace LEDPiLib.Modules.Model
{
    public struct Vector4D
    {
        public Vector4 vector;

        public Vector4D(Vector4 vector)
        {
            this.vector = vector;
        }

        public Vector4D(float x, float y, float z, float w)
        {
            vector = new Vector4(x, y, z, w);
        }

        public static Vector4D operator +(Vector4D v1, float k)
        {
            return new Vector4D(v1.vector.X + k, v1.vector.Y + k, v1.vector.Z + k, v1.vector.W);
        }

        public static Vector4D operator +(Vector4D v1, Vector4D v2)
        {
            return new Vector4D(v1.vector + v2.vector);
        }

        public static Vector4D operator -(Vector4D v1, Vector4D v2)
        {
            return new Vector4D(v1.vector - v2.vector);
        }

        public static Vector4D operator *(Vector4D v1, float k)
        {
            return new Vector4D(v1.vector * k);
        }
        
        public static float operator *(Vector4D v1, Vector4D v2)
        {
            return Vector4.Dot(v1.vector, v2.vector);
        }

        public static Vector4D operator /(Vector4D v1, float k)
        {
            return new Vector4D(v1.vector / k);
        }

        public Vector4D Sub(float f, bool allowNegativ = true)
        {
            Vector4D ret = new Vector4D(this.vector.X - f, this.vector.Z - f, this.vector.Z - f, this.vector.W - f);

            if (!allowNegativ)
            {
                if (ret.vector.X < 0) ret.vector.X = 0;
                if (ret.vector.Y < 0) ret.vector.Y = 0;
                if (ret.vector.Z < 0) ret.vector.Z = 0;
                if (ret.vector.W < 0) ret.vector.W = 0;
            }

            return ret;
        }

        internal float Length()
        {
            return vector.Length();
        }

        internal Vector4D Normalise()
        {
            return new Vector4D(Vector4.Normalize(this.vector));
        }
    }
}
