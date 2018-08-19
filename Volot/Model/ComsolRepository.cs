using MeshGenerator.Elements;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Volot.Model
{
    public class ComsolRepository<ID> : IRepository<ID, List<Tetrahedron>>
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
            using (StreamReader reader = new StreamReader($"Comsol/{id}.txt"))
            {
                while (!reader.ReadLine().Equals("% Coordinates")) ;
                ReadNodes(reader);
                tetrahedrons.AddRange(ReadElements(reader));
            }

            foreach (var tn in tetrahedrons)
            {
                for (int i = 0; i < tn.Nodes.Count; i++)
                {
                    tn.Nodes[i] = Nodes[tn.Nodes[i].GlobalIndex];
                }
            }
            return tetrahedrons;
        }

        void ReadNodes(StreamReader reader)
        {
            string currentLine = "";
            while (!(currentLine = reader.ReadLine()).Equals("% Elements (tetrahedral)"))
            {
                string[] line = currentLine.Split(' ')
                    .Where(x => x.CompareTo("") != 0)
                    .Select(s => s.Replace('.', ','))
                    .ToArray();

                //Node node = new Node((int)Math.Round(Double.Parse(line[1])),
                //    (int)Math.Round(Double.Parse(line[2])),
                //    (int)Math.Round(Double.Parse(line[3])))
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

        List<Tetrahedron> ReadElements(StreamReader reader)
        {
            List<Tetrahedron> tetrahedrons = new List<Tetrahedron>();

            string currentLine = "";
            while (!(currentLine = reader.ReadLine()).Split(' ')[1].Equals("Data"))
            {
                string[] line = currentLine.Split(' ').Where(x => x.CompareTo("") != 0).ToArray();
                
                tetrahedrons.Add(ReadTetrahedron(line));
            }

            return tetrahedrons;
        }

        Tetrahedron ReadTetrahedron(string[] line)
        {
            List<Node> list = new List<Node>();

            for (int i = 0; i < line.Length; i++)
            {
                Node node = new Node(0, 0, 0)
                {
                    GlobalIndex = Int32.Parse(line[i]) - 1
                };
                list.Add(node);
            }

            return new Tetrahedron(list);
        }
    }
}
