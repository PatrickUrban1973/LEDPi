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
    [LEDModule(LEDModules.Terrain)]
    public class LEDTerrainModule : LEDEngine3DModuleBase
    {
        Mesh meshCube = new Mesh();
        Mat4x4 matProj; // Matrix that converts from view space to screen space
        Vector3D vCamera = new Vector3D(0.5f, .01f, 4f);  // Location of camera in world space
        Vector3D vLookDir = new Vector3D(0f,0f,0f); // Direction vector along the direction camera points
        Vector3D light_direction = new Vector3D(-.75f, .5f, -0.2f);
        float fYaw = 0f;     // FPS Camera rotation in XZ plane
        Mat4x4 matWorld;
        Mat4x4 matView;

        private int scaleGrid = 9;

        private float offsetY = 0;
        private Perlin perlin;

        public LEDTerrainModule(ModuleConfiguration moduleConfiguration) : base(moduleConfiguration, 2.5f)
        {
            perlin = new Perlin(-1);
            matProj = Mat4x4.MakeProjection(45.0f, 1.5f, 0.1f, 1000.0f);

            // Set up "World Tranmsform" though not updating theta 
            // makes this a bit redundant
            Mat4x4 matRotZ, matRotX;
            matRotZ = Mat4x4.MakeRotationZ(0f);
            matRotX = Mat4x4.MakeRotationX(1.1f);

            Mat4x4 matTrans = Mat4x4.MakeTranslation(0.0f, 0.0f, 5.0f);

            matWorld = Mat4x4.MakeIdentity();   // Form World Matrix
            matWorld = matRotZ * matRotX; // Transform by rotation
            matWorld = matWorld * matTrans; // Transform by translation

            // Create "Point At" Matrix for camera
            Vector3D vUp = new Vector3D(0, 2, 0);
            Vector3D vTarget = new Vector3D(0, 0, 1);
            Mat4x4 matCameraRot = Mat4x4.MakeRotationY(fYaw);
            vLookDir = matCameraRot * vTarget;
            vTarget = vCamera + vLookDir;
            Mat4x4 matCamera = Mat4x4.PointAt(vCamera, vTarget, vUp);

            // Make view matrix from camera
           matView = matCamera.QuickInverse();
        }

        protected override bool completedRun()
        {
            return false;
        }

        private void updateTriangles()
        {
            List<Triangle> triangles = new List<Triangle>();

            for (int y = 0; y < scaleGrid - 1; y++)
            {
                for (int x = 0; x < scaleGrid - 1; x++)
                {
                    triangles.Add(new Triangle(new List<Vector3D>()
                    {
                        new Vector3D(LEDEngine3D.Map(x, 0, scaleGrid -1, 0, 1), LEDEngine3D.Map(y + 1, 0, scaleGrid -1, 0, 1), perlin.perlin(LEDEngine3D.Map(x, 0, scaleGrid -1, 0, 1), y + 1 + offsetY)),  //1
                        new Vector3D(LEDEngine3D.Map(x + 1, 0, scaleGrid -1, 0, 1), LEDEngine3D.Map(y, 0, scaleGrid -1, 0, 1), perlin.perlin(LEDEngine3D.Map(x + 1, 0, scaleGrid -1, 0, 1), y + offsetY)),  //3
                        new Vector3D(LEDEngine3D.Map(x, 0, scaleGrid -1, 0, 1), LEDEngine3D.Map(y, 0, scaleGrid -1, 0, 1), perlin.perlin(LEDEngine3D.Map(x, 0, scaleGrid -1, 0, 1),  y + offsetY)),         //2
                    },
                        new List<Vector2D>()
                        {
                            new Vector2D(0.0f, LEDEngine3D.Map(y, 0, scaleGrid -1, 0, 1), 1.0f), new Vector2D(0.0f, 0.0f, 1.0f), new Vector2D(LEDEngine3D.Map(x, 0, scaleGrid -1, 0, 1), 0.0f, 1.0f) ,
                        }
                    ));
                    triangles.Add(new Triangle(new List<Vector3D>()
                        {
                            new Vector3D(LEDEngine3D.Map(x, 0, scaleGrid -1, 0, 1), LEDEngine3D.Map(y + 1, 0, scaleGrid -1, 0, 1), perlin.perlin(LEDEngine3D.Map(x, 0, scaleGrid -1, 0, 1), y + 1 + offsetY)),
                            new Vector3D(LEDEngine3D.Map(x + 1, 0, scaleGrid -1, 0, 1), LEDEngine3D.Map(y + 1, 0, scaleGrid -1, 0, 1), perlin.perlin(LEDEngine3D.Map(x + 1, 0, scaleGrid -1, 0, 1), y + 1 + offsetY)),
                            new Vector3D(LEDEngine3D.Map(x + 1, 0, scaleGrid -1, 0, 1), LEDEngine3D.Map(y, 0, scaleGrid -1, 0, 1), perlin.perlin(LEDEngine3D.Map(x + 1, 0, scaleGrid -1, 0, 1), y + offsetY)),
                        },
                        new List<Vector2D>()
                        {
                            new Vector2D(0.0f, LEDEngine3D.Map(y, 0, scaleGrid -1, 0, 1), 1.0f), new Vector2D(LEDEngine3D.Map(x, 0, scaleGrid -1, 0, 1), 0.0f, 1.0f), new Vector2D(LEDEngine3D.Map(x, 0, scaleGrid -1, 0, 1), LEDEngine3D.Map(y, 0, scaleGrid -1, 0, 1), 1.0f)  ,
                        }
                    ));
                }
            }

            meshCube.Tris = triangles;
            offsetY += 1f;
        }

        protected override Image<Rgba32> Run()
        {
            image = new Image<Rgba32>(renderWidth, renderHeight);
            engine3D.Canvas = image;

            updateTriangles();

            // Store triagles for rastering later
            drawTriangles(meshCube, matWorld, matView, matProj, vCamera, light_direction, true);

            image.Mutate(c => c.Resize(LEDWidth, LEDHeight));
            return image;
        }
    }
}
