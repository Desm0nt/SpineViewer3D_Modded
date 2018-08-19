using System.Collections.Generic;
using System.Linq;
using Volot.DescriptionOfGeometry.Parameters;

namespace Volot.DescriptionOfGeometry.STL
{
    public class StlMesh
    {
        public List<Vertex> Vertices { get; set; }

        private List<List<int>> TrianglesList { get; set; }

        public StlMesh(List<StlTriangle> triangles)
        {
            Vertices = GetVertices(triangles);
            int k = 0;
            TrianglesList = GetTriangles(triangles);
        }

        public List<StlTriangle> GetStlTriangles()
        {
            List<StlTriangle> triangles = new List<StlTriangle>();

            foreach (var t in TrianglesList)
            {
                triangles.Add(new StlTriangle(new StlNormal(), Vertices[t.ElementAt(0)],
                    Vertices[t.ElementAt(1)], Vertices[t.ElementAt(2)]));
            }
            return triangles;
        }
        public List<StlTriangle> GetStlTrianglesU(Spine spine)
        {
            List<StlTriangle> triangles = new List<StlTriangle>();

            foreach (var t in TrianglesList)
            {
                if (spine.U.Contains(t.ElementAt(0)) && spine.U.Contains(t.ElementAt(1)) && spine.U.Contains(t.ElementAt(2)))
                { 
                triangles.Add(new StlTriangle(new StlNormal(), Vertices[t.ElementAt(0)],
                    Vertices[t.ElementAt(1)], Vertices[t.ElementAt(2)]));
                }
            }
            return triangles;
        }
        public List<StlTriangle> GetStlTrianglesD(Spine spine)
        {
            List<StlTriangle> triangles = new List<StlTriangle>();

            foreach (var t in TrianglesList)
            {
                if (spine.D.Contains(t.ElementAt(0)) && spine.D.Contains(t.ElementAt(1)) && spine.D.Contains(t.ElementAt(2)))
                {
                    triangles.Add(new StlTriangle(new StlNormal(), Vertices[t.ElementAt(0)],
                        Vertices[t.ElementAt(1)], Vertices[t.ElementAt(2)]));
                }
            }
            return triangles;
        }

        private List<Vertex> GetVertices(List<StlTriangle> triangles)
        {
            List<Vertex> list = new List<Vertex>();
            foreach (var tp in triangles)
            {
                list.Add(new Vertex(tp.Vertex1.X, tp.Vertex1.Y, tp.Vertex1.Z));
                list.Add(new Vertex(tp.Vertex2.X, tp.Vertex2.Y, tp.Vertex2.Z));
                list.Add(new Vertex(tp.Vertex3.X, tp.Vertex3.Y, tp.Vertex3.Z));
            }
            list = list.Distinct().ToList();
            var i = 0;
            foreach (var p in list)
            {
                p.IdInMesh = i;
                i++;
            }
            return list;
        }

        private List<List<int>> GetTriangles(List<StlTriangle> triangles)
        {
            TrianglesList = new List<List<int>>();
            foreach (var t in triangles)
            {
                TrianglesList.Add(new List<int>
                {
                    Vertices.Find(p => p.Equals(t.Vertex1)).IdInMesh,
                    Vertices.Find(p => p.Equals(t.Vertex2)).IdInMesh,
                    Vertices.Find(p => p.Equals(t.Vertex3)).IdInMesh,
                });
            }
            return TrianglesList;
        }
    }
}