namespace Volot.DescriptionOfGeometry.Parameters
{
    /// <summary>
    /// Tag geometry from XML
    /// </summary>
    public class Geometry
    {
        public string Name { get; private set; }
        public double Value { get; private set; }

        public Geometry()
        { }

        public Geometry(string name, double value)
        {
            Name = name;
            Value = value;
        }
    }
}