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
    [LEDModule(LEDModules.Vertex)]
    public class LEDVertexModule : LEDEngine3DModuleBase
    {
        private readonly Mat4x4 matProj; // Matrix that converts from view space to screen space
        private  readonly Vector3D vCamera = new Vector3D(0f, -1f, 4f);  // Location of camera in world space
        private Vector3D vLookDir = new Vector3D(0f,0f,0f); // Direction vector along the direction camera points
        private  readonly Vector3D light_direction = new Vector3D(0.0f, 1.0f, -1.0f);
        private float fYaw = 0f;     // FPS Camera rotation in XZ plane
        private float fThetaZ;
        private float fThetaX;
        private float fThetaY;
        private float fElapsedTime;

        private float x;
        private float y;
        private float z;

        private const float a = 10;
        private const float b = 28;
        private const float c = 8.0f / 3.0f;

        private int colorIndex;

        private readonly Stopwatch stopwatch = new Stopwatch();

        private readonly Mesh meshCube;
        private readonly Func<List<Triangle>> functions;
        private readonly float viewDistance;

        public LEDVertexModule(ModuleConfiguration moduleConfiguration) : base(moduleConfiguration, 3f)
        {
            int randomModuleNumber = MathHelper.GlobalRandom().Next(0, 2);

            if (randomModuleNumber == 0)
            {
                functions = calcLorenzAttractor;
                viewDistance = 7f;
            }
            else
            {
                functions = calc3DKnot;
                viewDistance = 13f;
            }
            
            x = (float)MathHelper.GlobalRandom().NextDouble();
            y = (float)MathHelper.GlobalRandom().NextDouble();
            z = (float)MathHelper.GlobalRandom().NextDouble();
            
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
            image = GetNewImage();
            engine3D.Canvas = image;

            fThetaZ += (float) MathHelper.GlobalRandom().NextDouble() * fElapsedTime; // Uncomment to spin me right round baby right round
            fThetaY += (float) MathHelper.GlobalRandom().NextDouble() * fElapsedTime; // Uncomment to spin me right round baby right round
            fThetaX += (float) MathHelper.GlobalRandom().NextDouble() * fElapsedTime; // Uncomment to spin me right round baby right round

            // Set up "World Tranmsform" though not updating theta 
            // makes this a bit redundant
            Mat4x4 matRotZ = Mat4x4.MakeRotationZ(fThetaZ);
            Mat4x4 matRotY = Mat4x4.MakeRotationX(fThetaY);
            Mat4x4 matRotX = Mat4x4.MakeRotationX(fThetaX);

            Mat4x4 matTrans = Mat4x4.MakeTranslation(0.0f, 0.0f, viewDistance);
            Mat4x4 matWorld = matRotZ * matRotX * matRotY; // Transform by rotation
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


            meshCube.Tris.AddRange(functions.Invoke());
            
            drawTriangles(meshCube, matWorld, matView, matProj, vCamera, light_direction, true, false);

            return image;
        }


        private List<Triangle> calcLorenzAttractor()
        {
            const float dt = 0.01f;
            float dx = (a * (y - x)) * dt;
            float dy = (x * (b - z) - y) * dt;
            float dz = (x * y - c * z) * dt;
            x = x + dx;
            y = y + dy;
            z = z + dz;

            Vector3D newPoint = new Vector3D(MathHelper.Map(x, -20, 20, 0f, 1f), MathHelper.Map(y, -20, 20, 0, 1f), MathHelper.Map(z, -20, 20, 0, 1f));

            if (colorIndex >= 255)
                colorIndex = 0;

            Rgba32 color = Colors[colorIndex++];
            return addVertex(newPoint, color);
        }

        private float beta;
        private List<Triangle> calc3DKnot()
        {

            float r = (float) (100 * (0.8f + 1.6f * Math.Sin(6 * beta)));
            float theta = 2 * beta;
            float phi = (float) (0.6 * Math.PI * Math.Sin(12 * beta));
            x = (float) (r * Math.Cos(phi) * Math.Cos(theta));
            y = (float) (r * Math.Cos(phi) * Math.Sin(theta));
            z = (float) (r * Math.Sin(phi));
            Color color = new Rgba32(255, r, 255);
            Vector3D newPoint = new Vector3D(MathHelper.Map(x, -20, 20, 0f, 1f), MathHelper.Map(y, -20, 20, 0, 1f), MathHelper.Map(z, -20, 20, 0, 1f));
            
            beta += 0.01f;
            return addVertex(newPoint, color);
        }
    }
}
