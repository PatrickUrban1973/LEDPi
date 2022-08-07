using System;
using System.Collections.Generic;
using System.Numerics;
using LEDPiLib.DataItems;
using LEDPiLib.Modules.Helper;
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
        private readonly List<KeyValuePair<Vector2, Vector2>> molecules = new List<KeyValuePair<Vector2, Vector2>>();

        private readonly MoleculeParticle p1;
        private readonly MoleculeParticle p2;
        private const int offset = 15;

        public LEDMoleculeModule(ModuleConfiguration moduleConfiguration) : base(moduleConfiguration, 2f, 30)
        {
            p1 = new MoleculeParticle(renderWidth / 2 - offset, renderHeight / 4 - offset);
            p2 = new MoleculeParticle(renderWidth / 2 + offset, renderHeight / 4 + offset);

            p1.Vel = new Vector2(MathHelper.Map((float)MathHelper.GlobalRandom().NextDouble(), 0, 1, 1, 2), 0);
            p2.Vel = new Vector2(MathHelper.Map((float)MathHelper.GlobalRandom().NextDouble(), 0, 1, 1, 2), 0);
        }

        protected override bool completedRun()
        {
            return false;
        }

        protected override Image<Rgba32> RunInternal()
        {
            image = GetNewImage();
            UseBlend(image, 0.85f);

            molecules.Clear();
            
            p1.ApplyForce(gravityForce(0.2f));
            p2.ApplyForce(gravityForce(0.2f));
            p1.ApplyForce(springForce(p1, p2, Convert.ToInt32(renderOffset * 10), 0.01f));
            p2.ApplyForce(springForce(p2, p1, Convert.ToInt32(renderOffset * 10), 0.01f));
            p1.Update();
            p2.Update();
            p1.Edges(renderWidth, renderHeight);
            p2.Edges(renderWidth, renderHeight);

            molecules.Add(new KeyValuePair<Vector2, Vector2>(p1.Pos, p2.Pos));

            Color color = new Color(new Rgba32(255, 255, 255));
            
            foreach (KeyValuePair<Vector2, Vector2> molecule in molecules)
            {
                image.Mutate(c => c.DrawLines(color, 0.1f, new []{new PointF(molecule.Key.X, molecule.Key.Y), new PointF(molecule.Value.X, molecule.Value.Y) }));
                image.Mutate(c => c.Draw(color, .5f, new ComplexPolygon(new EllipsePolygon(new PointF(molecule.Key.X, molecule.Key.Y), renderOffset * 3))));
                image.Mutate(c => c.Draw(color, .5f, new ComplexPolygon(new EllipsePolygon(new PointF(molecule.Value.X, molecule.Value.Y), renderOffset * 3))));
            }

            SetLastPictureBlend(image);
            return image;
        }

        private Vector2 gravityForce(float strength)
        {
            return new Vector2(0, strength);
        }

        private Vector2 springForce(MoleculeParticle a, MoleculeParticle b, int len, float constant)
        {
            Vector2 force = b.Pos - a.Pos;
            float currentLen = Vector2.Distance(new Vector2(0,0), force);
            force = Vector2.Normalize(force) * (len - currentLen);

            force *= -constant;

            return force;
        }

    }
}
