namespace LEDPiLib.Modules.Model.ReactionDiffusion
{
    public class Cell
    {
        public Cell(float a, float b)
        {
            A = a;
            B = b;
        }

        public Cell(Cell c)
        {
            A = c.A;
            B = c.B;
        }

        public Cell()
        {
        }

        public float A { get; set; } = 1;
        public float B { get; set; }
    }
}
