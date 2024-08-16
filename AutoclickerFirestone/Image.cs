using System.Drawing;

namespace AutoclickerFirestone
{
    public class Image
    {
        public string Path;
        public string Name;
        public Point PointA;
        public Point PointB;

        public Image(string Path, string Name, Point posA, Point posB)
        {
            this.Path = Path;
            this.Name = Name;
            this.PointA = posA;
            this.PointB = posB;
        }

        public override string ToString()
        {
            return this.Name;
        }

        public string GetFilePath()
        {
            return this.Path;
        }
    }
}
