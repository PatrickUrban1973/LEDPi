using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using LEDPiLib.Modules.Model;
using LEDPiLib.Modules.Model.Common;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace LEDPiLib.Modules.Helper
{
    public class LEDEngine3D
    {
        private Image<Rgba32> canvas;
        private int width;
        private int height;

        private static readonly Color[] greyScaleMap = {
            Color.Black,
            Color.DarkGray.WithAlpha(0.25f),
            Color.DarkGray.WithAlpha(0.5f),
            Color.DarkGray.WithAlpha(0.75f),
            Color.DarkGray.WithAlpha(1f),
            Color.Gray.WithAlpha(0.25f),
            Color.Gray.WithAlpha(0.5f),
            Color.Gray.WithAlpha(0.75f),
            Color.Gray.WithAlpha(1f),
            Color.White.WithAlpha(0.25f),
            Color.White.WithAlpha(0.5f),
            Color.White.WithAlpha(0.75f),
            Color.White.WithAlpha(1f),
            Color.Black.WithAlpha(1f),
        };

        public Image<Rgba32> Canvas
        {
            get
            {
                return canvas;
            }
            set
            {
                canvas = value;
                width = canvas.Width;
                height = canvas.Height;
            }
        }

        private static Vector3D Vector_IntersectPlane(Vector3D plane_p, Vector3D plane_n, Vector3D lineStart, Vector3D lineEnd, ref float t)
		{
			plane_n = plane_n.Normalise();
            float plane_d = -(plane_n * plane_p);
			float ad = lineStart * plane_n;
			float bd = lineEnd * plane_n;
			t = (-plane_d - ad) / (bd - ad);
            Vector3D lineStartToEnd = lineEnd - lineStart;
            Vector3D lineToIntersect = lineStartToEnd + t;
			return lineStart + lineToIntersect;
		}

        private float dist(Vector3D plane_n, Vector3D plane_p, Vector3D p)
        {
            Vector3D n = p.Normalise();
            return (plane_n.vector.X * n.vector.X + plane_n.vector.Y * n.vector.Y + plane_n.vector.Z * n.vector.Z - (plane_n * plane_p));
		}

        public Color GetColour(float lum)
        {
            return greyScaleMap[(int)MathHelper.Map(lum, 0, 1, 0, 13)];
        }

		public int Triangle_ClipAgainstPlane(Vector3D plane_p, Vector3D plane_n, Triangle in_tri, ref List<Triangle> clipped)
		{
			// Make sure plane normal is indeed normal
			plane_n = plane_n.Normalise();

            int nInsidePointCount = 0;
            int nOutsidePointCount = 0;

            // Create two temporary storage arrays to classify points either side of plane
            // If distance sign is positive, point lies on "inside" of plane
            Vector3D[] inside_points = new Vector3D[3];
            Vector3D[] outside_points = new Vector3D[3];


			// Get signed distance of each point in triangle to plane
			float d0 = dist(plane_n, plane_p, in_tri.P[0]);
			float d1 = dist(plane_n, plane_p, in_tri.P[1]);
			float d2 = dist(plane_n, plane_p, in_tri.P[2]);

			if (d0 >= 0) { inside_points[nInsidePointCount++] = in_tri.P[0]; }
			else
			{
				outside_points[nOutsidePointCount++] = in_tri.P[0]; 
			}
			if (d1 >= 0)
			{
				inside_points[nInsidePointCount++] = in_tri.P[1]; 
			}
			else
			{
				outside_points[nOutsidePointCount++] = in_tri.P[1]; 
			}
			if (d2 >= 0)
			{
				inside_points[nInsidePointCount++] = in_tri.P[2]; 
			}
			else
			{
				outside_points[nOutsidePointCount++] = in_tri.P[2]; 
			}

			// Now classify triangle points, and break the input triangle into 
			// smaller output triangles if required. There are four possible
			// outcomes...

			if (nInsidePointCount == 0)
			{
				// All points lie on the outside of plane, so clip whole triangle
				// It ceases to exist

				return 0; // No returned triangles are valid
			}

			if (nInsidePointCount == 3)
			{
				// All points lie on the inside of plane, so do nothing
				// and allow the triangle to simply pass through
				clipped.Add(in_tri);
				return 1; // Just the one returned original triangle is valid
			}

			if (nInsidePointCount == 1 && nOutsidePointCount == 2)
            {
                Triangle out_tri1 = new Triangle(new List<Vector3D>());
				clipped.Add(out_tri1);

				// Triangle should be clipped. As two points lie outside
				// the plane, the triangle simply becomes a smaller triangle

				// Copy appearance info to new triangle
				out_tri1.color = in_tri.color;

				// The inside point is valid, so keep that...
				out_tri1.P.Add(inside_points[0]);

				// but the two new points are at the locations where the 
				// original sides of the triangle (lines) intersect with the plane
				float t = 0;
				out_tri1.P.Add(Vector_IntersectPlane(plane_p, plane_n, inside_points[0], outside_points[0], ref t));
                out_tri1.P.Add(Vector_IntersectPlane(plane_p, plane_n, inside_points[0], outside_points[1], ref t));

				return 1; // Return the newly formed single triangle
			}

			if (nInsidePointCount == 2 && nOutsidePointCount == 1)
			{
                Triangle out_tri1 = new Triangle(new List<Vector3D>()) { color = in_tri.color };
                Triangle out_tri2 = new Triangle(new List<Vector3D>()) { color = in_tri.color };
                clipped.Add(out_tri1);
                clipped.Add(out_tri2);

				// Triangle should be clipped. As two points lie inside the plane,
				// the clipped triangle becomes a "quad". Fortunately, we can
				// represent a quad with two new triangles

				// Copy appearance info to new triangles
				out_tri1.color = in_tri.color;
				out_tri2.color = in_tri.color;

				// The first triangle consists of the two inside points and a new
				// point determined by the location where one side of the triangle
				// intersects with the plane
				out_tri1.P.Add(inside_points[0]);
				out_tri1.P.Add(inside_points[1]);

				float t = 0;
				out_tri1.P.Add(Vector_IntersectPlane(plane_p, plane_n, inside_points[0], outside_points[0], ref t));

				// The second triangle is composed of one of he inside points, a
				// new point determined by the intersection of the other side of the 
				// triangle and the plane, and the newly created point above
				out_tri2.P.Add(inside_points[1]);
				out_tri2.P.Add(out_tri1.P[2]);
				out_tri2.P.Add(Vector_IntersectPlane(plane_p, plane_n, inside_points[1], outside_points[0], ref t));

                return 2; // Return two newly formed triangles which form a quad
			}

            return -1;
        }

        public void DrawLines(List<Tuple<Vector2, Vector2>> lines, Color col)
        {
            List<Vector2> points = new List<Vector2>();
            lines.ForEach(p => points.AddRange(GetLineVectors(p.Item1.X, p.Item1.Y, p.Item2.X, p.Item2.Y)));

            Draw(points, col);
        }

        public void DrawLine(Vector2 a, Vector2 b, Color col)
        {
            Draw(GetLineVectors(a.X, a.Y, b.X, b.Y), col);
        }

        public void DrawFilledRectangle(Model.Rectangle rectangle)
        {
            List<Vector2> vectors = new List<Vector2>();
            int startX = Convert.ToInt32(rectangle.Pos.X);
            int endX = Convert.ToInt32(rectangle.Pos.X + rectangle.Size.X);
            int startY = Convert.ToInt32(rectangle.Pos.Y);
            int endY = Convert.ToInt32(rectangle.Pos.Y + rectangle.Size.Y);

            for(int x = startX; x <= endX; x++)
            {
                for (int y = startY; y <= endY; y++)
                {
                    vectors.Add(new Vector2(x, y));
                }
            }

            Draw(vectors, rectangle.color);
        }

        public void DrawTriangle(float x1, float y1, float x2, float y2, float x3, float y3, Color col)
        {
            Draw(GetLineVectors(x1, y1, x2, y2).Concat(GetLineVectors(x2, y2, x3, y3)).Concat(GetLineVectors(x3, y3, x1, y1)), col);
        }

        public List<Vector2> GetLineVectors(float x1, float y1, float x2, float y2)
        {
            List<Vector2> vectors = new List<Vector2>();
            float x, y;
            float dx = x2 - x1;
            float dy = y2 - y1;
            float dx1 = Math.Abs(dx);
            float dy1 = Math.Abs(dy);
            float px = 2 * dy1 - dx1;
            float py = 2 * dx1 - dy1;
            if (dy1 <= dx1)
            {
                float xe;                
                if (dx >= 0)
                {
                    x = x1;
                    y = y1;
                    xe = x2;
                }
                else
                {
                    x = x2;
                    y = y2;
                    xe = x1;
                }
                vectors.Add(new Vector2(x,y));
                while (x < xe)
                {
                    x++;
                    if (px < 0)
                        px += 2 * dy1;
                    else
                    {
                        if ((dx < 0 && dy < 0) || (dx > 0 && dy > 0))
                            y++;
                        else
                            y--;
                        px += 2 * (dy1 - dx1);
                    }
                    vectors.Add(new Vector2(x, y));
                }
            }
            else
            {
                float ye;
                if (dy >= 0)
                {
                    x = x1;
                    y = y1;
                    ye = y2;
                }
                else
                {
                    x = x2;
                    y = y2;
                    ye = y1;
                }
                vectors.Add(new Vector2(x, y));
                while (y < ye)
                {
                    y += 1;
                    if (py <= 0)
                        py += 2 * dx1;
                    else
                    {
                        if ((dx < 0 && dy < 0) || (dx > 0 && dy > 0))
                            x++;
                        else
                            x--;
                        py += 2 * (dx1 - dy1);
                    }
                    vectors.Add(new Vector2(x, y));
                }
            }

            return vectors;
        }

        public void FillTriangle(float fx1, float fy1, float fx2, float fy2, float fx3, float fy3, Color col)
        {
            int x1 = Convert.ToInt32(fx1);
            int y1 = Convert.ToInt32(fy1);
            int x2 = Convert.ToInt32(fx2);
            int y2 = Convert.ToInt32(fy2);
            int x3 = Convert.ToInt32(fx3);
            int y3 = Convert.ToInt32(fy3);

            void Swap(ref int tempX, ref int tempY)
            {
                int t = tempX; 
                tempX = tempY; 
                tempY = t;
            }

            void Drawline(int sx, int ex, int ny, Color color)
            {
                int realsx = sx;
                int realex = ex;

                if (ny < 0 || ny > height)
                    return;

                if (realsx > width)
                    return;

                if (realsx < 0)
                    realsx = 0;

                if (realex > width)
                    realex = width;

                Draw(realsx, realex, ny, color);
            }

            int t2x, minx, t1xp, t2xp;
            bool changed1 = false;
            bool changed2 = false;
            int e1, signx1, signx2, maxx;
            // Sort vertices
            if (y1 > y2) { Swap(ref y1, ref y2); Swap(ref x1, ref x2); }
            if (y1 > y3) { Swap(ref y1, ref y3); Swap(ref x1, ref x3); }
            if (y2 > y3) { Swap(ref y2, ref y3); Swap(ref x2, ref x3); }

            int t1x = t2x = x1; // Starting points 
            int y = y1;  
            int dx1 = (x2 - x1); 
            
            if (dx1 < 0) 
            { 
                dx1 = -dx1;
                signx1 = -1;
            }
            else 
                signx1 = 1;
            
            int dy1 = (y2 - y1);

            int dx2 = (x3 - x1);
            if (dx2 < 0)
            {
                dx2 = -dx2; 
                signx2 = -1;
            }
            else signx2 = 1;

            int dy2 = (y3 - y1);

            if (dy1 > dx1)
            {   // swap values
                Swap(ref dx1, ref dy1);
                changed1 = true;
            }
            if (dy2 > dx2)
            {   // swap values
                Swap(ref dy2, ref dx2);
                changed2 = true;
            }

            int e2 = dx2 >> 1;
            // Flat top, just process the second half
            if (y1 == y2) 
                goto next;
            
            e1 = dx1 >> 1;

            for (int i = 0; i < dx1;)
            {
                t1xp = 0; t2xp = 0;
                if (t1x < t2x)
                {
                    minx = t1x;
                    maxx = t2x;
                }
                else
                {
                    minx = t2x;
                    maxx = t1x;
                }

                // process first line until y value is about to change
                while (i < dx1)
                {
                    i++;
                    e1 += dy1;
                    while (e1 >= dx1)
                    {
                        e1 -= dx1;
                        if (changed1) 
                            t1xp = signx1;
                        else 
                            goto next1;
                    }
                    if (changed1) break;
                    
                    t1x += signx1;
                }
                
                // Move line
                next1:
                // process second line until y value is about to change
                while (true)
                {
                    e2 += dy2;
                    while (e2 >= dx2)
                    {
                        e2 -= dx2;
                        if (changed2) 
                            t2xp = signx2;
                        else 
                            goto next2;
                    }
                    if (changed2) break;
                    else t2x += signx2;

                    if (dy2 == 0)
                        break;
                }
                
                next2:
                if (minx > t1x) minx = t1x; if (minx > t2x) minx = t2x;
                if (maxx < t1x) maxx = t1x; if (maxx < t2x) maxx = t2x;
                Drawline(minx, maxx, y, col);    // Draw line from min to max points found on the y
                                            // Now increase y
                if (!changed1) 
                    t1x += signx1;
                
                t1x += t1xp;
                if (!changed2) 
                    t2x += signx2;
                t2x += t2xp;
                y += 1;
                if (y == y2) 
                    break;
            }

            next:
            // Second half
            dx1 = (x3 - x2);
            if (dx1 < 0)
            {
                dx1 = -dx1;
                signx1 = -1;
            }
            else 
                signx1 = 1;
            
            dy1 = (y3 - y2);
            t1x = x2;

            if (dy1 > dx1)
            {   // swap values
                Swap(ref dy1, ref dx1);
                changed1 = true;
            }
            else 
                changed1 = false;

            e1 = dx1 >> 1;

            for (int i = 0; i <= dx1; i++)
            {
                t1xp = 0; t2xp = 0;
                if (t1x < t2x)
                {
                    minx = t1x;
                    maxx = t2x;
                }
                else
                {
                    minx = t2x;
                    maxx = t1x;
                }

                // process first line until y value is about to change
                while (i < dx1)
                {
                    e1 += dy1;
                    while (e1 >= dx1)
                    {
                        e1 -= dx1;
                        if (changed1)
                        {
                            t1xp = signx1;
                            break;
                        }
                        else 
                            goto next3;
                    }
                    if (changed1) 
                        break;
                    else 
                        t1x += signx1;
                    if (i < dx1) 
                        i++;
                }
                
                next3:
                // process second line until y value is about to change
                while (t2x != x3)
                {
                    e2 += dy2;
                    while (e2 >= dx2)
                    {
                        e2 -= dx2;
                        
                        if (changed2) 
                            t2xp = signx2;
                        else 
                            goto next4;
                    }
                    if (changed2) 
                        break;
                    else 
                        t2x += signx2;

                    if (dy2 == 0)
                        break;
                }
                
                next4:
                if (minx > t1x) 
                    minx = t1x; 
                if (minx > t2x) 
                    minx = t2x;
                if (maxx < t1x) 
                    maxx = t1x; 
                if (maxx < t2x) 
                    maxx = t2x;
                Drawline(minx, maxx, y, col);
                if (!changed1) 
                    t1x += signx1;

                t1x += t1xp;

                if (!changed2) 
                    t2x += signx2;
                
                t2x += t2xp;
                y += 1;
                
                if (y > y3) 
                    return;
            }
        }

        public void Draw(IEnumerable<Vector2> vectors, Color col)
        {
            Canvas.ProcessPixelRows(accessor =>
            {
                foreach (IGrouping<float, Vector2> vector2s in vectors.GroupBy(c => c.Y))
                {
                    int y = (int) vector2s.Key;

                    if (y >= 0 && y < height)
                    {
                        var row = accessor.GetRowSpan(y);

                        foreach (Vector2 vector2 in vector2s)
                        {
                            int x = (int) vector2.X;

                            if (x >= 0 && x < width)
                            {
                                row[x] = col;
                            }
                        }
                    }
                }
            });
        }
      
        public void Draw(float x, float y, Color col)
        {
            int intX = Convert.ToInt32(x);
            int intY = Convert.ToInt32(y);
            Canvas.ProcessPixelRows(accessor =>
            {
                if (intY >= 0 && intY < height)
                {
                    var row = accessor.GetRowSpan(intY);
                    if (intX >= 0 && intX < width)
                    {
                        row[intX] = col;
                    }
                }
            });
        }

        private void Draw(float fromX, float toX, float y, Color col)
        {
            int intFromX = (int) fromX;
            int intToX = (int)toX;
            int intY = (int)y;
            Canvas.ProcessPixelRows(accessor =>
            {
                if (intY >= 0 && intY < height)
                {
                    var row = accessor.GetRowSpan(intY);
                    
                    for(;intFromX <= intToX; intFromX++)
                    {
                        if (intFromX >= 0 && intFromX < width)
                        {
                            row[intFromX] = col;
                        }
                    }
                }
            });
        }
    }
}
