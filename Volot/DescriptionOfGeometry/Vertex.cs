using System;
using System.Collections.Generic;

namespace Volot.DescriptionOfGeometry
{
    /// <summary>
    /// Координаты точки в пространстве
    /// </summary>
    public class Vertex
    {
        public double X { get; }
        public double Y { get; }
        public double Z { get; }
        public int IdInMesh { get; set; }

        public Vertex(double ix, double iy, double iz)
        {
            IdInMesh = 0;
            X = ix;
            Y = iy;
            Z = iz;
        }

        public Vertex(double ix, double iy, double iz, int idInMesh)
        {
            IdInMesh = idInMesh;
            X = ix;
            Y = iy;
            Z = iz;
        }

        public Vertex(double ix, double iy)
        {
            X = ix;
            Y = iy;
            Z = 0;
            IdInMesh = 0;
        }

        /// <summary>
        /// Расстояние между 2-мя точками
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static double D(Vertex v1, Vertex v2)
        {
            return Math.Sqrt(Math.Pow(v2.X - v1.X, 2) + Math.Pow(v2.Y - v1.Y, 2) + Math.Pow(v2.Z - v1.Z, 2));
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType()) return false;
            Vertex p = (Vertex) obj;
            return Equals(p);
        }

        public bool Equals(Vertex o)
        {
            if (Math.Abs(X - o.X) < Double.Epsilon && Math.Abs(Y - o.Y) < Double.Epsilon && Math.Abs(Z - o.Z) < Double.Epsilon)
                return true;
            return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = X.GetHashCode();
                hashCode = (hashCode * 397) ^ Y.GetHashCode();
                hashCode = (hashCode * 397) ^ Z.GetHashCode();
                return hashCode;
            }
        }

        // TODO: Удалить метод
        public bool Equals(Vertex o, double eps)
        {
            if (Math.Abs(X - o.X) < eps && Math.Abs(Y - o.Y) < eps && Math.Abs(Z - o.Z) < eps)
                return true;
            return false;
        }
    }
}