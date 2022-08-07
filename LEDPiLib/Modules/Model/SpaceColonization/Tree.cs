using LEDPiLib.Modules.Helper;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace LEDPiLib.Modules.Model.SpaceColonization
{
    public class Tree
    {
        private List<Branch> branches = new List<Branch>();
        private List<Leaf> leaves = new List<Leaf>();
        private LEDEngine3D engine3D = new  LEDEngine3D();

        public Tree(int maxWidth, int maxHeigth, float max_dist)
        {
            for (int i = 0; i < 2000; i++)
            {
                leaves.Add(new Leaf(maxWidth, maxHeigth));
            }
            Branch root = new Branch(new Vector2(maxWidth / 2, maxHeigth), new Vector2(0, -1));
            branches.Add(root);
            Branch current = new Branch(root);

            while (!CloseEnough(current, max_dist))
            {
                Branch trunk = new Branch(current);
                branches.Add(trunk);
                current = trunk;
            }
        }

        public bool CloseEnough(Branch b, float max_dist)
        {
            foreach(Leaf l in leaves)
            {
                float d = Vector2.Distance(b.Pos, l.Pos);
                if (d < max_dist)
                {
                    return true;
                }
            }
            return false;
        }

        public void Grow(float min_dist, float max_dist)
        {
            foreach (Leaf l in leaves)
            {
                Branch closest = null;
                Vector2 closestDir = new Vector2();
                float record = -1;

                foreach (Branch b in branches)
                {
                    Vector2 dir = l.Pos - b.Pos;
                    float d = MathHelper.Mag(dir);
                    if (d < min_dist)
                    {
                        l.Reached = true;
                        closest = null;
                        break;
                    }
                    else if (d > max_dist)
                    {

                    }
                    else if (closest == null || d < record)
                    {
                        closest = b;
                        closestDir = dir;
                        record = d;
                    }
                }
                if (closest != null)
                {
                    closestDir = Vector2.Normalize(closestDir);
                    closest.Dir += closestDir;
                    closest.Count++;
                }
            }

            for (int i = branches.Count - 1; i >= 0; i--)
            {
                Branch b = branches[i];
                if (b.Count > 0)
                {
                    b.Dir /= b.Count;
                    b.Dir = Vector2.Normalize(b.Dir);

                    Branch newB = new Branch(b);
                    branches.Add(newB);
                    b.Reset();
                }
            }
        }

        public void Show(Image<Rgba32> image)
        {
            List<Tuple<Vector2, Vector2>> lines = new List<Tuple<Vector2, Vector2>>();
            engine3D.Canvas = image;
            foreach (Leaf l in leaves.Where(c => c.Reached))
            {
                l.Show(image);
            }

            foreach (Branch b in branches)
            {
                if (b.Parent != null)
                {
                    lines.Add(new Tuple<Vector2, Vector2>(b.Pos, b.Parent.Pos));
                }
            }
            engine3D.DrawLines(lines, Color.SaddleBrown);
        }
    }
}
