using System;
using System.Collections.Generic;
using LEDPiLib.DataItems;
using LEDPiLib.Modules.Helper;
using LEDPiLib.Modules.Model;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using static LEDPiLib.LEDPIProcessorBase;

namespace LEDPiLib.Modules
{
    [LEDModule(LEDModules.ReactionDiffusion)]
    public class LEDReactionDiffusionModule : ModuleBase
    {
        private List<List<Cell>> grid = new List<List<Cell>>();
        private List<List<Cell>> prev = new List<List<Cell>>();

        private float dA = 1.0f;
        private float dB = 0.5f;
        private float feed = 0.055f;
        private float k = 0.062f;

        public LEDReactionDiffusionModule(ModuleConfiguration moduleConfiguration) : base(moduleConfiguration, 2f)
        {
            Random random = new Random();

            for (int i = 0; i < renderHeight; i++)
            {
                grid.Add(new List<Cell>());
                prev.Add(new List<Cell>());

                for (int j = 0; j < renderWidth; j++)
                {
                    grid[i].Add(new Cell());
                    prev[i].Add(new Cell());
                }
            }

            for (int n = 0; n < 10; n++)
            {
                int startX = random.Next(20, renderWidth - 20);
                int startY = random.Next(20, renderHeight - 20);

                for (int i = startY; i < startY + 10; i++)
                {
                    for (int j = startX; j < startX + 10; j++)
                    {
                        grid[i][j] = new Cell(1, 1);
                        prev[i][j] = new Cell(1, 1);
                    }
                }
            }
        }

        protected override bool completedRun()
        {
            return false;
        }

        protected override Image<Rgba32> Run()
        {
            Image<Rgba32> image = new Image<Rgba32>(renderHeight, renderWidth);
            SetBackgroundColor(image);

            for (int i = 1; i < renderHeight - 1; i++)
            {
                for (int j = 1; j < renderWidth - 1; j++)
                {
                    List<Cell> beforeRow = prev[i - 1];
                    List<Cell> currentRow = prev[i];
                    List<Cell> nextRow = prev[i + 1];

                    Cell spot = currentRow[j];
                    Cell newspot = grid[i][j];

                    float a = spot.A;
                    float b = spot.B;

                    float laplaceA = 0;
                    laplaceA += a * -1;
                    laplaceA += nextRow[j].A * 0.2f;
                    laplaceA += beforeRow[j].A * 0.2f;
                    laplaceA += currentRow[j + 1].A * 0.2f;
                    laplaceA += currentRow[j - 1].A * 0.2f;
                    laplaceA += beforeRow[j - 1].A * 0.05f;
                    laplaceA += nextRow[j - 1].A * 0.05f;
                    laplaceA += beforeRow[j + 1].A * 0.05f;
                    laplaceA += nextRow[j + 1].A * 0.05f;

                    float laplaceB = 0;
                    laplaceB += b * -1;
                    laplaceB += nextRow[j].B * 0.2f;
                    laplaceB += beforeRow[j].B * 0.2f;
                    laplaceB += currentRow[j + 1].B * 0.2f;
                    laplaceB += currentRow[j - 1].B * 0.2f;
                    laplaceB += beforeRow[j - 1].B * 0.05f;
                    laplaceB += nextRow[j - 1].B * 0.05f;
                    laplaceB += beforeRow[j + 1].B * 0.05f;
                    laplaceB += nextRow[j + 1].B * 0.05f;

                    newspot.A = a + (dA * laplaceA - a * b * b + feed * (1 - a)) * 1;
                    newspot.B = b + (dB * laplaceB + a * b * b - (k + feed) * b) * 1;

                    newspot.A = LEDEngine3D.Constrain(newspot.A, 0, 1);
                    newspot.B = LEDEngine3D.Constrain(newspot.B, 0, 1);
                }
            }

            for (int i = 0; i < renderHeight; i++)
            {
                for (int j = 0; j < renderWidth; j++)
                {
                    int color = Convert.ToInt32((grid[i][j].A - grid[i][j].B)) * 255;

                    image.GetPixelRowSpan(i)[j] = new Rgba32(255 - color, 255 - color, 255 - color);
                }
            }


            image.Mutate(c => c.Resize(LEDWidth, LEDHeight));
            copyGrid();
            return image;
        }

        private void copyGrid()
        {
            for (int i = 0; i < renderHeight; i++)
            {
                for (int j = 0; j < renderWidth; j++)
                {
                    prev[i][j] = new Cell(grid[i][j]);
                }
            }
        }
    }
}
