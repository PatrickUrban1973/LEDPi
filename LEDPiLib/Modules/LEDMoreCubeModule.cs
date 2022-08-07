using System;
using System.Collections.Generic;
using System.Numerics;
using LEDPiLib.DataItems;
using LEDPiLib.Modules.Helper;
using LEDPiLib.Modules.Model;
using LEDPiLib.Modules.Model.Common;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using static LEDPiLib.LEDPIProcessorBase;

namespace LEDPiLib.Modules
{
    [LEDModule(LEDModules.MoreCube)]
    public class LEDMoreCubeModule : LEDEngine3DModuleBase
    {
        private Mesh meshCube;
        private readonly Mat4x4 matProj; // Matrix that converts from view space to screen space
        private Vector3D vCamera = new Vector3D(0f, 0f, 2.5f);  // Location of camera in world space
        private Vector3D vLookDir = new Vector3D(0f,0f,0f); // Direction vector along the direction camera points
        private readonly Vector3D light_direction = new Vector3D(0.0f, 1.5f, -1.0f);

        private const int scaleGrid = 20;

        private const float quarter_pi = 0.7853982f;
        private readonly float ma;
        private readonly float maxD;
        private float angle;
        private readonly bool withLight;

        public LEDMoreCubeModule(ModuleConfiguration moduleConfiguration) : base(moduleConfiguration, 2f)
        {
            ma = (float)Math.Atan(Math.Cos(quarter_pi));
            matProj = Mat4x4.MakeProjection(90.0f, 1f, 0.1f, 1000.0f);
            maxD = Vector2.Distance(new Vector2(0, 0), new Vector2(scaleGrid, scaleGrid));
            withLight = MathHelper.GlobalRandom().Next() % 2 == 0;
        }

        protected override bool completedRun()
        {
            return false;
        }

        protected override Image<Rgba32> RunInternal()
        {
            image = GetNewImage();
            engine3D.Canvas = image;
                        
            List<Triangle> triangles = new List<Triangle>();

            for (int x = scaleGrid - 1; x >= 0; x--)
            {
                float x0 = MathHelper.Map(x, 0, scaleGrid, 0, 1);
                float x1 = MathHelper.Map(x + 1, 0, scaleGrid, 0, 1);
 
                for (int y = scaleGrid - 1; y >= 0; y--)
                {
                    float d = Vector2.Distance(new Vector2(x, y), new Vector2(scaleGrid / 2, scaleGrid / 2));
                    
                    float offset = MathHelper.Map(d, 0, maxD, (float)-Math.PI, (float)Math.PI);
                    float a = angle + offset;
                    float z = MathHelper.Map((float)Math.Sin(a), -1, 1, 0, 1) * -1f;

                    float y0 = MathHelper.Map(y, 0, scaleGrid, 0, 1);
                    float y1 = MathHelper.Map(y + 1, 0, scaleGrid, 0, 1);

                    // SOUTH
                    triangles.Add(
                        new Triangle(new List<Vector3D>()
                        {
                            new Vector3D(x0, y0, z, 1.0f), 
                            new Vector3D(x0, y1, z, 1.0f),
                            new Vector3D(x1, y1, z, 1.0f)
                        }){color = Color.Aqua});
                    triangles.Add(
                        new Triangle(new List<Vector3D>()
                        {
                            new Vector3D(x0, y0, z, 1.0f), 
                            new Vector3D(x1, y1, z, 1.0f),
                            new Vector3D(x1, y0, z, 1.0f)
                        }){color = Color.Aqua});

                    // EAST           																			   
                    triangles.Add(
                        new Triangle(new List<Vector3D>()
                        {
                            new Vector3D(x1, y0, z, 1.0f), 
                            new Vector3D(x1, y1, z, 1.0f),
                            new Vector3D(x1, y1, 1.0f - z, 1.0f)
                        }){color = Color.Yellow});
                    triangles.Add(
                        new Triangle(new List<Vector3D>()
                        {
                            new Vector3D(x1, y0, z, 1.0f), 
                            new Vector3D(x1, y1, 1.0f - z, 1.0f),
                            new Vector3D(x1, y0, 1.0f - z, 1.0f)
                        }){color = Color.Yellow});

                    // TOP             																			   
                    triangles.Add(
                        new Triangle(new List<Vector3D>()
                        {
                            new Vector3D(x0, y1, z, 1.0f),
                            new Vector3D(x0, y1, 1.0f - z, 1.0f),
                            new Vector3D(x1, y1, 1.0f - z, 1.0f)
                        }){color = Color.Purple});
                    triangles.Add(
                        new Triangle(new List<Vector3D>()
                        {
                            new Vector3D(x0, y1, z, 1.0f),
                            new Vector3D(x1, y1, 1.0f - z, 1.0f),
                            new Vector3D(x1, y1, z, 1.0f)
                        }){color = Color.Purple});
                }
            }

            meshCube.Tris = triangles;



            // Set up "World Transform" though not updating theta 
            // makes this a bit redundant
            Mat4x4 matRotZ = Mat4x4.MakeRotationZ(0);
            Mat4x4 matRotX = Mat4x4.MakeRotationX(-quarter_pi);
            Mat4x4 matRotY = Mat4x4.MakeRotationY(ma);


            Vector3D vForward = vLookDir * 0.001f;

            vCamera += vForward;

            Mat4x4 matTrans = Mat4x4.MakeTranslation(0.0f, 0.0f, 4.5f);

            Mat4x4 matWorld = matRotZ * matRotX * matRotY; // Transform by rotation
			matWorld *= matTrans; // Transform by translation

			// Create "Point At" Matrix for camera
			Vector3D vUp = new Vector3D(0, -1.5f, 0);
			Vector3D vTarget = new Vector3D(0, 0, 1);
            Mat4x4 matCameraRot = Mat4x4.MakeRotationY(0);
			vLookDir = matCameraRot * vTarget;
			vTarget = vCamera + vLookDir;
            Mat4x4 matCamera = Mat4x4.PointAt(vCamera, vTarget, vUp);

            // Make view matrix from camera
            Mat4x4 matView = matCamera.QuickInverse();

            // Store triagles for rastering later
            drawTriangles(meshCube, matWorld, matView, matProj, vCamera, light_direction, false, withLight);

            angle -= 0.1f;            
            return image;
        }
    }
}
