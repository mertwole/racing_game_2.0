namespace Editor.GameEntities
{
    public class Curvature
    {
        public double Start { get; set; }
        public double Length { get; set; }
        public double End { get => Start + Length; }
        public double Value { get; set; }

        public Curvature(double start, double length, double value)
        {
            Start = start;
            Length = length;
            Value = value;
        }
    }
}
