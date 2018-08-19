using MeshGenerator.Elements;
using MeshGenerator.Scene;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Volot.Model
{
    /// <summary>
    /// Repository wich works with files in STL format and data with type FEModel
    /// </summary>
    /// <typeparam name="ID"></typeparam>
    public class StlRepository<ID> : IRepository<ID, IScene>  // format STL ASCII
    {
        /// <summary>
        /// Create file in ASCII STL format and save finite element model
        /// </summary>
        /// <param name="id">Unique file name or path</param>
        /// <param name="item">Finite element model</param>
        public void Create(ID id, IScene item)
        {
            using (StreamWriter sw = new StreamWriter($"{id}.stl"))
            {
                Save(sw, id, item);
            }
        }
        public void Create2(ID id, IScene item)
        {
            using (StreamWriter sw = new StreamWriter($"{id}.stl"))
            {
                Save2(sw, id, item);
            }
        }

        /// <summary>
        /// NOT IMPLEMENTED
        /// </summary>
        /// <param name="id"></param>
        public void Delete(ID id)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// NOT IMPLEMENTED
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IScene Read(ID id)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Save finite element model to ASCII STL file  
        /// </summary>
        /// <param name="sw">Stream with path for save</param>
        /// <param name="item">Finite element model</param>
        void Save(StreamWriter sw, ID id, IScene item)
        {
            sw.WriteLine($"solid {id.ToString()}");
            foreach (var tetrahedron in item.Tetrahedrons)
            {
                List<Triangle> triangles = TrianglesFromTetrahedrons(tetrahedron);
                foreach (var triangle in triangles)
                {
                    Tuple<double, double, double> normal = Normal(triangle);
                    sw.WriteLine($"  facet normal {normal.Item1} {normal.Item2} {normal.Item3}");
                    sw.WriteLine("    outer loop");
                    foreach (var node in triangle.Nodes)
                    {
                        sw.WriteLine($"      vertex {node.PX} {node.PY} {node.PZ}");
                    }
                    sw.WriteLine("    endloop");
                    sw.WriteLine("  endfacet");
                }
            }
            sw.WriteLine($"endsolid {id.ToString()}");
        }
        void Save2(StreamWriter sw, ID id, IScene item)
        {
            sw.WriteLine($"solid {id.ToString()}");
            foreach (var tetrahedron in item.Tetrahedrons)
            {
                List<Triangle> triangles = TrianglesFromTetrahedrons(tetrahedron);
                foreach (var triangle in triangles)
                {
                    Tuple<double, double, double> normal = Normal(triangle);
                    sw.WriteLine($"  facet normal {normal.Item1} {normal.Item2} {normal.Item3}");
                    sw.WriteLine("    outer loop");
                    foreach (var node in triangle.Nodes)
                    {
                        sw.WriteLine($"      vertex {node.PX+700} {node.PY} {node.PZ} {node.DefColor}");
                    }
                    sw.WriteLine("    endloop");
                    sw.WriteLine("  endfacet");
                }
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
