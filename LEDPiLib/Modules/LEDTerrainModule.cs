using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using LEDPiLib.DataItems;
using LEDPiLib.Modules.Helper;
using LEDPiLib.Modules.Model;
using LEDPiLib.Modules.Model.Common;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using static LEDPiLib.LEDPIProcessorBase;

namespace LEDPiLib.Modules
{
    [LEDModule(LEDModules.Terrain)]
    public class LEDTerrainModule : LEDEngine3DModuleBase
    {
        private Mesh meshCube;
        private readonly Mat4x4 matProj; // Matrix that converts from view space to screen space
        private readonly Vector3D vCamera = new Vector3D(0.5f, .01f, 4f);  // Location of camera in world space
        private readonly Vector3D light_direction = new Vector3D(-.75f, .5f, -0.2f);
        float fYaw = 0f;     // FPS Camera rotation in XZ plane
        private readonly Mat4x4 matWorld;
        private readonly Mat4x4 matView;

        private const int scaleGrid = 20;
        private readonly float fAspectRatio = 2.25f;
        
        private float offsetY;
        private readonly float perlinZ;
        private readonly Perlin perlin;

        public LEDTerrainModule(ModuleConfiguration moduleConfiguration) : base(moduleConfiguration, 2.5f)
        {
            if (!string.IsNullOrEmpty(moduleConfiguration.Parameter))
                fAspectRatio = float.Parse(moduleConfiguration.Parameter, CultureInfo.InvariantCulture.NumberFormat);
                
            perlin = new Perlin();
            matProj = Mat4x4.MakeProjection(45.0f, fAspectRatio, 0.1f, 1000.0f);

            perlinZ = MathHelper.Map(MathHelper.GlobalRandom().Next(0, 100), 0, 99, 1.0f, 1.75f);
            
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
            var vLookDir = matCameraRot * vTarget;
            vTarget = vCamera + vLookDir;
            Mat4x4 matCamera = Mat4x4.PointAt(vCamera, vTarget, vUp);

            // Make view matrix from camera
           matView = matCamera.QuickInverse();
        }

        private Color getColor(List<Vector3D> vectors)
        {
            float z = vectors.Max(c => c.vector.Z);
            Color color;

            if (z > 0.7f)
                color = Color.DarkGray;
            else if (z > 0.5f)
                color = Color.Gray;
            else
                color = Color.White;

            return color;
        }

        private void createTriangle(List<Triangle> triangles, int y, float x0, float x1, float y0, float y1, float x0Perlin, float x1Perlin)
        {
            List<Vector3D> vectors = new List<Vector3D>();
            vectors.Add(new Vector3D(x0, y1, perlin.perlin(x0Perlin, y + 1 + offsetY, perlinZ)));
            vectors.Add(new Vector3D(x1, y0, perlin.perlin(x1Perlin, y + offsetY, perlinZ)));  //3
            vectors.Add(new Vector3D(x0, y0, perlin.perlin(x0Perlin, y + offsetY, perlinZ)));         //2
            triangles.Add(new Triangle(vectors) { color = getColor(vectors) });

            vectors = new List<Vector3D>();
            vectors.Add(new Vector3D(x0, y1, perlin.perlin(x0Perlin, y + 1 + offsetY, perlinZ)));
            vectors.Add(new Vector3D(x1, y1, perlin.perlin(x1Perlin, y + 1 + offsetY, perlinZ)));  //3
            vectors.Add(new Vector3D(x1, y0, perlin.perlin(x1Perlin, y + offsetY, perlinZ)));         //2
            triangles.Add(new Triangle(vectors) { color = getColor(vectors) });
        }

        protected override bool completedRun()
        {
            return false;
        }

        private void updateTriangles()
        {
            List<Triangle> triangles = new List<Triangle>();

            for (int y = (scaleGrid * 2) - 1; y >= 0 ; y--)
            {
                float y0 = MathHelper.Map(y, 0, scaleGrid - 1, 0, 1);
                float y1 = MathHelper.Map(y + 1, 0, scaleGrid - 1, 0, 1);

                for (int x = scaleGrid - 1; x >= 0 ; x--)
                {
                    float x0 = MathHelper.Map(x, 0, scaleGrid - 1, 0, 1);
                    float x1 = MathHelper.Map(x + 1, 0, scaleGrid - 1, 0, 1);
                    float x0Perlin = MathHelper.Map(x, 0, scaleGrid - 1, -.25f, 1.5f);
                    float x1Perlin = MathHelper.Map(x + 1, 0, scaleGrid - 1, -.25f, 1.5f);

                    createTriangle(triangles, y, x0, x1, y0, y1, x0Perlin, x1Perlin);
                }
            }

            meshCube.Tris = triangles;
            offsetY += 1f;
        }

        protected override Image<Rgba32> RunInternal()
        {
            image = GetNewImage();
            engine3D.Canvas = image;

            updateTriangles();

            // Store triagles for rastering later
            drawTriangles(meshCube, matWorld, matView, matProj, vCamera, light_direction, true, false);

            return image;
        }
    }
}
