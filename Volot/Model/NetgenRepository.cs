using MeshGenerator.Elements;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Volot.Model
{
    public class NetgenRepository<ID> : IRepository<ID, List<Tetrahedron>>
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
            using (StreamReader reader = new StreamReader($"NetGen/{id}.vol"))
            {
                while (!reader.ReadLine().Equals("surfaceelementsgi")) ;
                ReadTriangles(reader);
                while (!reader.ReadLine().Equals("volumeelements")) ;
                tetrahedrons.AddRange(ReadTetrahedrons(reader));
                while (!reader.ReadLine().Equals("points")) ;
                ReadNodes(reader);
            }
            foreach (var tngl in Triangles)
            {
                for (int i = 0; i < tngl.Nodes.Count; i++)
                {
                    tngl.Nodes[i] = Nodes[tngl.Nodes[i].GlobalIndex - 1];
                }
            }
            foreach (var tn in tetrahedrons)
            {
                for (int i = 0; i < tn.Nodes.Count; i++)
                {
                    tn.Nodes[i] = Nodes[tn.Nodes[i].GlobalIndex - 1];
                }
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

                //int idMaterial = Int32.Parse(line[1]);

                List<Node> list = new List<Node>();

                for (int i = 2; i < line.Length; i++)
                {
                    Node node = new Node(0, 0, 0)
                    {
                        GlobalIndex = Int32.Parse(line[i])
                    };
                    list.Add(node);
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

                Node node = new Node(Double.Parse(line[0]),
                    Double.Parse(line[1]),
                    Double.Parse(line[2]))
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

                for (int i = 5; i < line.Length - 3; i++)
                {
                    Node node = new Node(0, 0, 0)
                    {
                        GlobalIndex = Int32.Parse(line[i])
                    };
                    list.Add(node);
                }

                Triangles.Add(new Triangle(list));
            }
        }
    }
}
