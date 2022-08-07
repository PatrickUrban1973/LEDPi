using System.Collections.Generic;

namespace LEDPiLib.Modules.Model.Maze
{
    public class CollisionMap
    {
        public float Width { get; private set; }
        public float Height { get; private set; }

        private Dictionary<int, Dictionary<int, bool>> rows = new Dictionary<int, Dictionary<int, bool>>();  

        public CollisionMap(Maze maze)
        {
            //Width = maze.Width * 2 + 2;
            //Height = maze.Height * 2 + 2;

            //for (int y = 0; y < Height; y++)
            //{
            //    Dictionary<int, bool> rowDict = new Dictionary<int, bool>(); 
            //    rows.Add(y, rowDict);
                
            //    for (int x = 0; x < Width; x++)
            //    {
            //        rowDict.Add(x, false);
            //    }   
            //}
            
        }
    }
}