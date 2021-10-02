using System;
using System.Numerics;

namespace LEDPiLib.Modules.Model
{
    public struct Mat4x4
    {
        public Matrix4x4 M;

        public Mat4x4(Matrix4x4 m)
        {
            M = m;
		
        }

        private static Mat4x4 vecToMatrix(Vector4D v)
        {
            Mat4x4 m = new Mat4x4();
            m.M.M11 = v.vector.X;
            m.M.M21 = v.vector.Y;
            m.M.M31 = v.vector.Z;
            m.M.M41 = v.vector.W;
            return m;
        }

        private static Vector3D matrixToVec(Mat4x4 m)
        {
            return new Vector3D(m.M.M11, m.M.M21, m.M.M31);
        }

        private static Vector4D matrixToVec4(Mat4x4 m)
        {
            return new Vector4D(m.M.M11, m.M.M21, m.M.M31, m.M.M41);
        }

        public static Vector3D operator *(Mat4x4 m, Vector4D v1)
        {
            return matrixToVec(m * vecToMatrix(v1));
        }

        public static Vector4D MatMul(Mat4x4 m, Vector4D v1)
        {
            return matrixToVec4(m * vecToMatrix(v1));
        }

        public static Vector3D operator *(Mat4x4 m, Vector3D v1)
        {
            return new Vector3D(v1.vector.X * m.M.M11 + v1.vector.Y * m.M.M21 + v1.vector.Z * m.M.M31 + v1.W * m.M.M41,
                         v1.vector.X * m.M.M12 + v1.vector.Y * m.M.M22 + v1.vector.Z * m.M.M32 + v1.W * m.M.M42,
                         v1.vector.X * m.M.M13 + v1.vector.Y * m.M.M23 + v1.vector.Z * m.M.M33 + v1.W * m.M.M43,
                         v1.vector.X * m.M.M14 + v1.vector.Y * m.M.M24 + v1.vector.Z * m.M.M34 + v1.W * m.M.M44);
        }

        public static Mat4x4 operator *(Mat4x4 m1, Mat4x4 m2)
		{
			return new Mat4x4(m1.M * m2.M);
		}

		public static Mat4x4 MakeIdentity()
		{
			return new Mat4x4(Matrix4x4.Identity);
		}

		public static Mat4x4 MakeRotationX(float fAngleRad)
		{
			return new Mat4x4(Matrix4x4.CreateRotationX(fAngleRad));
		}

		public static Mat4x4 MakeRotationY(float fAngleRad)
		{
            return new Mat4x4(Matrix4x4.CreateRotationY(fAngleRad));
		}

		public static Mat4x4 MakeRotationZ(float fAngleRad)
		{
            return new Mat4x4(Matrix4x4.CreateRotationZ(fAngleRad));
		}

		public static Mat4x4 MakeTranslation(float x, float y, float z)
		{
            return new Mat4x4(Matrix4x4.CreateTranslation(x,y,z));
		}

		public static Mat4x4 MakeProjection(float fFovDegrees, float fAspectRatio, float fNear, float fFar)
		{
			float fFovRad = 1.0f / Convert.ToSingle(Math.Tan(fFovDegrees * 0.5f / 180.0f * 3.14159f));
			Mat4x4 matrix = new Mat4x4(new Matrix4x4());
			matrix.M.M11 = fAspectRatio * fFovRad;
			matrix.M.M22 = fFovRad;
			matrix.M.M33 = fFar / (fFar - fNear);
			matrix.M.M43 = (-fFar * fNear) / (fFar - fNear);
			matrix.M.M34 = 1.0f;
			matrix.M.M44 = 0.0f;
			return matrix;
		}

		public static Mat4x4 PointAt(Vector3D pos, Vector3D target, Vector3D up)
        {
            // Calculate new forward direction
            Vector3D newForward = target - pos;
            newForward = newForward.Normalise();

            // Calculate new Up direction
            Vector3D a = newForward * (up * newForward);
            Vector3D newUp = up - a;
            newUp = newUp.Normalise();

            // New Right direction is easy, its just cross product
            Vector3D newRight = newUp.CrossProduct(newForward);

            // Construct Dimensioning and Translation Matrix	
            Mat4x4 matrix = new Mat4x4();
            matrix.M.M11 = newRight.vector.X; 
            matrix.M.M12 = newRight.vector.Y; 
            matrix.M.M13 = newRight.vector.Z; 
            matrix.M.M14 = 0.0f;
            matrix.M.M21 = newUp.vector.X; 
            matrix.M.M22 = newUp.vector.Y; 
            matrix.M.M23 = newUp.vector.Z; 
            matrix.M.M24 = 0.0f;
            matrix.M.M31 = newForward.vector.X; 
            matrix.M.M32 = newForward.vector.Y; 
            matrix.M.M33 = newForward.vector.Z; 
            matrix.M.M34 = 0.0f;
            matrix.M.M41 = pos.vector.X; 
            matrix.M.M42 = pos.vector.Y; 
            matrix.M.M43 = pos.vector.Z; 
            matrix.M.M44 = 1.0f;
            return matrix;
		}

		internal Mat4x4 QuickInverse() // Only for Rotation/Translation Matrices
		{
            Mat4x4 matrix = new Mat4x4(new Matrix4x4());

            matrix.M.M11 = this.M.M11; matrix.M.M12 = this.M.M21; matrix.M.M13 = this.M.M31; matrix.M.M14 = 0.0f;
            matrix.M.M21 = this.M.M12; matrix.M.M22 = this.M.M22; matrix.M.M23 = this.M.M32; matrix.M.M24 = 0.0f;
            matrix.M.M31 = this.M.M13; matrix.M.M32 = this.M.M23; matrix.M.M33 = this.M.M33; matrix.M.M34 = 0.0f;
            matrix.M.M41 = -(this.M.M41 * matrix.M.M11 + this.M.M42 * matrix.M.M21 + this.M.M43 * matrix.M.M31);
            matrix.M.M42 = -(this.M.M41 * matrix.M.M12 + this.M.M42 * matrix.M.M22 + this.M.M43 * matrix.M.M32);
            matrix.M.M43 = -(this.M.M41 * matrix.M.M13 + this.M.M42 * matrix.M.M23 + this.M.M43 * matrix.M.M33);
            matrix.M.M44 = 1.0f;

			return matrix;
        }


	}
}
