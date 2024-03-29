﻿using System;
using System.Collections.Generic;
using System.Numerics;
using LEDPiLib.DataItems;
using LEDPiLib.Modules.Helper;
using LEDPiLib.Modules.Model;
using LEDPiLib.Modules.Model.Common;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using static LEDPiLib.LEDPIProcessorBase;

namespace LEDPiLib.Modules
{
    [LEDModule(LEDModules.Hypercube)]
    public class LEDHypercubeModule : ModuleBase
    {
        private float angle;
        private readonly List<Vector4> points;
        private readonly float screenOffset;
        private readonly int secondDegree;

        public LEDHypercubeModule(ModuleConfiguration moduleConfiguration) : base(moduleConfiguration, 1f, 25)
        {
            points = new List<Vector4>()
            {
                new Vector4(-1, -1, -1, 1),
                new Vector4(1, -1, -1, 1),
                new Vector4(1, 1, -1, 1),
                new Vector4(-1, 1, -1, 1),
                new Vector4(-1, -1, 1, 1),
                new Vector4(1, -1, 1, 1),
                new Vector4(1, 1, 1, 1),
                new Vector4(-1, 1, 1, 1),
                new Vector4(-1, -1, -1, -1),
                new Vector4(1, -1, -1, -1),
                new Vector4(1, 1, -1, -1),
                new Vector4(-1, 1, -1, -1),
                new Vector4(-1, -1, 1, -1),
                new Vector4(1, -1, 1, -1),
                new Vector4(1, 1, 1, -1),
                new Vector4(-1, 1, 1, -1),
            };

            screenOffset = renderWidth / 2f;

            secondDegree = MathHelper.GlobalRandom().Next(1, 360 + 1);
        }

        protected override bool completedRun()
        {
            return false;
        }

        protected override Image<Rgba32> RunInternal()
        {
            List<Vector3D> projected3d = new List<Vector3D>();
            Image<Rgba32> image = GetNewImage();

            foreach (Vector4 point in points)
            {
                Mat4x4 rotationXY = new Mat4x4(new Matrix4x4((float)Math.Cos(angle), (float)-Math.Sin(angle), 0, 0, (float)Math.Sin(angle), (float)Math.Cos(angle), 0, 0, 0, 0, 1, 0, 0, 0, 0, 1));
                Mat4x4 rotationZW = new Mat4x4(new Matrix4x4(1, 0, 0, 0, 0, 1, 0, 0, 0, 0, (float)Math.Cos(angle), (float)-Math.Sin(angle), 0, 0, (float)Math.Sin(angle), (float)Math.Cos(angle)));
                Mat4x4 rotationRandom = new Mat4x4(new Matrix4x4(1, 0, 0, 0, 0, 0, (float)Math.Cos(secondDegree), (float)-Math.Sin(secondDegree), 0, 0, (float)Math.Sin(secondDegree), (float)Math.Cos(secondDegree), 0, 1, 0, 0));

                Vector4 rotated = point;
                rotated = Mat4x4.MatMul(rotationRandom, rotated);
                rotated = Mat4x4.MatMul(rotationZW, rotated);
                rotated = Mat4x4.MatMul(rotationXY, rotated);

                float distance = 2;
                float w = 1 / (distance - rotated.W);

                Mat4x4 projection = new Mat4x4(new Matrix4x4(w, 0, 0, 0, 0, w, 0, 0, 0, 0, w, 0, 0, 0, 0, 0));

                Vector3D projected = projection * rotated;
                projected *= (renderWidth / 8f);
                projected += screenOffset;
                projected3d.Add(projected);
            }


            foreach (Vector3D vector3D in projected3d)
            {
                image.Mutate(c => c.Fill(Color.White, new ComplexPolygon(new EllipsePolygon(new PointF(vector3D.vector.X, vector3D.vector.Y), 1f))));
            }

            //       // Connecting
            for (int i = 0; i < 4; i++)
            {
                connect(image, 0, i, (i + 1) % 4, projected3d);
                connect(image, 0, i + 4, ((i + 1) % 4) + 4, projected3d);
                connect(image, 0, i, i + 4, projected3d);
            }

            for (int i = 0; i < 4; i++)
            {
                connect(image, 8, i, (i + 1) % 4, projected3d);
                connect(image, 8, i + 4, ((i + 1) % 4) + 4, projected3d);
                connect(image, 8, i, i + 4, projected3d);
            }

            for (int i = 0; i < 8; i++)
            {
                connect(image, 0, i, i + 8, projected3d);
            }

            angle += 0.02f;
            return image;
        }

        private void connect(Image<Rgba32> image, int offset, int i, int j, List<Vector3D> localPoints)
        {
            Vector3D a = localPoints[i + offset];
            Vector3D b = localPoints[j + offset];
            
            image.Mutate(c => c.DrawLines(Color.White, 1f, new PointF[]{new PointF(a.vector.X, a.vector.Y), new PointF(b.vector.X, b.vector.Y) }));
        }
    }
}
