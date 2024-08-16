namespace AutoclickerFirestone
{
    internal class Monitor
    {
        public string Device;
        public int BoundsWidth;
        public int BoundsHeight;
        public string Position;

        public Monitor(string Device, int BoundsWidth, int BoundsHeight, string Position) {
            this.Device = Device;
            this.BoundsWidth = BoundsWidth;
            this.BoundsHeight = BoundsHeight;
            this.Position = Position;
        }

        public override string ToString()
        {
            return this.Position;
        }
    }
}
