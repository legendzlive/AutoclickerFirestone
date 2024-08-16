using ImageMagick;
using System.Drawing;

namespace AutoclickerFirestone
{
    public class Pixel
    {
        public string Name;
        public Point Point;
        public MagickColor Color;

        public Pixel(string name, Point location, MagickColor color)
        {
            Name = name;
            Point = location;
            Color = color;
        }

        public override string ToString()
        {
            return this.Name;
        }

        internal string GetFilePath()
        {
            return string.Format($"pixels/FSB_{this.Name}_{this.Color.ToHexString()}_X{this.Point.X}Y{this.Point.Y}.txt");
        }
    }
}
