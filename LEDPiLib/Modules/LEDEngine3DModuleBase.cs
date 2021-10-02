using LEDPiLib.DataItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LEDPiLib.Modules.Helper;
using LEDPiLib.Modules.Model;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Diagnostics;

namespace LEDPiLib.Modules
{
    public abstract class LEDEngine3DModuleBase : ModuleBase
    {
        protected LEDEngine3D engine3D = new LEDEngine3D();
        protected Image<Rgba32> image;

        public LEDEngine3DModuleBase(ModuleConfiguration moduleConfiguration, float renderOffset, int handBrake = 0) : base(moduleConfiguration, renderOffset, handBrake)
        {
        }

        protected void drawTriangles(Mesh meshCube, Mat4x4 matWorld, Mat4x4 matView, Mat4x4 matProj, Vector3D vCamera, Vector3D light_direction, bool wire, bool withLight = true)
        {
            Stack<Triangle> vecTrianglesToRaster = new Stack<Triangle>();

            // Draw Triangles
            foreach (Triangle tri in meshCube.Tris)
            {
                // World Matrix Transform
                Triangle triTransformed = new Triangle(new List<Vector3D>()
                {
                    matWorld * tri.P[0],
                    matWorld * tri.P[1],
                    matWorld * tri.P[2],
                })
                { color = tri.color };


                Triangle triProjected = new Triangle(new List<Vector3D>());
                Triangle triViewed;

                // Calculate Triangle Normal
                Vector3D normal, line1, line2;

                // Get lines either side of Triangle
                line1 = triTransformed.P[1] - triTransformed.P[0];
                line2 = triTransformed.P[2] - triTransformed.P[0];

                // Take cross product of lines to get normal to Triangle surface
                normal = line1.CrossProduct(line2);

                // You normally need to normalise a normal!
                normal = normal.Normalise();

                // Get Ray from Triangle to camera
                Vector3D vCameraRay = triTransformed.P[0] - vCamera;

                // If ray is aligned with normal, then Triangle is visible
                if (normal * vCameraRay < 0.0f)
                {
                    if (withLight)
                    {
                        // Illumination
                        light_direction = light_direction.Normalise();

                        // How "aligned" are light direction and Triangle surface normal?
                        float dp = Math.Max(0.1f, light_direction * normal);

                        // Choose console colours as required (much easier with RGB)
                        triTransformed.color = engine3D.GetColour(dp);
                    }
                    else
                    {
                        triTransformed.color = tri.color;
                    }

                    // Convert World Space --> View Space
                    triViewed = new Triangle(
                        new List<Vector3D>()
                        {
                            matView * triTransformed.P[0],
                            matView * triTransformed.P[1],
                            matView * triTransformed.P[2]
                        })
                    { color = triTransformed.color };
                    
                    // Clip Viewed Triangle against near plane, this could form two additional
                    // additional Triangles. 
                    int nClippedTriangles = 0;
                    List<Triangle> clipped = new List<Triangle>();

                    nClippedTriangles = engine3D.Triangle_ClipAgainstPlane(new Vector3D(0.0f, 0.0f, 0.1f),
                        new Vector3D(0.0f, 0.0f, 1.0f), triViewed, ref clipped);

                    // We may end up with multiple Triangles form the clip, so project as
                    // required
                    for (int n = 0; n < nClippedTriangles; n++)
                    {
                        // Project Triangles from 3D --> 2D
                        triProjected.P.Add(matProj * clipped[n].P[0]);
                        triProjected.P.Add(matProj * clipped[n].P[1]);
                        triProjected.P.Add(matProj * clipped[n].P[2]);
                        triProjected.color = clipped[n].color;

                        // Scale into view, we moved the normalising into cartesian space
                        // out of the matrix.vector function from the previous videos, so
                        // do this manually
                        triProjected.P[0] = triProjected.P[0] / triProjected.P[0].W;
                        triProjected.P[1] = triProjected.P[1] / triProjected.P[1].W;
                        triProjected.P[2] = triProjected.P[2] / triProjected.P[2].W;

                        Vector3D triProjectedVector0 = triProjected.P[0];
                        Vector3D triProjectedVector1 = triProjected.P[1];
                        Vector3D triProjectedVector2 = triProjected.P[2];

                        // X/Y are inverted so put them back
                        triProjectedVector0.vector.X *= -1.0f;
                        triProjectedVector1.vector.X *= -1.0f;
                        triProjectedVector2.vector.X *= -1.0f;
                        triProjectedVector0.vector.Y *= -1.0f;
                        triProjectedVector1.vector.Y *= -1.0f;
                        triProjectedVector2.vector.Y *= -1.0f;

                        // Offset verts into visible normalised space
                        Vector3D vOffsetView = new Vector3D(1, 1, 0);
                        triProjectedVector0 += vOffsetView;
                        triProjectedVector1 += vOffsetView;
                        triProjectedVector2 += vOffsetView;
                        triProjectedVector0.vector.X *= 0.5f * (float)renderWidth;
                        triProjectedVector0.vector.Y *= 0.5f * (float)renderHeight;
                        triProjectedVector1.vector.X *= 0.5f * (float)renderWidth;
                        triProjectedVector1.vector.Y *= 0.5f * (float)renderHeight;
                        triProjectedVector2.vector.X *= 0.5f * (float)renderWidth;
                        triProjectedVector2.vector.Y *= 0.5f * (float)renderHeight;

                        triProjected.P[0] = triProjectedVector0;
                        triProjected.P[1] = triProjectedVector1;
                        triProjected.P[2] = triProjectedVector2;

                        // Store Triangle for sorting
                        vecTrianglesToRaster.Push(triProjected);
                    }
                }
            }

            // Clear Screen
            SetBackgroundColor(image);

            Stack<Triangle> listTriangles = null;
            List<Triangle> completeList = new List<Triangle>();

            // Loop through all transformed, viewed, projected, and sorted Triangles
            foreach (Triangle triToRaster in vecTrianglesToRaster)
            {
                if (listTriangles != null)
                    completeList.AddRange(listTriangles);

                listTriangles = new Stack<Triangle>();

                // Clip Triangles against all four screen edges, this could yield
                // a bunch of Triangles, so create a queue that we traverse to 
                //  ensure we only test new Triangles generated against planes

                // Add initial Triangle
                listTriangles.Push(triToRaster);
                int nNewTriangles = 1;

                for (int p = 0; p < 4; p++)
                {
                    int nTrisToAdd = 0;
                    List<Triangle> clipped = new List<Triangle>();
                    while (nNewTriangles > 0)
                    {
                        // Take Triangle from front of queue
                        Triangle test = listTriangles.Pop();
                        nNewTriangles--;

                        // Clip it against a plane. We only need to test each 
                        // subsequent plane, against subsequent new Triangles
                        // as all Triangles after a plane clip are guaranteed
                        // to lie on the inside of the plane. I like how this
                        // comment is almost completely and utterly justified
                        switch (p)
                        {
                            case 0: nTrisToAdd = engine3D.Triangle_ClipAgainstPlane(new Vector3D(0.0f, 0.0f, 0.0f), new Vector3D(0.0f, 1.0f, 0.0f), test, ref clipped); break;
                            case 1: nTrisToAdd = engine3D.Triangle_ClipAgainstPlane(new Vector3D(0.0f, (float)renderHeight - 1, 0.0f), new Vector3D(0.0f, -1.0f, 0.0f), test, ref clipped); break;
                            case 2: nTrisToAdd = engine3D.Triangle_ClipAgainstPlane(new Vector3D(0.0f, 0.0f, 0.0f), new Vector3D(1.0f, 0.0f, 0.0f), test, ref clipped); break;
                            case 3: nTrisToAdd = engine3D.Triangle_ClipAgainstPlane(new Vector3D((float)renderWidth - 1, 0.0f, 0.0f), new Vector3D(-1.0f, 0.0f, 0.0f), test, ref clipped); break;
                        }

                        // Clipping may yield a variable number of Triangles, so
                        // add these new ones to the back of the queue for subsequent
                        // clipping against next planes
                        for (int w = 0; w < nTrisToAdd; w++)
                            listTriangles.Push(clipped[w]);

                    }
                    nNewTriangles = listTriangles.Count();
                }
            }

            // pick up the last triangles
            if (listTriangles != null)
                completeList.AddRange(listTriangles);

            // Draw the transformed, viewed, clipped, projected, sorted, clipped Triangles
            foreach (Triangle t in completeList)
            {
                if(!wire)
                    engine3D.FillTriangle(t.P[0].vector.X, t.P[0].vector.Y, t.P[1].vector.X, t.P[1].vector.Y, t.P[2].vector.X, t.P[2].vector.Y, t.color);
                else
                    engine3D.DrawTriangle(t.P[0].vector.X, t.P[0].vector.Y, t.P[1].vector.X, t.P[1].vector.Y, t.P[2].vector.X, t.P[2].vector.Y, t.color);
            }

        }
    }
}
