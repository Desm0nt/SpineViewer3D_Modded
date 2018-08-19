using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace Volot
{
    class ObjReader
    {
        /// <summary>
        /// Список точек, составляющих поверхность (треугольник).
        /// </summary>
        public struct Facet
        {
            public int A { get; set; }
            public int B { get; set; }
            public int C { get; set; }
        }
        private List<RVertex> Vertx = new List<RVertex>();
        private List<RColor> Colors = new List<RColor>();
        private List<Facet> Facets = new List<Facet>();

        /// <summary>
        /// Gets or sets the stream reader.
        /// </summary>
        private static StreamReader Reader { get; set; }

        /// <summary>
        /// Загрузка модели из obj-файла.
        /// </summary>
        /// <param name="s">Содержмое obj-файла.</param>
        /// <returns></returns>
        public void LoadObj(string s)
        {
            using (Reader = new StreamReader(s))
            {
                while (!Reader.EndOfStream)
                {
                    var line = Reader.ReadLine();
                    if (line == null)
                    {
                        break;
                    }

                    line = line.Trim();
                    if (line.StartsWith("#") || line.Length == 0)
                    {
                        continue;
                    }

                    SplitLine(line, out string id, out string values);

                    switch (id.ToLower())
                    {
                        // Vertex data
                        case "v": // geometric vertices
                            AddVertex(values);
                            break;
                        case "f": // texture vertices
                            AddFacets(values);
                            break;
                    }
                }
            }
        }

        public List<RVertex> ReadVertex()
        {
            var vrtx = new List<RVertex>();
            foreach (var fct in Facets)
            {
                vrtx.Add(new RVertex { X = Vertx[fct.A - 1].X, Y = Vertx[fct.A - 1].Y, Z = Vertx[fct.A - 1].Z});
                vrtx.Add(new RVertex { X = Vertx[fct.B - 1].X, Y = Vertx[fct.B - 1].Y, Z = Vertx[fct.B - 1].Z});
                vrtx.Add(new RVertex { X = Vertx[fct.C - 1].X, Y = Vertx[fct.C - 1].Y, Z = Vertx[fct.C - 1].Z});
            }
            return vrtx;
        }

        public List<RColor> ReadColors()
        {
            var colrs = new List<RColor>();
            foreach (var fct in Facets)
            {
                colrs.Add(new RColor { R = Colors[fct.A - 1].R, G = Colors[fct.A - 1].G, B = Colors[fct.A - 1].B });
                colrs.Add(new RColor { R = Colors[fct.B - 1].R, G = Colors[fct.B - 1].G, B = Colors[fct.B - 1].B });
                colrs.Add(new RColor { R = Colors[fct.C - 1].R, G = Colors[fct.C - 1].G, B = Colors[fct.C - 1].B });
            }
            return colrs;
        }

        public List<RNormal> ReadNormals()
        {
            var namls = new List<RNormal>();
            foreach (var fct in Facets)
            {
                Vector3D dir = Vector3D.CrossProduct(
                    new Vector3D(Vertx[fct.B - 1].X, Vertx[fct.B - 1].Y, Vertx[fct.B - 1].Z) -
                    new Vector3D(Vertx[fct.A - 1].X, Vertx[fct.A - 1].Y, Vertx[fct.A - 1].Z),
                    new Vector3D(Vertx[fct.C - 1].X, Vertx[fct.C - 1].Y, Vertx[fct.C - 1].Z) -
                    new Vector3D(Vertx[fct.A - 1].X, Vertx[fct.A - 1].Y, Vertx[fct.A - 1].Z));
                namls.Add(new RNormal(dir.X, dir.Y, dir.Z));
                namls.Add(new RNormal(dir.X, dir.Y, dir.Z));
                namls.Add(new RNormal(dir.X, dir.Y, dir.Z));
            }
            return namls;
        }


        /// <summary>
        /// Добавление вершины.
        /// </summary>
        /// <param name="values">
        /// Входны данные.
        /// </param>
        private void AddVertex(string values)
        {
            var fields = values.Split(' ').ToList<string>();
            if (fields.Count > 3)
            {
                Vertx.Add(new RVertex
                {
                    X = Convert.ToDouble(fields[0], CultureInfo.InvariantCulture) / 10,
                    Y = Convert.ToDouble(fields[1], CultureInfo.InvariantCulture) / 10,
                    Z = Convert.ToDouble(fields[2], CultureInfo.InvariantCulture) / 10
                });
                Colors.Add(new RColor
                {
                    R = Convert.ToSingle(fields[3], CultureInfo.InvariantCulture),
                    G = Convert.ToSingle(fields[4], CultureInfo.InvariantCulture),
                    B = Convert.ToSingle(fields[5], CultureInfo.InvariantCulture),
                });

            }
            else
            {
                Vertx.Add(new RVertex(Convert.ToDouble(fields[0], CultureInfo.InvariantCulture) / 10, 
                    Convert.ToDouble(fields[1], CultureInfo.InvariantCulture) / 10, 
                    Convert.ToDouble(fields[2], CultureInfo.InvariantCulture) / 10));
                Colors.Add(new RColor());
            }
        }

        /// <summary>
        /// Добавление списка вершин треугольника.
        /// </summary>
        /// <param name="values">
        /// Входны данные.
        /// </param>
        private void AddFacets(string values)
        {
            var fields = values.Split(' ').ToList<string>();
            Facets.Add(new Facet { A = Convert.ToInt32(fields[0]), B = Convert.ToInt32(fields[1]), C = Convert.ToInt32(fields[2]) });
        }

        private static void SplitLine(string line, out string id, out string values)
        {
            int idx = line.IndexOf(' ');
            if (idx < 0)
            {
                id = line;
                values = null;
                return;
            }

            id = line.Substring(0, idx);
            values = line.Substring(idx + 1);
        }
        
    }
}
