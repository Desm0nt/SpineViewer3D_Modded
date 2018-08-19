using MeshGenerator.Elements;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Volot.Model
{
    public class XyzRepository<ID> : IRepository<ID, List<Node>>
    {
        public void Create(ID id, List<Node> item)
        {
            throw new NotImplementedException();
        }
        public void Create2(ID id, List<Node> item)
        {
            throw new NotImplementedException();
        }
        public void Delete(ID id)
        {
            throw new NotImplementedException();
        }

        public List<Node> Read(ID id)
        {
            List<Node> nodes = new List<Node>();
            using (StreamReader reader = new StreamReader($"{id}.xyz"))
            {
                nodes = ReadNodes(reader);
            }
            return nodes;
        }

        List<Node> ReadNodes(StreamReader reader)
        {
            List<Node> nodes = new List<Node>();

            string[] lines = reader.ReadToEnd().Split('\r', '\n');

            for (int i = 0; i < lines.Length; i++)
            {
                if (!lines[i].Equals(""))
                {
                    string[] line = lines[i].Split(' ')
                       .Where(x => x.CompareTo("") != 0)
                       .Select(s => s.Replace('.', ','))
                       .ToArray();

                    Node node = new Node((int)Double.Parse(line[0]), (int)Double.Parse(line[1]), (int)Double.Parse(line[2]))
                    {
                        IdMaterial = 1
                    };

                    nodes.Add(node);
                }
            }
            return nodes;
        }
    }
}
