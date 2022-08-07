namespace LEDPiLib.Modules.Model.Rain
{
    public class RainDrop
    {
        public RainDrop(int x, int y)
        {
            X = x;
            Y = y;
        }

        public int X { get; set; }
        public int Y { get; set; }

        public bool Recycled { get; set; }
    }
}