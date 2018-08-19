using System;
using System.Collections.Generic;

namespace Volot.DescriptionOfGeometry
{
    public class Transformation
    {
        private static double[,] Multiply(double[,] a, double[,] b)
        {
            if (a.GetLength(1) != b.GetLength(0))
                throw new ArgumentException("Не совпадают размерности матриц");
            double[,] c = new double[a.GetLength(0), b.GetLength(1)];
            for (int i = 0; i < a.GetLength(0); ++i)
            {
                for (int j = 0; j < b.GetLength(1); ++j)
                {
                    c[i, j] = 0;
                    for (int k = 0; k < a.GetLength(1); ++k)
                    {
                        c[i, j] += a[i, k] * b[k, j];
                    }
                }
            }
            return c;
        }

        /// <summary>
        /// Поворот точек на заданный угол вокруг выбранной оси 1-rX, 2-rY, 3-rX и относительно точки М(x,y,z)
        /// </summary>
        /// <param name="angel">Угол</param>
        /// <param name="points"></param>
        /// <param name="r">1-rX, 2-rY, 3-rX</param>
        /// <param name="x">Координата точки относительно которой происходит вращение</param>
        /// <param name="y">Координата точки относительно которой происходит вращение</param>
        /// <param name="z">Координата точки относительно которой происходит вращение</param>
        /// <returns></returns>
        public static double[,] Rotate(double angel, double[,] points, int r, double x, double y, double z)
        {
            double[,] mypoints = new double[points.GetLength(0), points.GetLength(1)];
            switch (r)
            {
                case 1:
                {
                    double[,] rx =
                    {
                        {1, 0, 0, 0},
                        {0, Math.Cos(angel), Math.Sin(angel), 0},
                        {0, -Math.Sin(angel), Math.Cos(angel), 0},
                        {
                            0, -z * Math.Sin(angel) - y * Math.Cos(angel) + y,
                            -y * Math.Cos(angel) + z * Math.Sin(angel) + z, 1
                        }
                    };
                    mypoints = Multiply(points, rx);
                }
                    break;
                case 2:
                {
                    double[,] ry =
                    {
                        {Math.Cos(angel), 0, Math.Sin(angel), 0},
                        {0, 1, 0, 0},
                        {-Math.Sin(angel), 0, Math.Cos(angel), 0},
                        {
                            -z * Math.Sin(angel) - x * Math.Cos(angel) + x, 0,
                            -x * Math.Cos(angel) + z * Math.Sin(angel) + z, 1
                        }
                    };
                    mypoints = Multiply(points, ry);
                }
                    break;
                case 3:
                {
                    double[,] rz =
                    {
                        {Math.Cos(angel), Math.Sin(angel), 0, 0},
                        {-Math.Sin(angel), Math.Cos(angel), 0, 0},
                        {0, 0, 1, 0},
                        {
                            -x * Math.Cos(angel) + y * Math.Sin(angel) + x,
                            -x * Math.Sin(angel) - y * Math.Cos(angel) + y, 0, 1
                        }
                    };
                    mypoints = Multiply(points, rz);
                }
                    break;
            }
            return mypoints;
        }


        public static double[,] ReflectionObject(double[,] points, bool x, bool y, bool z)
        {
            double[,] step =
            {
                {x ? -1 : 1, 0, 0, 0},
                {0, y ? -1 : 1, 0, 0},
                {0, 0, z ? -1 : 1, 0},
                {0, 0, 0, 1}
            };
            return Multiply(points, step);
        }


        public static double[,] MoveObject(double[,] points, double stepX, double stepY, double stepZ)
        {
            double[,] step =
            {
                {1, 0, 0, 0},
                {0, 1, 0, 0},
                {0, 0, 1, 0},
                {stepX, stepY, stepZ, 1}
            };
            return Multiply(points, step);
        }


        public static double[,] ScaleObject(double[,] points, double scaleX, double scaleY, double scaleZ)
        {
            double[,] scale =
            {
                {scaleX, 0, 0, 0},
                {0, scaleY, 0, 0},
                {0, 0, scaleZ, 0},
                {0, 0, 0, 1}
            };
            return Multiply(points, scale);
        }

        /// <summary>
        /// Пересечение 2х прямых AB и CD в плоскости Oxy
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        public static Vertex Cross(Vertex a, Vertex b, Vertex c, Vertex d) //точки a и b концы первого отрезка  c и d второго
        {
            double x = -((a.X * b.Y - b.X * a.Y) * (d.X - c.X) - (c.X * d.Y - d.X * c.Y) * (b.X - a.X)) /
                  ((a.Y - b.Y) * (d.X - c.X) - (c.Y - d.Y) * (b.X - a.X));
            double y = ((c.Y - d.Y) * (-x) - (c.X * d.Y - d.X * c.Y)) / (d.X - c.X);
            return new Vertex(x, y, 0);
        }

        public static List<Vertex> ConvertPointsArrayToList(double[,] points)
        {
            List<Vertex> p = new List<Vertex>();
            for (int j = 0; j < points.GetLength(0); j++)
            {
                p.Add(new Vertex(points[j, 0], points[j, 1], points[j, 2],j));
            }
            return p;
        }

        public static double[,] ConvertPointsListToArray(List<Vertex> points)
        {
            double[,] obj = new double[points.Count, 4];
            int i = 0;
            foreach (var p in points)
            {
                obj[i, 0] = p.X;
                obj[i, 1] = p.Y;
                obj[i, 2] = p.Z;
                obj[i, 3] = 1;
                i++;
            }
            return obj;
        }
    }
}