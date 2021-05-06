using System.Collections.Generic;
using System.Diagnostics;
using LEDPiLib.DataItems;
using LEDPiLib.Modules.Helper;
using LEDPiLib.Modules.Model;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using static LEDPiLib.LEDPIProcessorBase;

namespace LEDPiLib.Modules
{
    [LEDModule(LEDModules.Cube)]
    public class LEDCubeModule : LEDEngine3DModuleBase
    {
        Mesh meshCube = new Mesh();
        Mat4x4 matProj; // Matrix that converts from view space to screen space
        Vector3D vCamera = new Vector3D(0f, 0f, 2.5f);  // Location of camera in world space
        Vector3D vLookDir = new Vector3D(0f,0f,0f); // Direction vector along the direction camera points
        Vector3D light_direction = new Vector3D(0.0f, 1.0f, -1.0f);
        float fYaw = 0f;     // FPS Camera rotation in XZ plane
        float fTheta;   // Spins World transform
        private float fElapsedTime = 0f;

        Stopwatch stopwatch = new Stopwatch();

        public LEDCubeModule(ModuleConfiguration moduleConfiguration) : base(moduleConfiguration, 3f)
        {
            meshCube.Tris = new List<Triangle>()
            {
                // SOUTH
                new Triangle(new List<Vector3D>() {new Vector3D(0.0f, 0.0f, 0.0f, 1.0f), new Vector3D(0.0f, 1.0f, 0.0f, 1.0f), new Vector3D(1.0f, 1.0f, 0.0f, 1.0f) }, new List<Vector2D>()
                {
                    new Vector2D(0.0f, 1.0f, 1.0f), new Vector2D(0.0f, 0.0f, 1.0f), new Vector2D(1.0f, 0.0f, 1.0f)
                }),
                new Triangle(new List<Vector3D>() {new Vector3D(0.0f, 0.0f, 0.0f, 1.0f), new Vector3D(1.0f, 1.0f, 0.0f, 1.0f), new Vector3D(1.0f, 0.0f, 0.0f, 1.0f) }, new List<Vector2D>()
                {
                    new Vector2D(0.0f, 1.0f, 1.0f), new Vector2D(1.0f, 0.0f, 1.0f), new Vector2D(1.0f, 1.0f, 1.0f)
                }),

                // EAST           																			   
                new Triangle(new List<Vector3D>() {new Vector3D(1.0f, 0.0f, 0.0f, 1.0f), new Vector3D(1.0f, 1.0f, 0.0f, 1.0f), new Vector3D(1.0f, 1.0f, 1.0f, 1.0f) }, new List<Vector2D>(){new Vector2D(0.0f, 1.0f, 1.0f), new Vector2D(0.0f, 0.0f, 1.0f), new Vector2D(1.0f, 0.0f, 1.0f) }),
                new Triangle(new List<Vector3D>() {new Vector3D(1.0f, 0.0f, 0.0f, 1.0f), new Vector3D(1.0f, 1.0f, 1.0f, 1.0f), new Vector3D(1.0f, 0.0f, 1.0f, 1.0f) }, new List<Vector2D>(){new Vector2D(0.0f, 1.0f, 1.0f), new Vector2D(1.0f, 0.0f, 1.0f), new Vector2D(1.0f, 1.0f, 1.0f) }),

                // NORTH           																			   
                new Triangle(new List<Vector3D>() {new Vector3D(1.0f, 0.0f, 1.0f, 1.0f), new Vector3D(1.0f, 1.0f, 1.0f, 1.0f), new Vector3D(0.0f, 1.0f, 1.0f, 1.0f) }, new List<Vector2D>(){new Vector2D(0.0f, 1.0f, 1.0f), new Vector2D(0.0f, 0.0f, 1.0f), new Vector2D(1.0f, 0.0f, 1.0f) }),
                new Triangle(new List<Vector3D>() {new Vector3D(1.0f, 0.0f, 1.0f, 1.0f), new Vector3D(0.0f, 1.0f, 1.0f, 1.0f), new Vector3D(0.0f, 0.0f, 1.0f, 1.0f) }, new List<Vector2D>(){new Vector2D(0.0f, 1.0f, 1.0f), new Vector2D(1.0f, 0.0f, 1.0f), new Vector2D(1.0f, 1.0f, 1.0f) }),

                // WEST            																			   
                new Triangle(new List<Vector3D>() {new Vector3D(0.0f, 0.0f, 1.0f, 1.0f), new Vector3D(0.0f, 1.0f, 1.0f, 1.0f), new Vector3D(0.0f, 1.0f, 0.0f, 1.0f) }, new List<Vector2D>(){new Vector2D(0.0f, 1.0f, 1.0f), new Vector2D(0.0f, 0.0f, 1.0f), new Vector2D(1.0f, 0.0f, 1.0f) }),
                new Triangle(new List<Vector3D>() {new Vector3D(0.0f, 0.0f, 1.0f, 1.0f), new Vector3D(0.0f, 1.0f, 0.0f, 1.0f), new Vector3D(0.0f, 0.0f, 0.0f, 1.0f) }, new List<Vector2D>(){new Vector2D(0.0f, 1.0f, 1.0f), new Vector2D(1.0f, 0.0f, 1.0f), new Vector2D(1.0f, 1.0f, 1.0f) }),

                // TOP             																			   
                new Triangle(new List<Vector3D>() {new Vector3D(0.0f, 1.0f, 0.0f, 1.0f), new Vector3D(0.0f, 1.0f, 1.0f, 1.0f), new Vector3D(1.0f, 1.0f, 1.0f, 1.0f) }, new List<Vector2D>(){new Vector2D(0.0f, 1.0f, 1.0f), new Vector2D(0.0f, 0.0f, 1.0f), new Vector2D(1.0f, 0.0f, 1.0f) }),
                new Triangle(new List<Vector3D>() {new Vector3D(0.0f, 1.0f, 0.0f, 1.0f), new Vector3D(1.0f, 1.0f, 1.0f, 1.0f), new Vector3D(1.0f, 1.0f, 0.0f, 1.0f) }, new List<Vector2D>(){new Vector2D(0.0f, 1.0f, 1.0f), new Vector2D(1.0f, 0.0f, 1.0f), new Vector2D(1.0f, 1.0f, 1.0f) }),

                // BOTTOM          																			  
                new Triangle(new List<Vector3D>() {new Vector3D(1.0f, 0.0f, 1.0f, 1.0f), new Vector3D(0.0f, 0.0f, 1.0f, 1.0f), new Vector3D(0.0f, 0.0f, 0.0f, 1.0f) }, new List<Vector2D>(){new Vector2D(0.0f, 1.0f, 1.0f), new Vector2D(0.0f, 0.0f, 1.0f), new Vector2D(1.0f, 0.0f, 1.0f) }),
                new Triangle(new List<Vector3D>() {new Vector3D(1.0f, 0.0f, 1.0f, 1.0f), new Vector3D(0.0f, 0.0f, 0.0f, 1.0f), new Vector3D(1.0f, 0.0f, 0.0f, 1.0f) }, new List<Vector2D>(){new Vector2D(0.0f, 1.0f, 1.0f), new Vector2D(1.0f, 0.0f, 1.0f), new Vector2D(1.0f, 1.0f, 1.0f) }),
            };

            matProj = Mat4x4.MakeProjection(90.0f, (float)renderHeight / (float)renderWidth, 0.1f, 1000.0f);

        }

        protected override bool completedRun()
        {
            return false;
        }

        protected override Image<Rgba32> Run()
        {
            fElapsedTime = stopwatch.ElapsedMilliseconds / 1000f;
            stopwatch.Restart();
            image = new Image<Rgba32>(renderWidth, renderHeight);
            engine3D.Canvas = image;

            // Set up "World Tranmsform" though not updating theta 
            // makes this a bit redundant
            Mat4x4 matRotZ, matRotX;
			fTheta += 1.0f * fElapsedTime; // Uncomment to spin me right round baby right round
			matRotZ = Mat4x4.MakeRotationZ(fTheta * 0.5f);
			matRotX = Mat4x4.MakeRotationX(fTheta);

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

            // Store triagles for rastering later
            // Store triagles for rastering later
            drawTriangles(meshCube, matWorld, matView, matProj, vCamera, light_direction, false);

            image.Mutate(c => c.Resize(LEDWidth, LEDHeight));
            return image;
        }
    }
}
