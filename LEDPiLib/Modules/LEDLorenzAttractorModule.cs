using System;
using System.Collections.Generic;
using System.Diagnostics;
using LEDPiLib.DataItems;
using LEDPiLib.Modules.Helper;
using LEDPiLib.Modules.Model;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using static LEDPiLib.LEDPIProcessorBase;

namespace LEDPiLib.Modules
{
    [LEDModule(LEDModules.LorenzAttractor)]
    public class LEDLorenzAttractorModule : LEDEngine3DModuleBase
    {
        private Mat4x4 matProj; // Matrix that converts from view space to screen space
        private Vector3D vCamera = new Vector3D(0f, -1f, 4f);  // Location of camera in world space
        private Vector3D vLookDir = new Vector3D(0f,0f,0f); // Direction vector along the direction camera points
        private Vector3D light_direction = new Vector3D(0.0f, 1.0f, -1.0f);
        private float fYaw = 0f;     // FPS Camera rotation in XZ plane
        private float fTheta;   // Spins World transform
        private float fElapsedTime = 0f;

        private float x = 0.01f;
        private float y = 1;
        private float z = 1;

        private float a = 10;
        private float b = 28;
        private float c = 8.0f / 3.0f;

        int colorIndex = 0;

        Stopwatch stopwatch = new Stopwatch();

        Mesh meshCube = new Mesh();
        Vector3D pointBefore;
        bool first = true;

        public LEDLorenzAttractorModule(ModuleConfiguration moduleConfiguration) : base(moduleConfiguration, 3f)
        {
            Random random = new Random();

            x = Convert.ToSingle( random.NextDouble());
            y = Convert.ToSingle( random.NextDouble());
            z = Convert.ToSingle( random.NextDouble());
            
            matProj = Mat4x4.MakeProjection(90.0f, 1f, 0.1f, 1000.0f);
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
            image = new Image<Rgba32>(renderWidth, renderHeight);
            engine3D.Canvas = image;

            // Set up "World Tranmsform" though not updating theta 
            // makes this a bit redundant
            Mat4x4 matRotZ, matRotX;
			fTheta += 1.0f * fElapsedTime; // Uncomment to spin me right round baby right round
			matRotZ = Mat4x4.MakeRotationZ(fTheta);
			matRotX = Mat4x4.MakeRotationX(45f);

            Mat4x4 matTrans = Mat4x4.MakeTranslation(0.0f, 0.0f, 5.0f);

            Mat4x4 matWorld = Mat4x4.MakeIdentity();   // Form World Matrix
            matWorld = matRotZ * matRotX; // Transform by rotation
			matWorld = matWorld * matTrans; // Transform by translation

			// Create "Point At" Matrix for camera
			Vector3D vUp = new Vector3D(0, 1, 0);
			Vector3D vTarget = new Vector3D(0, 0, 1);
            Mat4x4 matCameraRot = Mat4x4.MakeRotationY(fYaw);
			vLookDir = matCameraRot * vTarget;
			vTarget = vCamera + vLookDir;
            Mat4x4 matCamera = Mat4x4.PointAt(vCamera, vTarget, vUp);

            // Make view matrix from camera
            Mat4x4 matView = matCamera.QuickInverse();


            float dt = 0.01f;
            float dx = (a * (y - x)) * dt;
            float dy = (x * (b - z) - y) * dt;
            float dz = (x * y - c * z) * dt;
            x = x + dx;
            y = y + dy;
            z = z + dz;

            if (first)
            {
                pointBefore = new Vector3D(MathHelper.Map(x, -20, 20, 0f, 1f), MathHelper.Map(y, -20, 20, 0, 1f), MathHelper.Map(z, -20, 20, 0, 1f));
                first = false;
            }
            else
            {
                Vector3D newPoint = new Vector3D(MathHelper.Map(x, -20, 20, 0f, 1f), MathHelper.Map(y, -20, 20, 0, 1f), MathHelper.Map(z, -20, 20, 0, 1f));
                Vector3D newPoint2 = newPoint + new Vector3D(0.01f, 0f, 0f);
                Vector3D pointBefore2 = pointBefore + new Vector3D(0.01f, 0f, 0f);

                if (colorIndex >= 255)
                    colorIndex = 0;

                Rgba32 color = Colors[colorIndex++];

                meshCube.Tris.Add(new Triangle(new List<Vector3D> {pointBefore, pointBefore2, newPoint})
                    {color = color });
                meshCube.Tris.Add(new Triangle(new List<Vector3D> { pointBefore2, newPoint, newPoint2 })
                { color = color });
                pointBefore = newPoint;
            }

            drawTriangles(meshCube, matWorld, matView, matProj, vCamera, light_direction, true, false);

            return image;
        }
    }
}
