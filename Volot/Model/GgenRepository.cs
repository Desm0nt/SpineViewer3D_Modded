using MeshGenerator.Elements;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Volot.Model
{
    public class GgenRepository<ID> : IRepository<ID, List<Tetrahedron>>
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
            using (StreamReader reader = new StreamReader($"Ggen/{id}.msh"))
            {
                while (!reader.ReadLine().Equals("$Nodes")) ;
                ReadNodes(reader);
                while (!reader.ReadLine().Equals("$Elements")) ;
                tetrahedrons.AddRange(ReadElements(reader));
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

        void ReadNodes(StreamReader reader)
        {
            reader.ReadLine();

            string currentLine = "";
            while (!(currentLine = reader.ReadLine()).Equals("$EndNodes"))
            {
                string[] line = currentLine.Split(' ')
                    .Where(x => x.CompareTo("") != 0)
                    .Select(s => s.Replace('.', ','))
                    .ToArray();

                //Node node = new Node((int)Math.Round(Double.Parse(line[1])),
                //    (int)Math.Round(Double.Parse(line[2])),
                //    (int)Math.Round(Double.Parse(line[3])))
                Node node = new Node(Double.Parse(line[1]) / 10,
                    Double.Parse(line[3]) / 10,
                    Double.Parse(line[2]) / 10)
                //Node node = new Node(Double.Parse(line[1]),
                //    Double.Parse(line[2]),
                //    Double.Parse(line[3]))
                {
                    IdMaterial = ID_MATERIAL,
                    GlobalIndex = Nodes.Count
                };

                Nodes.Add(node.GlobalIndex, node);
            }
        }

        List<Tetrahedron> ReadElements(StreamReader reader)
        {
            reader.ReadLine();

            List<Tetrahedron> tetrahedrons = new List<Tetrahedron>();

            string currentLine = "";
            while (!(currentLine = reader.ReadLine()).Equals("$EndElements"))
            {
                string[] line = currentLine.Split(' ').Where(x => x.CompareTo("") != 0).ToArray();

                if (line[1].Equals("2"))
                {
                    ReadTriangle(line);
                }
                if (line[1].Equals("4"))
                {
                    tetrahedrons.Add(ReadTetrahedron(line));
                }
            }

            return tetrahedrons;
        }

        Tetrahedron ReadTetrahedron(string[] line)
        {
            List<Node> list = new List<Node>();

            for (int i = 5; i < line.Length; i++)
            {
                Node node = new Node(0, 0, 0)
                {
                    GlobalIndex = Int32.Parse(line[i])
                };
                list.Add(node);
            }

            return new Tetrahedron(list);
        }

        void ReadTriangle(string[] line)
        {
            List<Node> list = new List<Node>();

            for (int i = 5; i < line.Length; i++)
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
