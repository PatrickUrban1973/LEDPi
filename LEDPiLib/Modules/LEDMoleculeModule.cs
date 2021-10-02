using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using LEDPiLib.DataItems;
using LEDPiLib.Modules.Helper;
using LEDPiLib.Modules.Model;
using LEDPiLib.Modules.Model.Molecule;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using static LEDPiLib.LEDPIProcessorBase;

namespace LEDPiLib.Modules
{
    [LEDModule(LEDModules.Molecule)]
    public class LEDMoleculeModule : ModuleBase
    {

        private Image<Rgba32> image;
        private List<KeyValuePair<Vector2D, Vector2D>> molecules = new List<KeyValuePair<Vector2D, Vector2D>>();

        private MoleculeParticle p1;
        private MoleculeParticle p2;
        private int offset = 15;
        private int maxMolecules = 10;

        public LEDMoleculeModule(ModuleConfiguration moduleConfiguration) : base(moduleConfiguration, 2f, 30)
        {
            Random random = new Random();

            p1 = new MoleculeParticle(renderWidth / 2 - offset, renderHeight / 4 - offset);
            p2 = new MoleculeParticle(renderWidth / 2 + offset, renderHeight / 4 + offset);

            p1.Vel = new Vector2D(MathHelper.Map(Convert.ToSingle(random.NextDouble()), 0, 1, 1, 2), 0);
            p2.Vel = new Vector2D(MathHelper.Map(Convert.ToSingle(random.NextDouble()), 0, 1, 1, 2), 0);
        }

        protected override bool completedRun()
        {
            return false;
        }

        protected override Image<Rgba32> RunInternal()
        {
            image = new Image<Rgba32>(renderWidth, renderHeight);
            SetBackgroundColor(image);

            p1.ApplyForce(gravityForce(0.2f));
            p2.ApplyForce(gravityForce(0.2f));
            p1.ApplyForce(springForce(p1, p2, Convert.ToInt32(renderOffset * 10), 0.01f));
            p2.ApplyForce(springForce(p2, p1, Convert.ToInt32(renderOffset * 10), 0.01f));
            p1.Update();
            p2.Update();
            p1.Edges(renderWidth, renderHeight);
            p2.Edges(renderWidth, renderHeight);

            molecules.Add(new KeyValuePair<Vector2D, Vector2D>(p1.Pos, p2.Pos));

            if (molecules.Count > maxMolecules)
                molecules.RemoveAt(0);

            var temp = molecules.ToList();
            temp.Reverse();

            int counter = 0;
            foreach (KeyValuePair<Vector2D, Vector2D> molecule in temp)
            {
                byte greyscale = Convert.ToByte(MathHelper.Map(counter++, 0, maxMolecules, 255, 0));

                Color color = new Color(new Rgba32(greyscale, greyscale, greyscale));
                image.Mutate(c => c.DrawLines(color, 0.1f, new []{new PointF(molecule.Key.vector.X, molecule.Key.vector.Y), new PointF(molecule.Value.vector.X, molecule.Value.vector.Y) }));
                image.Mutate(c => c.Draw(color, .5f, new ComplexPolygon(new EllipsePolygon(new PointF(molecule.Key.vector.X, molecule.Key.vector.Y), renderOffset * 3))));
                image.Mutate(c => c.Draw(color, .5f, new ComplexPolygon(new EllipsePolygon(new PointF(molecule.Value.vector.X, molecule.Value.vector.Y), renderOffset * 3))));
            }

            return image;
        }

        private Vector2D gravityForce(float strength)
        {
            return new Vector2D(0, strength);
        }

        private Vector2D springForce(MoleculeParticle a, MoleculeParticle b, int len, float constant)
        {
            Vector2D force = b.Pos - a.Pos;
            float currentLen = Vector2.Distance(new Vector2(0,0), force.vector);
            force = new Vector2D(Vector2.Normalize(force.vector) * (len - currentLen));

            force *= -constant;

            return force;
        }

    }
}
