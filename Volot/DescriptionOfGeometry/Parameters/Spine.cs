using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Volot.DescriptionOfGeometry.STL;

namespace Volot.DescriptionOfGeometry.Parameters
{
    public class Spine
    {
        public string Key { get; } //обозначение позвонка L1-L5
        public List<Vertex> Points { get; } //Список точек из XML, задающих геометрию позвонка (4 точки)
        public List<Geometry> Geometries { get; } //Список геометрических параметров из XML
        public StlMesh Model { get; } //Модель позвонка в виде STL
        public StlMesh Model2 { get; } //Модель позвонка в виде STL

        public struct Pointes
        {
            public double X { get; set; }
            public double Y { get; set; }
            public double Z { get; set; }
        }

        /*Индексы ключ. точек в  массиве Model: RightDown-RightUp-LeftUp-LeftDown, соответствуют точкам Points*/
        private int _k0 = 1030, _k1 = 985, _k2 = 873, _k3 = 962;
        private int _kk0 = 10, _kk1 = 95, _kk2 = 73, _kk3 = 62;


        public List<int> U { get; private set; }    //Индексы точек из массива Model, задающие верхнюю плоскость тела позвонка 
        public List<int> D { get; private set; }    //Индексы точек из массива Model, задающие нижнюю плоскость тела позвонка
        public List<int> UC { get; private set; }    //Индексы точек из массива Model, задающие верхнюю плоскость тела позвонка 
        public List<int> DC { get; private set; }    //Индексы точек из массива Model, задающие нижнюю плоскость тела позвонка
        //public List<int> U = new List<int>();    //Индексы точек из массива Model, задающие верхнюю плоскость тела позвонка 
        //public List<int> D = new List<int>();    //Индексы точек из массива Model, задающие нижнюю плоскость тела позвонка
        public List<Pointes> Up;
        public List<Pointes> Down;
        public List<Pointes> UpC;
        public List<Pointes> DownC;

        public Spine()
        {
            Key = "";
            Points = new List<Vertex>();
            Geometries = new List<Geometry>();
        }

        public Spine(string key)
        {
            Key = key;
            Points = new List<Vertex>();
            Geometries = new List<Geometry>();
        }

        public Spine(string key, List<Vertex> points, List<Geometry> geometries, int direction)
        {
            Key = key;
            Points = points;
            Geometries = geometries;
            if (Points.Count == 4)
            {
                //Считывание модели из файла
                Model = new StlMesh(StlFile.Load(new FileStream("FullModel.stl", FileMode.Open, FileAccess.Read))
                    .Triangles);

                int k = 0;

                Up = new List<Pointes>();
                Down = new List<Pointes>();
                UpC = new List<Pointes>();
                DownC = new List<Pointes>();
                using (StreamReader sr = new StreamReader("up.txt"))
                {
                    while (sr.Peek() >= 0)
                    {
                        string str;
                        string[] strArray;
                        str = sr.ReadLine();

                        strArray = str.Split(' ');
                        Pointes currentPoint = new Pointes();
                        currentPoint.X = float.Parse(strArray[0], NumberStyles.Float, CultureInfo.InvariantCulture);
                        currentPoint.Y = float.Parse(strArray[1], NumberStyles.Float, CultureInfo.InvariantCulture);
                        currentPoint.Z = float.Parse(strArray[2], NumberStyles.Float, CultureInfo.InvariantCulture);
                        Up.Add(currentPoint);
                    }
                }
                using (StreamReader sr = new StreamReader("down.txt"))
                {
                    while (sr.Peek() >= 0)
                    {
                        string str;
                        string[] strArray;
                        str = sr.ReadLine();

                        strArray = str.Split(' ');
                        Pointes currentPoint = new Pointes();
                        currentPoint.X = float.Parse(strArray[0], NumberStyles.Float, CultureInfo.InvariantCulture);
                        currentPoint.Y = float.Parse(strArray[1], NumberStyles.Float, CultureInfo.InvariantCulture);
                        currentPoint.Z = float.Parse(strArray[2], NumberStyles.Float, CultureInfo.InvariantCulture);
                        Down.Add(currentPoint);
                    }
                }
                using (StreamReader sr = new StreamReader("up_corner.txt"))
                {
                    while (sr.Peek() >= 0)
                    {
                        string str;
                        string[] strArray;
                        str = sr.ReadLine();

                        strArray = str.Split(' ');
                        Pointes currentPoint = new Pointes();
                        currentPoint.X = float.Parse(strArray[0], NumberStyles.Float, CultureInfo.InvariantCulture);
                        currentPoint.Y = float.Parse(strArray[1], NumberStyles.Float, CultureInfo.InvariantCulture);
                        currentPoint.Z = float.Parse(strArray[2], NumberStyles.Float, CultureInfo.InvariantCulture);
                        UpC.Add(currentPoint);
                    }
                }
                using (StreamReader sr = new StreamReader("down_corner.txt"))
                {
                    while (sr.Peek() >= 0)
                    {
                        string str;
                        string[] strArray;
                        str = sr.ReadLine();

                        strArray = str.Split(' ');
                        Pointes currentPoint = new Pointes();
                        currentPoint.X = float.Parse(strArray[0], NumberStyles.Float, CultureInfo.InvariantCulture);
                        currentPoint.Y = float.Parse(strArray[1], NumberStyles.Float, CultureInfo.InvariantCulture);
                        currentPoint.Z = float.Parse(strArray[2], NumberStyles.Float, CultureInfo.InvariantCulture);
                        DownC.Add(currentPoint);
                    }
                }
                U = new List<int>();
                foreach (var a in Up)
                {
                    foreach (var b in Model.Vertices)
                    {
                        if (a.X == b.X && a.Y == b.Y && a.Z == b.Z)
                        {
                            U.Add(b.IdInMesh);
                        }
                    }
                }
                D = new List<int>();
                foreach (var a in Down)
                {
                    foreach (var b in Model.Vertices)
                    {
                        if (a.X == b.X && a.Y == b.Y && a.Z == b.Z)
                        {
                            D.Add(b.IdInMesh);
                        }
                    }
                }
                UC = new List<int>();
                foreach (var a in UpC)
                {
                    foreach (var b in Model.Vertices)
                    {
                        if (a.X == b.X && a.Y == b.Y && a.Z == b.Z)
                        {
                            UC.Add(b.IdInMesh);
                        }
                    }
                }
                DC = new List<int>();
                foreach (var a in DownC)
                {
                    foreach (var b in Model.Vertices)
                    {
                        if (a.X == b.X && a.Y == b.Y && a.Z == b.Z)
                        {
                            DC.Add(b.IdInMesh);
                        }
                    }
                }

                //TODO Здесь добавить маркировку точек

                if (direction == 0)
                {
                    //Отражение
                    Model.Vertices = Transformation.ConvertPointsArrayToList(
                        Transformation.ReflectionObject(Transformation.ConvertPointsListToArray(Model.Vertices), true,
                            false, false));
                }

                //Масштабирование
                var dy = Vertex.D(Points[2], Points[3]) / Vertex.D(Model.Vertices[_k2], Model.Vertices[_k3]);
                var dx = Vertex.D(Points[2], Points[1]) / Vertex.D(Model.Vertices[_k2], Model.Vertices[_k1]);
                Model.Vertices =
                    Transformation.ConvertPointsArrayToList(
                        Transformation.ScaleObject(Transformation.ConvertPointsListToArray(Model.Vertices), dx, dy,
                            dx));

                //Поворот
                double k2 = (Points[0].Y - Points[3].Y) / (Points[0].X - Points[3].X);
                double k1 = (Model.Vertices[_k0].Y - Model.Vertices[_k3].Y) /
                            (Model.Vertices[_k0].X - Model.Vertices[_k3].X);
                double alfa = Math.Atan((k2 - k1) / (1 + k2 * k1));
                Model.Vertices = Transformation.ConvertPointsArrayToList(Transformation.Rotate(alfa,
                    Transformation.ConvertPointsListToArray(Model.Vertices), 3, 0, 0, 0));

                //Перемещение
                Model.Vertices = Transformation.ConvertPointsArrayToList(Transformation.MoveObject(
                    Transformation.ConvertPointsListToArray(Model.Vertices), Points[3].X - Model.Vertices[_k3].X,
                    Points[3].Y - Model.Vertices[_k3].Y, 0));

            }
        }
        public Spine(string key, List<Vertex> points, List<Geometry> geometries, int direction, bool mod2)
        {
            Key = key;
            Points = points;
            Geometries = geometries;
            if (Points.Count == 4)
            {
                //Считывание модели из файла
                Model2 = new StlMesh(StlFile.Load(new FileStream("FullModel.stl", FileMode.Open, FileAccess.Read))
                    .Triangles);
                Model = new StlMesh(StlFile.Load(new FileStream("CutModel.stl", FileMode.Open, FileAccess.Read))
    .Triangles);
                var i1 = Model2.Vertices[_k0];
                var i2 = Model2.Vertices[_k1];
                var i3 = Model2.Vertices[_k2];
                var i4 = Model2.Vertices[_k3];
                foreach (var a in Model.Vertices)
                {
                    if (Math.Round(a.X, 4) == Math.Round(i1.X, 4))
                        _kk0 = a.IdInMesh;
                    if (Math.Round(a.X, 4) == Math.Round(i2.X, 4))
                        _kk1 = a.IdInMesh;
                    if (Math.Round(a.X, 4) == Math.Round(i3.X, 4))
                        _kk2 = a.IdInMesh;
                    if (Math.Round(a.X, 4) == Math.Round(i4.X, 4))
                        _kk3 = a.IdInMesh;
                }

                int k = 0;

                Up = new List<Pointes>();
                Down = new List<Pointes>();
                UpC = new List<Pointes>();
                DownC = new List<Pointes>();
                using (StreamReader sr = new StreamReader("up.txt"))
                {
                    while (sr.Peek() >= 0)
                    {
                        string str;
                        string[] strArray;
                        str = sr.ReadLine();

                        strArray = str.Split(' ');
                        Pointes currentPoint = new Pointes();
                        currentPoint.X = float.Parse(strArray[0], NumberStyles.Float, CultureInfo.InvariantCulture);
                        currentPoint.Y = float.Parse(strArray[1], NumberStyles.Float, CultureInfo.InvariantCulture);
                        currentPoint.Z = float.Parse(strArray[2], NumberStyles.Float, CultureInfo.InvariantCulture);
                        Up.Add(currentPoint);
                    }
                }
                using (StreamReader sr = new StreamReader("down.txt"))
                {
                    while (sr.Peek() >= 0)
                    {
                        string str;
                        string[] strArray;
                        str = sr.ReadLine();

                        strArray = str.Split(' ');
                        Pointes currentPoint = new Pointes();
                        currentPoint.X = float.Parse(strArray[0], NumberStyles.Float, CultureInfo.InvariantCulture);
                        currentPoint.Y = float.Parse(strArray[1], NumberStyles.Float, CultureInfo.InvariantCulture);
                        currentPoint.Z = float.Parse(strArray[2], NumberStyles.Float, CultureInfo.InvariantCulture);
                        Down.Add(currentPoint);
                    }
                }
                using (StreamReader sr = new StreamReader("up_corner.txt"))
                {
                    while (sr.Peek() >= 0)
                    {
                        string str;
                        string[] strArray;
                        str = sr.ReadLine();

                        strArray = str.Split(' ');
                        Pointes currentPoint = new Pointes();
                        currentPoint.X = float.Parse(strArray[0], NumberStyles.Float, CultureInfo.InvariantCulture);
                        currentPoint.Y = float.Parse(strArray[1], NumberStyles.Float, CultureInfo.InvariantCulture);
                        currentPoint.Z = float.Parse(strArray[2], NumberStyles.Float, CultureInfo.InvariantCulture);
                        UpC.Add(currentPoint);
                    }
                }
                using (StreamReader sr = new StreamReader("down_corner.txt"))
                {
                    while (sr.Peek() >= 0)
                    {
                        string str;
                        string[] strArray;
                        str = sr.ReadLine();

                        strArray = str.Split(' ');
                        Pointes currentPoint = new Pointes();
                        currentPoint.X = float.Parse(strArray[0], NumberStyles.Float, CultureInfo.InvariantCulture);
                        currentPoint.Y = float.Parse(strArray[1], NumberStyles.Float, CultureInfo.InvariantCulture);
                        currentPoint.Z = float.Parse(strArray[2], NumberStyles.Float, CultureInfo.InvariantCulture);
                        DownC.Add(currentPoint);
                    }
                }
                U = new List<int>();
                foreach (var a in Up)
                {
                    foreach (var b in Model.Vertices)
                    {
                        if (a.X == b.X && a.Y == b.Y && a.Z == b.Z)
                        {
                            U.Add(b.IdInMesh);
                        }
                    }
                }
                D = new List<int>();
                foreach (var a in Down)
                {
                    foreach (var b in Model.Vertices)
                    {
                        if (a.X == b.X && a.Y == b.Y && a.Z == b.Z)
                        {
                            D.Add(b.IdInMesh);
                        }
                    }
                }
                UC = new List<int>();
                foreach (var a in UpC)
                {
                    foreach (var b in Model.Vertices)
                    {
                        if (a.X == b.X && a.Y == b.Y && a.Z == b.Z)
                        {
                            UC.Add(b.IdInMesh);
                        }
                    }
                }
                DC = new List<int>();
                foreach (var a in DownC)
                {
                    foreach (var b in Model.Vertices)
                    {
                        if (a.X == b.X && a.Y == b.Y && a.Z == b.Z)
                        {
                            DC.Add(b.IdInMesh);
                        }
                    }
                }

                //TODO Здесь добавить маркировку точек

                if (direction == 0)
                {
                    //Отражение
                    Model.Vertices = Transformation.ConvertPointsArrayToList(
                        Transformation.ReflectionObject(Transformation.ConvertPointsListToArray(Model.Vertices), true,
                            false, false));
                }

                //Масштабирование
                var dy = Vertex.D(Points[2], Points[3]) / Vertex.D(Model.Vertices[_kk2], Model.Vertices[_kk3]);
                var dx = Vertex.D(Points[2], Points[1]) / Vertex.D(Model.Vertices[_kk2], Model.Vertices[_kk1]);
                Model.Vertices =
                    Transformation.ConvertPointsArrayToList(
                        Transformation.ScaleObject(Transformation.ConvertPointsListToArray(Model.Vertices), dx, dy,
                            dx));
                //Поворот
                double k2 = (Points[0].Y - Points[3].Y) / (Points[0].X - Points[3].X);
                double k1 = (Model.Vertices[_kk0].Y - Model.Vertices[_kk3].Y) /
                            (Model.Vertices[_kk0].X - Model.Vertices[_kk3].X);
                double alfa = Math.Atan((k2 - k1) / (1 + k2 * k1));
                Model.Vertices = Transformation.ConvertPointsArrayToList(Transformation.Rotate(alfa,
                    Transformation.ConvertPointsListToArray(Model.Vertices), 3, 0, 0, 0));

                //Перемещение
                Model.Vertices = Transformation.ConvertPointsArrayToList(Transformation.MoveObject(
                    Transformation.ConvertPointsListToArray(Model.Vertices), Points[3].X - Model.Vertices[_kk3].X,
                    Points[3].Y - Model.Vertices[_kk3].Y, 0));

            }
        }
    }
}