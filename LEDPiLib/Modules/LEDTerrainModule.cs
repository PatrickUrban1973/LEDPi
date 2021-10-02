using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using LEDPiLib.DataItems;
using LEDPiLib.Modules.Helper;
using LEDPiLib.Modules.Model;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
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

        private int scaleGrid = 20;
        private float fAspectRatio = 1.75f;
        
        private float offsetY = 0;
        private float perlinZ;
        private Perlin perlin;

        public LEDTerrainModule(ModuleConfiguration moduleConfiguration) : base(moduleConfiguration, 2.5f, 30)
        {
            if (!string.IsNullOrEmpty(moduleConfiguration.Parameter))
                fAspectRatio = float.Parse(moduleConfiguration.Parameter, CultureInfo.InvariantCulture.NumberFormat);
                
            perlin = new Perlin(-1);
            matProj = Mat4x4.MakeProjection(45.0f, fAspectRatio, 0.1f, 1000.0f);

            perlinZ = 1f + Convert.ToSingle(new Random().NextDouble());
            
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

        private Color getColor(List<Vector3D> vectors)
        {
            float z = vectors.Max(c => c.vector.Z);
            Color color = Color.White;

            if (z > 0.7f)
                color = Color.DarkGray;
            else if (z > 0.5f)
                color = Color.Gray;

            //Debug.Print(z.ToString());

            return color;
        }

        private void createTriangle(List<Triangle> triangles, int x, int y)
        {
            List<Vector3D> vectors = new List<Vector3D>();
            vectors.Add(new Vector3D(MathHelper.Map(x, 0, scaleGrid - 1, 0, 1), MathHelper.Map(y + 1, 0, scaleGrid - 1, 0, 1), perlin.perlin(MathHelper.Map(x, 0, scaleGrid - 1, -.25f, 1), y + 1 + offsetY, perlinZ)));
            vectors.Add(new Vector3D(MathHelper.Map(x + 1, 0, scaleGrid - 1, 0, 1), MathHelper.Map(y, 0, scaleGrid - 1, 0, 1), perlin.perlin(MathHelper.Map(x + 1, 0, scaleGrid - 1, -.25f, 1), y + offsetY, perlinZ)));  //3
            vectors.Add(new Vector3D(MathHelper.Map(x, 0, scaleGrid - 1, 0, 1), MathHelper.Map(y, 0, scaleGrid - 1, 0, 1), perlin.perlin(MathHelper.Map(x, 0, scaleGrid - 1, -.25f, 1), y + offsetY, perlinZ)));         //2
            triangles.Add(new Triangle(vectors) { color = getColor(vectors) });

            vectors = new List<Vector3D>();
            vectors.Add(new Vector3D(MathHelper.Map(x, 0, scaleGrid - 1, 0, 1), MathHelper.Map(y + 1, 0, scaleGrid - 1, 0, 1), perlin.perlin(MathHelper.Map(x, 0, scaleGrid - 1, -.25f, 1), y + 1 + offsetY, perlinZ)));
            vectors.Add(new Vector3D(MathHelper.Map(x + 1, 0, scaleGrid - 1, 0, 1), MathHelper.Map(y + 1, 0, scaleGrid - 1, 0, 1), perlin.perlin(MathHelper.Map(x + 1, 0, scaleGrid - 1, -.25f, 1), y + 1 + offsetY, perlinZ)));  //3
            vectors.Add(new Vector3D(MathHelper.Map(x + 1, 0, scaleGrid - 1, 0, 1), MathHelper.Map(y, 0, scaleGrid - 1, 0, 1), perlin.perlin(MathHelper.Map(x + 1, 0, scaleGrid - 1, -.25f, 1), y + offsetY, perlinZ)));         //2
            triangles.Add(new Triangle(vectors) { color = getColor(vectors) });
        }

        protected override bool completedRun()
        {
            return false;
        }

        private void updateTriangles()
        {
            List<Triangle> triangles = new List<Triangle>();

            for (int y = scaleGrid - 1; y >= 0 ; y--)
            {
                for (int x = scaleGrid - 1; x >= 0 ; x--)
                {
                   createTriangle(triangles, x, y);
                }
            }

            meshCube.Tris = triangles;
            offsetY += 1f;
        }

        protected override Image<Rgba32> RunInternal()
        {
            image = new Image<Rgba32>(renderWidth, renderHeight);
            engine3D.Canvas = image;

            updateTriangles();

            // Store triagles for rastering later
            drawTriangles(meshCube, matWorld, matView, matProj, vCamera, light_direction, true, false);

            return image;
        }
    }
}
