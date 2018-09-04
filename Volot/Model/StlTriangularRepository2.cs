using MeshGenerator.Elements;
using Supercluster.KDTree;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Volot.Model
{
    public class StlTriangularRepository2<ID> : IRepository2<ID, List<Triangle>, List<Node>>
    {
        public void Create(ID id, List<Triangle> items)
        {
            using (StreamWriter sw = new StreamWriter($"{id}.stl"))
            {
                Save(sw, id, items);
            }
        }
        public void Create2(ID id, List<Triangle> items, List<Node> items2)
        {
            using (StreamWriter sw = new StreamWriter($"{id}.stl"))
            {
                Save2(sw, id, items, items2);
            }
        }

        public class DistancePoint
        {
            public double dist { get; set; }
            public double col { get; set; }
        }
        public class TPoint
        {
            public double X { get; set; }
            public double Y { get; set; }
            public double Z { get; set; }
        }

        public void Delete(ID id)
        {
            throw new NotImplementedException();
        }

        public List<Triangle> Read(ID id)
        {
            List<Triangle> triangles = new List<Triangle>();
            using (StreamReader sr = new StreamReader($"{id}.stl"))
            {
                string currentLine = "";
                while ((currentLine = sr.ReadLine()).Split(' ')[0].CompareTo("endsolid") != 0)
                {
                    if (currentLine.Split(' ').Last().CompareTo("loop") == 0)
                    {
                        Node first = ReadVertex(sr.ReadLine());
                        Node second = ReadVertex(sr.ReadLine());
                        Node third = ReadVertex(sr.ReadLine());

                        triangles.Add(new Triangle(first, second, third));
                    }
                }
            }

            return triangles;
        }

        private Node ReadVertex(string vertex)
        {
            List<string> lines = vertex.Split(' ').Where(s => s.CompareTo("") != 0).ToList();
            if (lines[0].CompareTo("vertex") == 0 || lines[0].CompareTo("\t\t\tvertex") == 0)
            {
                double x = Double.Parse(lines[1].Replace('.', ','));
                double y = Double.Parse(lines[3].Replace('.', ','));
                double z = Double.Parse(lines[2].Replace('.', ','));
                return new Node(x, y, z);
            }
            else
            {
                throw new Exception("Wrong vertex in stl.");
            }
        }

        /// <summary>
        /// Save finite element model to ASCII STL file  
        /// </summary>
        /// <param name="sw">Stream with path for save</param>
        /// <param name="items">Finite element model</param>
        void Save(StreamWriter sw, ID id, List<Triangle> items)
        {
            sw.WriteLine($"solid {id.ToString()}");

            foreach (var triangle in items)
            {
                Tuple<double, double, double> normal = Normal(triangle);
                sw.WriteLine($"  facet normal {normal.Item1} {normal.Item2} {normal.Item3}");
                sw.WriteLine("    outer loop");
                foreach (var node in triangle.Nodes)
                {
                    sw.WriteLine($"      vertex {node.X.ToString().Replace(',', '.') } {node.Y.ToString().Replace(',', '.')} {node.Z.ToString().Replace(',', '.')}");
                }
                sw.WriteLine("    endloop");
                sw.WriteLine("  endfacet");
            }
            sw.WriteLine($"endsolid {id.ToString()}");
        }

        void Save2(StreamWriter sw, ID id, List<Triangle> items, List<Node> items2)
        {
            var data = new List<double[]>();
            foreach (var a in items2)
            {
                data.Add(new double[] { a.X, a.Y, a.Z });
            }
            data.Sort((x, y) => x[0].CompareTo(y[0]));
            items2.Sort((x, y) => x.X.CompareTo(y.X));
            sw.WriteLine($"solid {id.ToString()}");
            foreach (var triangle in items)
            {
                Tuple<double, double, double> normal = Normal(triangle);
                sw.WriteLine($"  facet normal {normal.Item1} {normal.Item2} {normal.Item3}");
                sw.WriteLine("    outer loop");
                foreach (var node in triangle.Nodes)
                {                
                    var keys = data.Select(x => x[0]).ToList();
                    var index = keys.BinarySearch(node.X);
                    if (index < 0)
                    {
                        index = ~index;
                    }
                    if (index >= items2.Count)
                        index--;
                    var X_coord = items2[index].X;
                    var Ydata = data.Where(e => e[0]>=X_coord-5 && e[0] <= X_coord+5).ToList();
                    Ydata.Sort((x, y) => x[1].CompareTo(y[1]));
                    keys = Ydata.Select(x => x[1]).ToList();
                    index = keys.BinarySearch(node.Y);
                    if (index < 0)
                    {
                        index = ~index;
                    }
                    if (index >= Ydata.Count)
                        index--;
                    var Y_coord = Ydata[index][1];
                    var Zdata = data.Where(e => (e[0] >= X_coord - 5 && e[0] <= X_coord + 5) && (e[1] >= Y_coord - 5 && e[1] <= Y_coord + 5)).ToList();
                    Zdata.Sort((x, y) => x[2].CompareTo(y[2]));
                    keys = Zdata.Select(x => x[2]).ToList();
                    index = keys.BinarySearch(node.Z);
                    if (index < 0)
                    {
                        index = ~index;
                    }
                    if (index >= Zdata.Count)
                        index--;
                    var Z_coord = Zdata[index][2];
                    var color = items2.Where(e => (e.X >= X_coord - 5 && e.X <= X_coord + 5) && (e.Y >= Y_coord - 5 && e.Y <= Y_coord + 5) && (e.Z >= Z_coord - 5 && e.Z <= Z_coord + 5)).First();

                    sw.WriteLine($"      vertex {(node.X+700).ToString().Replace(',', '.') } {node.Y.ToString().Replace(',', '.')} {node.Z.ToString().Replace(',', '.')} {color.DefColor}");
                }
                sw.WriteLine("    endloop");
                sw.WriteLine("  endfacet");
            }
            sw.WriteLine($"endsolid {id.ToString()}");
        }

        /// <summary>
        /// Generate triangles from tetrahedron sides
        /// </summary>
        /// <param name="tetrahedron">Tetrahedron</param>
        /// <returns>List of triangles</returns>
        List<Triangle> TrianglesFromTetrahedrons(Tetrahedron tetrahedron)
        {
            List<Triangle> result = new List<Triangle>();
            for (int i = 0; i < tetrahedron.Nodes.Count; i++)
            {
                Triangle triangle = new Triangle(tetrahedron.Nodes[i], tetrahedron.Nodes[(i + 1) % 4], tetrahedron.Nodes[(i + 2) % 4]);
                result.Add(triangle);
            }
            return result;
        }


        /// <summary>
        /// Generate normal by the triangle
        /// </summary>
        /// <param name="triangle">Triangle</param>
        /// <returns>Cortege of elemnts of the normal</returns>
        Tuple<double, double, double> Normal(Triangle triangle)
        {
            double px = ItemNormal(triangle.Nodes, "X");
            double py = ItemNormal(triangle.Nodes, "Y");
            double pz = ItemNormal(triangle.Nodes, "Z");

            return new Tuple<double, double, double>(px, py, pz);
        }

        /// <summary>
        /// Element of the normal
        /// </summary>
        /// <param name="nodes">Nodes of the triangle for generating element of normal</param>
        /// <param name="axis">Coordinate axis</param>
        /// <returns></returns>
        double ItemNormal(List<Node> nodes, string axis)
        {
            bool isEquals = true;
            foreach (var item in nodes)
            {
                switch (axis)
                {
                    case "X": if (!nodes[0].X.Equals(item.X)) isEquals = false; break;
                    case "Y": if (!nodes[0].Y.Equals(item.Y)) isEquals = false; break;
                    case "Z": if (!nodes[0].Z.Equals(item.Z)) isEquals = false; break;
                }

            }

            return isEquals ? 1 : 0;
        }
    }
}
