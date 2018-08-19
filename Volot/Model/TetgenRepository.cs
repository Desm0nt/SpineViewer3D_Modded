using MeshGenerator.Elements;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Volot.Model
{
    public class TetgenRepository<ID> : IRepository<ID, List<Tetrahedron>>
    {
        public IDictionary<int, Node> Nodes { get; private set; } = new Dictionary<int, Node>();
        public List<Triangle> Triangles { get; private set; } = new List<Triangle>();
        private const int ID_MATERIAL = 1;

        public void Create(ID id, List<Tetrahedron> item)
        {
            throw new NotImplementedException();
        }
        public void Create2(ID id, List<Tetrahedron> item)
        {
            throw new NotImplementedException();
        }

        public void Delete(ID id)
        {
            throw new NotImplementedException();
        }

        public List<Tetrahedron> Read(ID id)
        {
            List<Tetrahedron> tetrahedrons = new List<Tetrahedron>();
            using (StreamReader reader = new StreamReader($"TetGen/{id}.1.mesh"))
            {
                while (!reader.ReadLine().Equals("Vertices")) ;
                ReadNodes(reader);
                while (!reader.ReadLine().Equals("Triangles")) ;
                ReadTriangles(reader);
                while (!reader.ReadLine().Equals("Tetrahedra")) ;
                tetrahedrons.AddRange(ReadTetrahedrons(reader));
                
            }

            return tetrahedrons;
        }

        List<Tetrahedron> ReadTetrahedrons(StreamReader reader)
        {
            reader.ReadLine();

            List<Tetrahedron> tetrahedrons = new List<Tetrahedron>();

            string currentLine = "";
            while (!(currentLine = reader.ReadLine()).Equals(""))
            {
                string[] line = currentLine.Split(' ').Where(x => x.CompareTo("") != 0).ToArray();
                
                List<Node> list = new List<Node>();

                for (int i = 0; i < line.Length - 1; i++)
                {
                    int globalIndex = Int32.Parse(line[i]) - 1;
                    list.Add(Nodes[globalIndex]);
                }

                tetrahedrons.Add(new Tetrahedron(list));
            }

            return tetrahedrons;
        }

        void ReadNodes(StreamReader reader)
        {
            reader.ReadLine();

            string currentLine = "";
            while (!(currentLine = reader.ReadLine()).Equals(""))
            {
                string[] line = currentLine.Split(' ')
                    .Where(x => x.CompareTo("") != 0)
                    .Select(s => s.Replace('.', ','))
                    .ToArray();

                Node node = new Node(Double.Parse(line[0]) / 10,
                    Double.Parse(line[2]) / 10,
                    Double.Parse(line[1]) / 10)
                //Node node = new Node(Double.Parse(line[0]),
                //    Double.Parse(line[1]),
                //    Double.Parse(line[2]))
                {
                    IdMaterial = ID_MATERIAL,
                    GlobalIndex = Nodes.Count
                };

                Nodes.Add(node.GlobalIndex, node);
            }
        }

        void ReadTriangles(StreamReader reader)
        {
            reader.ReadLine();

            string currentLine = "";
            while (!(currentLine = reader.ReadLine()).Equals(""))
            {
                string[] line = currentLine.Split(' ')
                    .Where(x => x.CompareTo("") != 0)
                    .ToArray();

                List<Node> list = new List<Node>();

                for (int i = 0; i < 3; i++)
                {
                    int globalIndex = Int32.Parse(line[i]) - 1;
                    list.Add(Nodes[globalIndex]);
                }

                Triangles.Add(new Triangle(list));
            }
        }
    }
}
