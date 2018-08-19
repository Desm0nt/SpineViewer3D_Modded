using System;

namespace Volot.DescriptionOfGeometry.STL
{
    public struct StlNormal
    {
        public bool IsZero => Math.Abs(X) < Double.Epsilon && Math.Abs(Y) < Double.Epsilon && Math.Abs(Z) < Double.Epsilon;

        public double X { get; set; }

        public double Y { get; set; }

        public double Z { get; set; }

        public StlNormal(double x, double y, double z): this()
        {
            X = x;
            Y = y;
            Z = z;
        }

        public StlNormal(Vertex v) : this()
        {
            X = v.X;
            Y = v.Y;
            Z = v.Z;
        }

        public StlNormal Normalize()
        {
            var length = Math.Sqrt(X * X + Y * Y + Z * Z);
            if (Math.Abs(length) < Double.Epsilon)
                return new StlNormal();
            return new StlNormal(X / length, Y / length, Z / length);
        }
    }
}
