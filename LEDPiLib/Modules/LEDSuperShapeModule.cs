using System;
using System.Collections.Generic;
using System.Diagnostics;
using LEDPiLib.DataItems;
using LEDPiLib.Modules.Helper;
using LEDPiLib.Modules.Model;
using LEDPiLib.Modules.Model.Common;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using static LEDPiLib.LEDPIProcessorBase;

namespace LEDPiLib.Modules
{
    [LEDModule(LEDModules.SuperShape)]
    public class LEDSuperShapeModule : LEDEngine3DModuleBase
    {
        private struct SuperShapeObject
        {
            public SuperShapeObject(float m, float n1, float n2, float n3 )
            {
                M = m;
                N1 = n1;
                N2 = n2;
                N3 = n3;
            }

            public float M { get; }
            public float N1 { get; }
            public float N2 { get; }
            public float N3 { get; }
        }

        private struct SuperShapeSet
        {
            public SuperShapeSet(SuperShapeObject superShape1, SuperShapeObject superShape2)
            {
                SuperShape1 = superShape1;
                SuperShape2 = superShape2;
            }

            public SuperShapeObject SuperShape1 { get;}
            public SuperShapeObject SuperShape2 { get; }
        }

        private readonly List<SuperShapeSet> superShapeSets = new List<SuperShapeSet>()
        {
            new SuperShapeSet(
                new SuperShapeObject(2f,0.7f,0.3f,0.2f), 
                new SuperShapeObject(3,100,100,100)
                ),
            new SuperShapeSet(
                new SuperShapeObject(7f,0.2f,1.7f,1.7f),
                new SuperShapeObject(7f,0.2f,1.7f,1.7f)
            ),
            new SuperShapeSet(
                new SuperShapeObject(6f,1f,1f,1f),
                new SuperShapeObject(3f,1f,1f,1f)
            ),
            new SuperShapeSet(
                new SuperShapeObject(6f, 0.249778f, 47.8198f, -0.8625f),
                new SuperShapeObject(7f, -76.8867f, 0.521395f, -56.72f)
            ),
            new SuperShapeSet(
                new SuperShapeObject(6f,60f,55f,1000f),
                new SuperShapeObject(6f,250f,100f,100f)
            ),
            new SuperShapeSet(
                new SuperShapeObject(8f,60,100f,30f),
                new SuperShapeObject(2f,10f,10f,10f)
            ),
            new SuperShapeSet(
                new SuperShapeObject(0f,60,100f,30f),
                new SuperShapeObject(0f,10f,10f,10f)
            ),
        };

        private readonly Mat4x4 matProj; // Matrix that converts from view space to screen space
        private readonly Vector3D vCamera = new Vector3D(0f, 0f, -75f);  // Location of camera in world space
        private Vector3D vLookDir = new Vector3D(0f,0f,0f); // Direction vector along the direction camera points
        private readonly Vector3D light_direction = new Vector3D(0.0f, 1.0f, -1.0f);
        private const float fYaw = 0f;     // FPS Camera rotation in XZ plane
        private float fTheta;   // Spins World transform
        private float fElapsedTime;

        private float a = 1f;
        private float b = 1f;

        private readonly Dictionary<int, Dictionary<int, Vector3D>> globe = new Dictionary<int,Dictionary<int, Vector3D>>();
        private readonly int total = 30;
        private readonly float PI;
        private readonly float HALF_PI;

        private readonly Stopwatch stopwatch = new Stopwatch();

        private readonly Mesh meshCube;

        private readonly SuperShapeSet superShapeSet;

        public LEDSuperShapeModule(ModuleConfiguration moduleConfiguration) : base(moduleConfiguration, 3f)
        {
            PI = (float)Math.PI;
            HALF_PI = PI / 2;

            int index;

            if (String.IsNullOrEmpty(moduleConfiguration.Parameter))
            {
                index = MathHelper.GlobalRandom().Next(0, superShapeSets.Count);
            }
            else
            {
                index = Convert.ToInt32(moduleConfiguration.Parameter);
            }

            superShapeSet = superShapeSets[index];

            matProj = Mat4x4.MakeProjection(90.0f, 1, 0.1f, 1000f);
            meshCube.Tris = new List<Triangle>();
        }

        protected override bool completedRun()
        {
            return false;
        }

        protected override Image<Rgba32> RunInternal()
        {
            fElapsedTime = stopwatch.ElapsedMilliseconds / 1000f;
            stopwatch.Restart();
            image = GetNewImage();
            engine3D.Canvas = image;

            // Set up "World Transform" though not updating theta 
            // makes this a bit redundant
			fTheta += 0.01f * fElapsedTime; // Uncomment to spin me right round baby right round
            Mat4x4 matRotZ = Mat4x4.MakeRotationZ(10 * fTheta);
            Mat4x4 matRotX = Mat4x4.MakeRotationX(35f * fTheta);
            Mat4x4 matRotY = Mat4x4.MakeRotationY(45f * fTheta);

            Mat4x4 matTrans = Mat4x4.MakeTranslation(0.0f, 0.0f, 5.0f);

            Mat4x4 matWorld = matRotZ * matRotX * matRotY; // Transform by rotation
			matWorld *= matTrans; // Transform by translation

			// Create "Point At" Matrix for camera
			Vector3D vUp = new Vector3D(0, 1, 0);
			Vector3D vTarget = new Vector3D(0, 0, 1);
            Mat4x4 matCameraRot = Mat4x4.MakeRotationY(fYaw);
			vLookDir = matCameraRot * vTarget;
			vTarget = vCamera + vLookDir;
            Mat4x4 matCamera = Mat4x4.PointAt(vCamera, vTarget, vUp);

            // Make view matrix from camera
            Mat4x4 matView = matCamera.QuickInverse();


            float r = 25;
            globe.Clear();
            meshCube.Tris.Clear();
            for (int i = 0; i < total + 1; i++)
            {
                Dictionary<int, Vector3D> lanDic = new Dictionary<int, Vector3D>();
                globe.Add(i, lanDic);
                float lat = MathHelper.Map(i, 0, total, -HALF_PI , HALF_PI);
                float r2 = superShape(lat, superShapeSet.SuperShape2);
                //float r2 = superShape(lat, 0, 10, 10, 10);
                for (int j = 0; j < total + 1; j++)
                {
                    float lon = MathHelper.Map(j, 0, total, -PI, PI);
                    float r1 = superShape(lon, superShapeSet.SuperShape1);
                    //float r1 = superShape(lon, 0, 60, 100, 30);
                    float x = (float)(r * r1 * Math.Cos(lon) * r2 * Math.Cos(lat));
                    float y = (float)(r * r1 * Math.Sin(lon) * r2 * Math.Cos(lat));
                    float z = (float)(r * r2 * Math.Sin(lat));
                    lanDic.Add(j, new Vector3D(x, y, z));
                }
            }

            for (int i = 1; i < total + 1; i++)
            {
                Rgba32 color = Colors[Convert.ToInt32(MathHelper.Map(i, 1, total + 1, 0, 255))];

                for (int j = 1; j < total + 1; j++)
                {
                    meshCube.Tris.Add(new Triangle(new List<Vector3D> { globe[i - 1][j - 1], globe[i - 1][j],  globe[i][j - 1] })
                    { color = color });
                    meshCube.Tris.Add(new Triangle(new List<Vector3D> { globe[i][j - 1], globe[i - 1][j], globe[i][j] })
                    { color = color });
                }
            }

            drawTriangles(meshCube, matWorld, matView, matProj, vCamera, light_direction, false, false);

            return image;
        }

        private float superShape(float theta, SuperShapeObject superShape)
        {
            double t1 = Math.Abs((1 / a) * Math.Cos(superShape.M * theta / 4));
            t1 = Math.Pow(t1, superShape.N2);
            double t2 = Math.Abs((1 / b) * Math.Sin(superShape.M * theta / 4));
            t2 = Math.Pow(t2, superShape.N3);
            double t3 = t1 + t2;
            double r = Math.Pow(t3, -1 / superShape.N1);
            return (float)r;
        }
    }
}
