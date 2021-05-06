using System;
using System.Collections.Generic;
using System.Numerics;
using LEDPiLib.Modules.Model;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace LEDPiLib.Modules.Helper
{
    public class LEDEngine3D
    {
        public Image<Rgba32> Canvas { get; set; }

        public Vector3D Vector_IntersectPlane(Vector3D plane_p, Vector3D plane_n, Vector3D lineStart, Vector3D lineEnd, ref float t)
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
            Color retColor = Color.Black;

            int pixel_bw = (int)(13.0f * lum);
            switch (pixel_bw)
            {
                case 0: retColor = Color.Black; 
                        break;

                case 1:
                    retColor = Color.DarkGray.WithAlpha(0.25f);
					break;
                case 2:
                    retColor = Color.DarkGray.WithAlpha(0.5f);
                    break;
                case 3:
                    retColor = Color.DarkGray.WithAlpha(0.75f);
                    break;
				case 4:
                    retColor = Color.DarkGray.WithAlpha(1f);
                    break;

                case 5:
                    retColor = Color.Gray.WithAlpha(0.25f);
                    break;
                case 6:
                    retColor = Color.Gray.WithAlpha(0.5f);
                    break;
                case 7:
                    retColor = Color.Gray.WithAlpha(0.75f);
                    break;
                case 8:
                    retColor = Color.Gray.WithAlpha(1f);
                    break;

                case 9:
                    retColor = Color.White.WithAlpha(0.25f);
                    break;
                case 10:
                    retColor = Color.White.WithAlpha(0.5f);
                    break;
                case 11:
                    retColor = Color.White.WithAlpha(0.75f);
                    break;
                case 12:
                    retColor = Color.White.WithAlpha(1f);
                    break;

                default:
                    retColor = Color.Black.WithAlpha(1f);
                    break;
            }


            return retColor;
        }

		public int Triangle_ClipAgainstPlane(Vector3D plane_p, Vector3D plane_n, Triangle in_tri, ref List<Triangle> clipped)
		{
			// Make sure plane normal is indeed normal
			plane_n = plane_n.Normalise();

            int nInsidePointCount = 0;
            int nOutsidePointCount = 0;
            int nInsideTexCount = 0;
            int nOutsideTexCount = 0;

            // Create two temporary storage arrays to classify points either side of plane
            // If distance sign is positive, point lies on "inside" of plane
            Vector3D[] inside_points = new Vector3D[3];
            Vector3D[] outside_points = new Vector3D[3];
			Vector2D[] inside_tex = new Vector2D[3];
            Vector2D[] outside_tex = new Vector2D[3]; 


			// Get signed distance of each point in triangle to plane
			float d0 = dist(plane_n, plane_p, in_tri.P[0]);
			float d1 = dist(plane_n, plane_p, in_tri.P[1]);
			float d2 = dist(plane_n, plane_p, in_tri.P[2]);

			if (d0 >= 0) { inside_points[nInsidePointCount++] = in_tri.P[0]; inside_tex[nInsideTexCount++] = in_tri.T[0]; }
			else
			{
				outside_points[nOutsidePointCount++] = in_tri.P[0]; outside_tex[nOutsideTexCount++] = in_tri.T[0];
			}
			if (d1 >= 0)
			{
				inside_points[nInsidePointCount++] = in_tri.P[1]; inside_tex[nInsideTexCount++] = in_tri.T[1];
			}
			else
			{
				outside_points[nOutsidePointCount++] = in_tri.P[1]; outside_tex[nOutsideTexCount++] = in_tri.T[1];
			}
			if (d2 >= 0)
			{
				inside_points[nInsidePointCount++] = in_tri.P[2]; inside_tex[nInsideTexCount++] = in_tri.T[2];
			}
			else
			{
				outside_points[nOutsidePointCount++] = in_tri.P[2]; outside_tex[nOutsideTexCount++] = in_tri.T[2];
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
                Triangle out_tri1 = new Triangle(new List<Vector3D>(), new List<Vector2D>());
				clipped.Add(out_tri1);

				// Triangle should be clipped. As two points lie outside
				// the plane, the triangle simply becomes a smaller triangle

				// Copy appearance info to new triangle
				out_tri1.color = in_tri.color;

				// The inside point is valid, so keep that...
				out_tri1.P.Add(inside_points[0]);
				out_tri1.T.Add(inside_tex[0]);

				// but the two new points are at the locations where the 
				// original sides of the triangle (lines) intersect with the plane
				float t = 0;
				out_tri1.P.Add(Vector_IntersectPlane(plane_p, plane_n, inside_points[0], outside_points[0], ref t));
                out_tri1.T.Add(new Vector2D(t * (outside_tex[0].X - inside_tex[0].X) + inside_tex[0].X,
                    t * (outside_tex[0].Y - inside_tex[0].Y) + inside_tex[0].Y,
                    t * (outside_tex[0].W - inside_tex[0].W) + inside_tex[0].W));

				out_tri1.P.Add(Vector_IntersectPlane(plane_p, plane_n, inside_points[0], outside_points[1], ref t));
                out_tri1.T.Add(new Vector2D(t * (outside_tex[1].X - inside_tex[0].X) + inside_tex[0].X,
                    t * (outside_tex[1].Y - inside_tex[0].Y) + inside_tex[0].Y,
                    t * (outside_tex[1].W - inside_tex[0].W) + inside_tex[0].W));

				return 1; // Return the newly formed single triangle
			}

			if (nInsidePointCount == 2 && nOutsidePointCount == 1)
			{
                Triangle out_tri1 = new Triangle(new List<Vector3D>(), new List<Vector2D>());
                Triangle out_tri2 = new Triangle(new List<Vector3D>(), new List<Vector2D>());
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
				out_tri1.T.Add(inside_tex[0]);
				out_tri1.T.Add(inside_tex[1]);

				float t = 0;
				out_tri1.P.Add(Vector_IntersectPlane(plane_p, plane_n, inside_points[0], outside_points[0], ref t));
                out_tri1.T.Add(new Vector2D(t * (outside_tex[0].X - inside_tex[0].X) + inside_tex[0].X,
                    t * (outside_tex[0].Y - inside_tex[0].Y) + inside_tex[0].Y,
                    t * (outside_tex[0].W - inside_tex[0].W) + inside_tex[0].W));

				// The second triangle is composed of one of he inside points, a
				// new point determined by the intersection of the other side of the 
				// triangle and the plane, and the newly created point above
				out_tri2.P.Add(inside_points[1]);
				out_tri2.T.Add(inside_tex[1]);
				out_tri2.P.Add(out_tri1.P[2]);
				out_tri2.T.Add(out_tri1.T[2]);
				out_tri2.P.Add(Vector_IntersectPlane(plane_p, plane_n, inside_points[1], outside_points[0], ref t));
                out_tri2.T.Add(new Vector2D(t * (outside_tex[0].X - inside_tex[1].X) + inside_tex[1].X,
                    t * (outside_tex[0].Y - inside_tex[1].Y) + inside_tex[1].Y,
                    t * (outside_tex[0].W - inside_tex[1].W) + inside_tex[1].W));
				return 2; // Return two newly formed triangles which form a quad
			}

            return -1;
        }

        public void DrawTriangle(float x1, float y1, float x2, float y2, float x3, float y3, Color col)
        {
            drawLine(x1, y1, x2, y2, col);
            drawLine(x2, y2, x3, y3, col);
            drawLine(x3, y3, x1, y1, col);
        }

        protected void drawLine(float x1, float y1, float x2, float y2, Color col)
        {
            float x, y, dx, dy, dx1, dy1, px, py, xe, ye, i;
            dx = x2 - x1;
            dy = y2 - y1;
            dx1 = Math.Abs(dx);
            dy1 = Math.Abs(dy);
            px = 2 * dy1 - dx1;
            py = 2 * dx1 - dy1;
            if (dy1 <= dx1)
            {
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
                Draw(x, y, col);
                for (i = 0; x < xe; i++)
                {
                    x = x + 1;
                    if (px < 0)
                        px = px + 2 * dy1;
                    else
                    {
                        if ((dx < 0 && dy < 0) || (dx > 0 && dy > 0))
                            y = y + 1;
                        else
                            y = y - 1;
                        px = px + 2 * (dy1 - dx1);
                    }
                    Draw(x, y, col);
                }
            }
            else
            {
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
                Draw(x, y, col);
                for (i = 0; y < ye; i++)
                {
                    y = y + 1;
                    if (py <= 0)
                        py = py + 2 * dx1;
                    else
                    {
                        if ((dx < 0 && dy < 0) || (dx > 0 && dy > 0))
                            x = x + 1;
                        else
                            x = x - 1;
                        py = py + 2 * (dx1 - dy1);
                    }
                    Draw(x, y, col);
                }
            }
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
                int t = tempX; tempX = tempY; tempY = t;
            }

            void Drawline(int sx, int ex, int ny)
            {
                for (int i = sx; i <= ex; i++)
                    Draw(i, ny, col);
            }

            int t1x, t2x, y, minx, maxx, t1xp, t2xp;
            bool changed1 = false;
            bool changed2 = false;
            int signx1, signx2, dx1, dy1, dx2, dy2;
            int e1, e2;
            // Sort vertices
            if (y1 > y2) { Swap(ref y1, ref y2); Swap(ref x1, ref x2); }
            if (y1 > y3) { Swap(ref y1, ref y3); Swap(ref x1, ref x3); }
            if (y2 > y3) { Swap(ref y2, ref y3); Swap(ref x2, ref x3); }

            t1x = t2x = x1; y = y1;   // Starting points
            dx1 = (int)(x2 - x1); if (dx1 < 0) { dx1 = -dx1; signx1 = -1; }
            else signx1 = 1;
            dy1 = (int)(y2 - y1);

            dx2 = (int)(x3 - x1);
            if (dx2 < 0)
            {
                dx2 = -dx2; signx2 = -1;
            }
            else signx2 = 1;

            dy2 = (int)(y3 - y1);

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

            e2 = (int)(Convert.ToInt32(dx2) >> 1);
            // Flat top, just process the second half
            if (y1 == y2) goto next;
            e1 = (int)(Convert.ToInt32(dx1) >> 1);

            for (int i = 0; i < dx1;)
            {
                t1xp = 0; t2xp = 0;
                if (t1x < t2x) { minx = t1x; maxx = t2x; }
                else { minx = t2x; maxx = t1x; }
                // process first line until y value is about to change
                while (i < dx1)
                {
                    i++;
                    e1 += Convert.ToInt32(dy1);
                    while (e1 >= dx1)
                    {
                        e1 -= Convert.ToInt32(dx1);
                        if (changed1) t1xp = signx1;//t1x += signx1;
                        else goto next1;
                    }
                    if (changed1) break;
                    else t1x += signx1;
                }
            // Move line
            next1:
                // process second line until y value is about to change
                while (true)
                {
                    e2 += Convert.ToInt32(dy2);
                    while (e2 >= dx2)
                    {
                        e2 -= Convert.ToInt32(dx2);
                        if (changed2) t2xp = signx2;//t2x += signx2;
                        else goto next2;
                    }
                    if (changed2) break;
                    else t2x += signx2;

                    if (dy2 == 0)
                        break;
                }
            next2:
                if (minx > t1x) minx = t1x; if (minx > t2x) minx = t2x;
                if (maxx < t1x) maxx = t1x; if (maxx < t2x) maxx = t2x;
                Drawline(minx, maxx, y);    // Draw line from min to max points found on the y
                                            // Now increase y
                if (!changed1) t1x += signx1;
                t1x += t1xp;
                if (!changed2) t2x += signx2;
                t2x += t2xp;
                y += 1;
                if (y == y2) break;

            }
        next:
            // Second half
            dx1 = (int)(x3 - x2); if (dx1 < 0) { dx1 = -dx1; signx1 = -1; }
            else signx1 = 1;
            dy1 = (int)(y3 - y2);
            t1x = x2;

            if (dy1 > dx1)
            {   // swap values
                Swap(ref dy1, ref dx1);
                changed1 = true;
            }
            else changed1 = false;

            e1 = (int)(Convert.ToInt32(dx1) >> 1);

            for (int i = 0; i <= dx1; i++)
            {
                t1xp = 0; t2xp = 0;
                if (t1x < t2x) { minx = t1x; maxx = t2x; }
                else { minx = t2x; maxx = t1x; }
                // process first line until y value is about to change
                while (i < dx1)
                {
                    e1 += Convert.ToInt32(dy1);
                    while (e1 >= dx1)
                    {
                        e1 -= Convert.ToInt32(dx1);
                        if (changed1) { t1xp = signx1; break; }//t1x += signx1;
                        else goto next3;
                    }
                    if (changed1) break;
                    else t1x += signx1;
                    if (i < dx1) i++;
                }
            next3:
                // process second line until y value is about to change
                while (Convert.ToInt32(t2x) != Convert.ToInt32(x3))
                {
                    e2 += Convert.ToInt32(dy2);
                    while (e2 >= dx2)
                    {
                        e2 -= Convert.ToInt32(dx2);
                        if (changed2) t2xp = signx2;
                        else goto next4;
                    }
                    if (changed2) break;
                    else t2x += signx2;

                    if (Convert.ToInt32(dy2) == 0)
                        break;
                }
            next4:

                if (minx > t1x) minx = t1x; if (minx > t2x) minx = t2x;
                if (maxx < t1x) maxx = t1x; if (maxx < t2x) maxx = t2x;
                Drawline(minx, maxx, y);
                if (!changed1) t1x += signx1;
                t1x += t1xp;
                if (!changed2) t2x += signx2;
                t2x += t2xp;
                y += 1;
                if (y > y3) return;
            }
        }

        public void Draw(float x, float y, Color col)
        {
            int intX = Convert.ToInt32(x);
            int intY = Convert.ToInt32(y);

            if (intX >= 0 && intX < Canvas.Width && intY >= 0 && intY < Canvas.Height)
            {
                Canvas.GetPixelRowSpan(intY)[intX] = col.ToPixel<Rgba32>();
            }
        }

        public static float Constrain(float constrainValue, float constrainMin, float constrainMax)
        {
            return constrainValue < constrainMin
                ? constrainMin
                : (constrainValue > constrainMax ? constrainMax : constrainValue);
        }

        public static float Map(float s, float a1, float a2, float b1, float b2)
        {
            return b1 + (s - a1) * (b2 - b1) / (a2 - a1);
        }

    }
}
