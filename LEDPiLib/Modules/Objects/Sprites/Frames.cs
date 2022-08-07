using System;
using System.Collections.Generic;
using System.Text;

namespace LEDPiLib.Modules.Objects.Sprites
{
    public class Frames
    {
        public Frame[] frames { get; set; }
    }

    public class Frame
    {
        public string name { get; set; }
        public Position position { get; set; }
    }

    public class Position
    {
        public int x { get; set; }
        public int y { get; set; }
        public int w { get; set; }
        public int h { get; set; }
    }
}
