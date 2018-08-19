using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace Volot
{
    class CustomStlReader
    {
        double RR, GG, BB;
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
        private static readonly Regex VertexRegex = new Regex(@"vertex\s*(\S*)\s*(\S*)\s*(\S*)\s*(\S*)", RegexOptions.Compiled);

        /// <summary>
        /// Gets or sets the stream reader.
        /// </summary>
        private static StreamReader Reader { get; set; }

        /// <summary>
        /// Загрузка модели из obj-файла.
        /// </summary>
        /// <param name="s">Содержмое obj-файла.</param>
        /// <returns></returns>
        public void LoadStl(string s)
        {
            string path = System.IO.Path.GetDirectoryName(s);
            string sMax = System.IO.File.ReadAllText(path + @"\test.color");
            double Max = double.Parse(sMax);
            double OnePercent = Max / 100;
            using (Reader = new StreamReader(s))
            {
                int i = 0;
                int j = 0;
                while (!Reader.EndOfStream)
                {
                    var line = Reader.ReadLine();
                    if (line == null)
                    {
                        break;
                    }
                    var match = VertexRegex.Match(line);
                    if (match.Success)
                    {
                        double x = double.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
                        double y = double.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture);
                        double z = double.Parse(match.Groups[3].Value, CultureInfo.InvariantCulture);
                        double col = double.Parse(match.Groups[4].Value);
                        AddVertex(x, y, z);
                        double colorvalue = ((col / OnePercent) / 100) * 240;
                        double fls = 1;
                        HSV hsv = new HSV(240-colorvalue, fls, fls);
                        RGB rgb = HSVToRGB(hsv);
                        RR = rgb.R / 255;
                        GG = rgb.G / 255;
                        BB = rgb.B / 255;
                        AddColor(RR, GG, BB);
                        if (j % 3 == 0)
                        {
                            int a = i;
                            i++;
                            int b = i;
                            i++;
                            int c = i;
                            i++;
                            AddFacets(a, b, c);
                        }

                        j++;
                    }
                }
            }
        }

        public List<RVertex> ReadVertex()
        {
            var vrtx = new List<RVertex>();
            foreach (var fct in Facets)
            {
                vrtx.Add(new RVertex { X = Vertx[fct.A].X, Y = Vertx[fct.A].Y, Z = Vertx[fct.A].Z});
                vrtx.Add(new RVertex { X = Vertx[fct.B].X, Y = Vertx[fct.B].Y, Z = Vertx[fct.B].Z});
                vrtx.Add(new RVertex { X = Vertx[fct.C].X, Y = Vertx[fct.C].Y, Z = Vertx[fct.C].Z});
            }
            return vrtx;
        }

        public List<RColor> ReadColors()
        {
            var colrs = new List<RColor>();
            foreach (var fct in Facets)
            {
                colrs.Add(new RColor { R = Colors[fct.A].R, G = Colors[fct.A].G, B = Colors[fct.A].B });
                colrs.Add(new RColor { R = Colors[fct.B].R, G = Colors[fct.B].G, B = Colors[fct.B].B });
                colrs.Add(new RColor { R = Colors[fct.C].R, G = Colors[fct.C].G, B = Colors[fct.C].B });
            }
            return colrs;
        }

        public List<RNormal> ReadNormals()
        {
            var namls = new List<RNormal>();
            foreach (var fct in Facets)
            {
                Vector3D dir = Vector3D.CrossProduct(
                    new Vector3D(Vertx[fct.B].X, Vertx[fct.B].Y, Vertx[fct.B].Z) -
                    new Vector3D(Vertx[fct.A].X, Vertx[fct.A].Y, Vertx[fct.A].Z),
                    new Vector3D(Vertx[fct.C].X, Vertx[fct.C].Y, Vertx[fct.C].Z) -
                    new Vector3D(Vertx[fct.A].X, Vertx[fct.A].Y, Vertx[fct.A].Z));
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
        private void AddVertex(double x, double y, double z)
        {
                Vertx.Add(new RVertex
                {
                    X = Convert.ToDouble(x, CultureInfo.InvariantCulture) / 100,
                    Z = Convert.ToDouble(y, CultureInfo.InvariantCulture) / 100,
                    Y = Convert.ToDouble(z, CultureInfo.InvariantCulture) / 100
                });
                //Colors.Add(new RColor
                //{
                //    R = Convert.ToSingle(fields[3], CultureInfo.InvariantCulture),
                //    G = Convert.ToSingle(fields[4], CultureInfo.InvariantCulture),
                //    B = Convert.ToSingle(fields[5], CultureInfo.InvariantCulture),
                //});
        }

        private void AddColor(double r, double g, double b)
        {
            Colors.Add(new RColor
            {
                R = Convert.ToSingle(r, CultureInfo.InvariantCulture),
                G = Convert.ToSingle(g, CultureInfo.InvariantCulture),
                B = Convert.ToSingle(b, CultureInfo.InvariantCulture)
            });
        }

        /// <summary>
        /// Добавление списка вершин треугольника.
        /// </summary>
        /// <param name="values">
        /// Входны данные.
        /// </param>
        private void AddFacets(int a, int b, int c)
        {
            Facets.Add(new Facet { A = Convert.ToInt32(a), B = Convert.ToInt32(b), C = Convert.ToInt32(c) });
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

        public struct RGB
        {
            private double _r;
            private double _g;
            private double _b;

            public RGB(double r, double g, double b)
            {
                this._r = r;
                this._g = g;
                this._b = b;
            }

            public double R
            {
                get { return this._r; }
                set { this._r = value; }
            }

            public double G
            {
                get { return this._g; }
                set { this._g = value; }
            }

            public double B
            {
                get { return this._b; }
                set { this._b = value; }
            }

            public bool Equals(RGB rgb)
            {
                return (this.R == rgb.R) && (this.G == rgb.G) && (this.B == rgb.B);
            }
        }

        public struct HSV
        {
            private double _h;
            private double _s;
            private double _v;

            public HSV(double h, double s, double v)
            {
                this._h = h;
                this._s = s;
                this._v = v;
            }

            public double H
            {
                get { return this._h; }
                set { this._h = value; }
            }

            public double S
            {
                get { return this._s; }
                set { this._s = value; }
            }

            public double V
            {
                get { return this._v; }
                set { this._v = value; }
            }

            public bool Equals(HSV hsv)
            {
                return (this.H == hsv.H) && (this.S == hsv.S) && (this.V == hsv.V);
            }
        }

        public static RGB HSVToRGB(HSV hsv)
        {
            double r = 0, g = 0, b = 0;

            if (hsv.S == 0)
            {
                r = hsv.V;
                g = hsv.V;
                b = hsv.V;
            }
            else
            {
                int i;
                double f, p, q, t;

                if (hsv.H == 360)
                    hsv.H = 0;
                else
                    hsv.H = hsv.H / 60;

                i = (int)Math.Truncate(hsv.H);
                f = hsv.H - i;

                p = hsv.V * (1.0 - hsv.S);
                q = hsv.V * (1.0 - (hsv.S * f));
                t = hsv.V * (1.0 - (hsv.S * (1.0 - f)));

                switch (i)
                {
                    case 0:
                        r = hsv.V;
                        g = t;
                        b = p;
                        break;

                    case 1:
                        r = q;
                        g = hsv.V;
                        b = p;
                        break;

                    case 2:
                        r = p;
                        g = hsv.V;
                        b = t;
                        break;

                    case 3:
                        r = p;
                        g = q;
                        b = hsv.V;
                        break;

                    case 4:
                        r = t;
                        g = p;
                        b = hsv.V;
                        break;

                    default:
                        r = hsv.V;
                        g = p;
                        b = q;
                        break;
                }

            }

            return new RGB((double)(r * 255), (double)(g * 255), (double)(b * 255));
        }

    }
}
