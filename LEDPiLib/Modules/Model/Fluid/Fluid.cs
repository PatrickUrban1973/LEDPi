using System;
using System.Collections.Generic;

namespace LEDPiLib.Modules.Model.Fluid
{
    internal class Fluid
    {
        public static int IX(int x, int y)
        {
            x = x < 0 ? 0 : x > N - 1 ? N - 1 : x;
            y = y < 0 ? 0 : y > N - 1 ? N - 1 : y;
            return x + (y * N);
        }

        private const int iter = 16;
        private static int N;
        private readonly float dt;
        private readonly float diff;
        private readonly float visc;

        private readonly float[] s;
        public readonly float[] density;

        private readonly float[] Vx;
        private readonly float[] Vy;

        private readonly float[] Vx0;
        private readonly float[] Vy0;

        public Fluid(int n, float localDt, float diffusion, float viscosity)
        {
            N = n;
            dt = localDt;
            diff = diffusion;
            visc = viscosity;

            s = new float[N * N];
            density = new float[N * N];

            Vx = new float[N * N];
            Vy = new float[N * N];

            Vx0 = new float[N * N];
            Vy0 = new float[N * N];
        }

        public void step()
        {
            float localVisc = this.visc;
            float localDiff = this.diff;
            float localDt = dt;
            float[] localVx = Vx;
            float[] localVy = Vy;
            float[] localVx0 = Vx0;
            float[] localVy0 = Vy0;
            float[] localS = s;
            float[] localDensity = density;

            diffuse(1, localVx0, localVx, localVisc, localDt);
            diffuse(2, localVy0, localVy, localVisc, localDt);

            project(localVx0, localVy0, localVx, localVy);

            advect(1, localVx, localVx0, localVx0, localVy0, localDt);
            advect(2, localVy, localVy0, localVx0, localVy0, localDt);

            project(localVx, localVy, localVx0, localVy0);

            diffuse(0, localS, localDensity, localDiff, localDt);
            advect(0, localDensity, localS, localVx, localVy, localDt);
        }

        public void addDensity(int x, int y, float amount)
        {
            int index = IX(x, y);
            density[index] += amount;
        }

        public void addVelocity(int x, int y, float amountX, float amountY)
        {
            int index = IX(x, y);
            Vx[index] += amountX;
            Vy[index] += amountY;
        }

        private void diffuse(int b, float[] x, float[] x0, float localDiff, float localDt)
        {
            float a = localDt * localDiff * (N - 2) * (N - 2);
            lin_solve(b, x, x0, a, 1 + 4 * a);
        }

        private void lin_solve(int b, float[] x, float[] x0, float a, float c)
        {
            float cRecip = 1.0f / c;
            for (int k = 0; k < iter; k++)
            {
                for (int j = 1; j < N - 1; j++)
                {
                    for (int i = 1; i < N - 1; i++)
                    {
                        x[IX(i, j)] =
                          (x0[IX(i, j)]
                          + a * (x[IX(i + 1, j)]
                          + x[IX(i - 1, j)]
                          + x[IX(i, j + 1)]
                          + x[IX(i, j - 1)]
                          )) * cRecip;
                    }
                }

                set_bnd(b, x);
            }
        }
        private void project(float[] velocX, float[] velocY, float[] p, float[] div)
        {
            for (int j = 1; j < N - 1; j++)
            {
                for (int i = 1; i < N - 1; i++)
                {
                    div[IX(i, j)] = -0.5f * (
                      velocX[IX(i + 1, j)]
                      - velocX[IX(i - 1, j)]
                      + velocY[IX(i, j + 1)]
                      - velocY[IX(i, j - 1)]
                      ) / N;
                    p[IX(i, j)] = 0;
                }
            }

            set_bnd(0, div);
            set_bnd(0, p);
            lin_solve(0, p, div, 1, 4);

            for (int j = 1; j < N - 1; j++)
            {
                for (int i = 1; i < N - 1; i++)
                {
                    velocX[IX(i, j)] -= 0.5f * (p[IX(i + 1, j)]
                      - p[IX(i - 1, j)]) * N;
                    velocY[IX(i, j)] -= 0.5f * (p[IX(i, j + 1)]
                      - p[IX(i, j - 1)]) * N;
                }
            }
            set_bnd(1, velocX);
            set_bnd(2, velocY);
        }


        private void advect(int b, float[] d, IReadOnlyList<float> d0, IReadOnlyList<float> velocX, IReadOnlyList<float> velocY, float localDt)
        {
            float dtx = localDt * (N - 2);
            float dty = localDt * (N - 2);

            float Nfloat = N;
            int j;

            float jfloat, ifloat;
            
            for (j = 1, jfloat = 1; j < N - 1; j++, jfloat++)
            {
                int i;
                for (i = 1, ifloat = 1; i < N - 1; i++, ifloat++)
                {
                    float tmp1 = dtx * velocX[IX(i, j)];
                    float tmp2 = dty * velocY[IX(i, j)];
                    float x = ifloat - tmp1;
                    float y = jfloat - tmp2;

                    if (x < 0.5f) x = 0.5f;
                    if (x > Nfloat + 0.5f) x = Nfloat + 0.5f;
                    float i0 = (float)Math.Floor(x);
                    float i1 = i0 + 1.0f;
                    if (y < 0.5f) y = 0.5f;
                    if (y > Nfloat + 0.5f) y = Nfloat + 0.5f;
                    float j0 = (float)Math.Floor(y);
                    float j1 = j0 + 1.0f;

                    float s1 = x - i0;
                    float s0 = 1.0f - s1;
                    float t1 = y - j0;
                    float t0 = 1.0f - t1;

                    int i0i = Convert.ToInt32(i0);
                    int i1i = Convert.ToInt32(i1);
                    int j0i = Convert.ToInt32(j0);
                    int j1i = Convert.ToInt32(j1);

                    // DOUBLE CHECK THIS!!!
                    d[IX(i, j)] =
                      s0 * (t0 * d0[IX(i0i, j0i)] + t1 * d0[IX(i0i, j1i)]) +
                      s1 * (t0 * d0[IX(i1i, j0i)] + t1 * d0[IX(i1i, j1i)]);
                }
            }

            set_bnd(b, d);
        }



        void set_bnd(int b, float[] x)
        {
            for (int i = 1; i < N - 1; i++)
            {
                x[IX(i, 0)] = b == 2 ? -x[IX(i, 1)] : x[IX(i, 1)];
                x[IX(i, N - 1)] = b == 2 ? -x[IX(i, N - 2)] : x[IX(i, N - 2)];
            }
            for (int j = 1; j < N - 1; j++)
            {
                x[IX(0, j)] = b == 1 ? -x[IX(1, j)] : x[IX(1, j)];
                x[IX(N - 1, j)] = b == 1 ? -x[IX(N - 2, j)] : x[IX(N - 2, j)];
            }

            x[IX(0, 0)] = 0.5f * (x[IX(1, 0)] + x[IX(0, 1)]);
            x[IX(0, N - 1)] = 0.5f * (x[IX(1, N - 1)] + x[IX(0, N - 2)]);
            x[IX(N - 1, 0)] = 0.5f * (x[IX(N - 2, 0)] + x[IX(N - 1, 1)]);
            x[IX(N - 1, N - 1)] = 0.5f * (x[IX(N - 2, N - 1)] + x[IX(N - 1, N - 2)]);
        }
    }
}
