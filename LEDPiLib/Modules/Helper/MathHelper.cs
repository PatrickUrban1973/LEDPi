using System;
using System.Numerics;

namespace LEDPiLib.Modules.Helper
{
    static class MathHelper
    {
        private static readonly Random _random  = new Random();

        public static float Constrain(float constrainValue, float constrainMin, float constrainMax)
        {
            return constrainValue < constrainMin
                ? constrainMin
                : (constrainValue > constrainMax ? constrainMax : constrainValue);
        }

        public static float Map(float s, float a1, float a2, float b1, float b2)
        {
            return b1 + (s - a1) * (b2 - b1) / (a2 - a1);
        }

        public static float Lerp(float a, float b, float x)
        {
            return a + x * (b - a);
        }

        public static Vector2 RadianToVector2D(float radian)
        {
            return new Vector2((float)Math.Cos(radian), (float)Math.Sin(radian));
        }
        public static Vector2 RadianToVector2D(float radian, float length)
        {
            return RadianToVector2D(radian) * length;
        }

        public static float Mag(Vector2 vector)
        {
            return (float)Math.Sqrt((vector.X * vector.X) + (vector.Y * vector.Y));
        }

        public static Vector3 Sub(Vector3 vector, float f, bool allowNegativ = true)
        {
            Vector3 ret = new Vector3(vector.X - f, vector.Z - f, vector.Z - f);

            if (!allowNegativ)
            {
                if (ret.X < 0) ret.X = 0;
                if (ret.Y < 0) ret.Y = 0;
                if (ret.Z < 0) ret.Z = 0;
            }

            return ret;
        }

        public static Matrix4x4 MakeProjection(float fFovDegrees, float fAspectRatio, float fNear, float fFar)
        {
            float fFovRad = 1.0f / (float)Math.Tan(fFovDegrees * 0.5f / 180.0f * 3.14159f);
            Matrix4x4 matrix = new Matrix4x4();
            matrix.M11 = fAspectRatio * fFovRad;
            matrix.M22 = fFovRad;
            matrix.M33 = fFar / (fFar - fNear);
            matrix.M43 = (-fFar * fNear) / (fFar - fNear);
            matrix.M34 = 1.0f;
            matrix.M44 = 0.0f;
            return matrix;
        }


        public static Random GlobalRandom()
        {
            return _random;
        }
    }
}
