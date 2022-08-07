using System.Numerics;

namespace LEDPiLib.Modules.Model.SpaceColonization
{
    public class Branch
    {
        Vector2 saveDir;
        float len = 2;

        public Branch(Vector2 v, Vector2 d)
        {
            Parent = null;
            Pos = new Vector2(v.X, v.Y);
            Dir = new Vector2(d.X, d.Y);
            saveDir = new Vector2(Dir.X, Dir.Y);
        }

        public Branch(Branch p)
        {
            Parent = p;
            Pos = Parent.Next();
            Dir = new Vector2(Parent.Dir.X, Parent.Dir.Y);
            saveDir = new Vector2(Dir.X, Dir.Y);
        }

        public Vector2 Pos { get; set; }
        public Vector2 Dir { get; set; }
        public int Count { get; set; } = 0;
        public Branch Parent { get; set; }

        public void Reset()
        {
            Count = 0;
            Dir = new Vector2(saveDir.X, saveDir.Y);
        }

        public Vector2 Next()
        {
            Vector2 v =  Dir * len;
            Vector2 next = Pos + v;
            return next;
        }
    }
}
