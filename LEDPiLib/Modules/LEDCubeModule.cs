using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using LEDPiLib.DataItems;
using LEDPiLib.Modules.Helper;
using LEDPiLib.Modules.Model;
using LEDPiLib.Modules.Model.Common;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using static LEDPiLib.LEDPIProcessorBase;

namespace LEDPiLib.Modules
{
    [LEDModule(LEDModules.Cube)]
    public class LEDCubeModule : LEDEngine3DModuleBase
    {
        [Flags]
        private enum Axis
        {
            X = 1,
            Y = 2,
            Z = 4,
        }

        private class Model3DConfiguration
        {
            public Model3DConfiguration(string fileName, Vector3D camera, Vector3D lookDir, Vector3D up, Vector3D target, Axis axis)
            {
                FileName = fileName;
                Camera = camera;
                LookDir = lookDir;
                Up = up;
                Target = target;
                Axis = axis;
            }

            public string FileName { get; private set; }
            public Vector3D Camera { get; private set; }
            public Vector3D LookDir { get; private set; }
            public Vector3D Up { get; private set; }
            public Vector3D Target { get; private set; }

            public Axis Axis { get; private set; }
        }

        private readonly List<Model3DConfiguration> configurations = new List<Model3DConfiguration>() { 
            new Model3DConfiguration("mountains.obj", new Vector3D(0f, 50f, -100.5f), new Vector3D(0f, -5f, 0f), new Vector3D(1, 1, 1), new Vector3D(0, -.5f, 1), Axis.Y), 
            new Model3DConfiguration("teapot.obj", new Vector3D(0f, 0f, 0.5f), new Vector3D(0f,0f,0f), new Vector3D(0, 1, 1), new Vector3D(0, .0f, 1), Axis.X|Axis.Y),
            new Model3DConfiguration("axis.obj", new Vector3D(0f, 0f, -7f), new Vector3D(0f,0f,0f), new Vector3D(0, 1, 0), new Vector3D(0, 0, 1), Axis.X|Axis.Y|Axis.Z),
            new Model3DConfiguration("VideoShip.obj", new Vector3D(0f, 0f, -3f), new Vector3D(0f,0f,0f), new Vector3D(0, 1, 1), new Vector3D(0, 0, 1), Axis.X|Axis.Y|Axis.Z),
            new Model3DConfiguration("cube.obj", new Vector3D(0f, 0f, 2.5f), new Vector3D(0f,0f,0f), new Vector3D(0, 1, 0), new Vector3D(0, 0, 1), Axis.X|Axis.Y|Axis.Z),
            new Model3DConfiguration("triangle.obj", new Vector3D(0f, 0f, 4.0f), new Vector3D(0f,0f,0f), new Vector3D(0, 1, 0), new Vector3D(0, 0, 1), Axis.X|Axis.Y|Axis.Z)
        };

        private readonly Mesh meshCube;
        private readonly Mat4x4 matProj; // Matrix that converts from view space to screen space
        private readonly Vector3D vCamera;
        private Vector3D vLookDir;
        private readonly Vector3D light_direction = new Vector3D(0.0f, 1.0f, -1.0f);
        float fYaw = 0f;     // FPS Camera rotation in XZ plane
        float fTheta;   // Spins World transform
        private float fElapsedTime;
        private readonly Model3DConfiguration model3DConfiguration;

        private readonly Stopwatch stopwatch = new Stopwatch();

        public LEDCubeModule(ModuleConfiguration moduleConfiguration) : base(moduleConfiguration, 3f)
        {
            int index;

            if (String.IsNullOrEmpty(moduleConfiguration.Parameter))
            {
                index = MathHelper.GlobalRandom().Next(0, configurations.Count);
            }
            else
            {
                index = Convert.ToInt32(moduleConfiguration.Parameter);
            }

            model3DConfiguration = configurations[index];

            vCamera = model3DConfiguration.Camera;
            vLookDir = model3DConfiguration.LookDir;

            meshCube.Tris = readObjFile(model3DConfiguration.FileName);
            matProj = Mat4x4.MakeProjection(90.0f, renderHeight / (float)renderWidth, 0.1f, 1000f);

        }

        private static List<Triangle> readObjFile(string fileName)
        {
            try
            {
                string path = Path.Combine(BasePath, "Modules", "Objects", "3DModel", fileName);
                CultureInfo cultures = new CultureInfo("en-US");
                List<Vector3D> vectors = new List<Vector3D>();
                vectors.Add(new Vector3D());
                List<Triangle> triangles = new List<Triangle>();

                const Int32 BufferSize = 128;
                using (var fileStream = File.OpenRead(path))
                using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, BufferSize))
                {
                    String line;
                    while ((line = streamReader.ReadLine()) != null)
                    {
                        string[] strings = line.Split(' ');


                        if (strings[0] == "v")
                        {
                            vectors.Add(new Vector3D(Convert.ToSingle(strings[1], cultures),
                                Convert.ToSingle(strings[2], cultures), Convert.ToSingle(strings[3], cultures)));
                        }

                        if (strings[0] == "f")
                        {
                            triangles.Add(new Triangle(new List<Vector3D>()
                            {
                                vectors[Convert.ToInt32(strings[1])], vectors[Convert.ToInt32(strings[2])],
                                vectors[Convert.ToInt32(strings[3])]
                            }));
                        }
                    }
                }

                return triangles;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
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
			fTheta += 1.0f * fElapsedTime; // Uncomment to spin me right round baby right round
            Mat4x4 matRotZ = Mat4x4.MakeRotationZ(fTheta * 0.5f);
            Mat4x4 matRotX = Mat4x4.MakeRotationX(fTheta);
            Mat4x4 matRotY = Mat4x4.MakeRotationY(fTheta);

            Mat4x4 matTrans = Mat4x4.MakeTranslation(0.0f, 0.0f, 5.0f);

            Mat4x4 matWorld = Mat4x4.MakeIdentity();   // Form World Matrix

            List<Mat4x4> matRots = new List<Mat4x4>();

            if ((model3DConfiguration.Axis & Axis.Y) > 0)
                matRots.Add(matRotY);
            if ((model3DConfiguration.Axis & Axis.X) > 0)
                matRots.Add(matRotX);
            if ((model3DConfiguration.Axis & Axis.Z) > 0)
                matRots.Add(matRotZ);

            bool first = true;
            matRots.ForEach(c =>
            {
                if (first)
                {
                    matWorld = c;
                    first = false;
                }
                else
                {
                    matWorld *= c;
                }
            });

            matWorld = matWorld * matTrans; // Transform by translation

            // Create "Point At" Matrix for camera
            Vector3D vUp = model3DConfiguration.Up;
            Vector3D vTarget = model3DConfiguration.Target;
            Mat4x4 matCameraRot = Mat4x4.MakeRotationY(fYaw);
			vLookDir = matCameraRot * vTarget;
			vTarget = vCamera + vLookDir;
            Mat4x4 matCamera = Mat4x4.PointAt(vCamera, vTarget, vUp);

            // Make view matrix from camera
            Mat4x4 matView = matCamera.QuickInverse();

            // Store triangles for rastering later
            drawTriangles(meshCube, matWorld, matView, matProj, vCamera, light_direction, false);

            return image;
        }




    }
}
