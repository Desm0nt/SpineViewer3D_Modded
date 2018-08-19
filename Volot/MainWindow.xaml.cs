using Dicom;
using Microsoft.Win32;
using SpineLib;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Input;
using System.Collections.Generic;
using SpineLib.Geometry.Descriptions;
using System.Windows.Shapes;
using SpineLib.Geometry;
using System.Globalization;
using SpineLib.DB;
using System.Drawing;
using System.Windows.Controls.Ribbon;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Windows.Media.Media3D;
using Volot.DescriptionOfGeometry.Parameters;
using HelixToolkit.Wpf;
using System.Xml.Linq;
using Volot.DescriptionOfGeometry.STL;
using MathNet.Numerics.LinearAlgebra.Double;
using MeshGenerator.Elements;
using MeshGenerator.Model;
using MeshGenerator.Modelling.Conditions;
using MeshGenerator.Modelling.Loads;
using MeshGenerator.Modelling.Solutions;
using MeshGenerator.Modelling.Solvers;
using MeshGenerator.Planes;
using MeshGenerator.Scene;
using MeshGenerator.Utils;
using Volot.Model;
using System.Threading;
using System.Threading.Tasks;

namespace Volot
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int METERS_TO_MILIMETERS = 1000;

        FeModel model;
        ISolution solution;
        //ISolve<DictionaryMatrix> solver;
        ISolve<SparseMatrix> solver;
        IRepository<string, List<Tetrahedron>> repository;
        IRepository<string, List<MeshGenerator.Elements.Triangle>> trnglRepository;

        ILoad load;
        IBoundaryCondition conditions;

        private bool rightMouseButtonDown = false;
        private bool leftMouseButtonDown = false;
        private bool middleMouseButtonDown = false;
        private System.Windows.Point startPoint;

        private short imageWidth = 2000;
        private short imageCenter = -1000;

        private short default_imageWidth = 2000;
        private short default_imageCenter = -1000;

        private short[,] raw = null;

        private TransformGroup imageTransform = new TransformGroup();

        private InstrumentWindow instrumentWindow;

        private bool needClose = false;

        private DicomFile currentFile;      //current opened dicom file
        private string image_hash = "";
        private string image_spine_file = null;
        private SpineStorage spines_storage = null; // image storage

        private bool needSave = false;


        private Dictionary<string, SpineDescription> spines_points;
        private Dictionary<Tuple<int, int, int>, System.Windows.Shapes.Rectangle> spine_points_rectangles;

        private Dictionary<string, SpinousProcessDescription> process_points;
        private Dictionary<Tuple<int, int, int>, System.Windows.Shapes.Rectangle> process_points_rectangles;

        private Dictionary<Tuple<Tuple<int, int, int>, Tuple<int, int, int>>, System.Windows.Shapes.Line> drawn_spine_lines;
        private Dictionary<Tuple<Tuple<int, int, int>, Tuple<int, int, int>>, System.Windows.Shapes.Line> drawn_process_lines;

        private FrameworkElement currentMovingRect = null;

        private Dictionary<Tuple<int, int, int>, Label> markers_rectangles;
        private Dictionary<Tuple<int, int, int>, System.Windows.Point> marker_points;

        private int rotatingAngle = 0;
        
        private StorageIO storageIO;
        static internal string dataSource = "Data Source = db.sqlite; Version=3;";

        private ContextMenu PatientMenu;
        private ContextMenu StudyMenu;

        private Bitmap pixels;
        private short[,] newpix;

        private Tuple<Tuple<int, int, int>, Tuple<int, int, int>> markerLine = null;

        Process process = new Process();
        public string tmphash;
        private BitmapImage tmpbmp;

        Model3DGroup demo = new Model3DGroup();
        GeometryModel3D[] mpath = new GeometryModel3D[100];
        double[] mx = new double[1000];
        double[] my = new double[1000];
        double[] mz = new double[1000];
        double[] sx = new double[1000];
        double[] sy = new double[1000];
        double[] sz = new double[1000];
        double[] rx = new double[1000];
        double[] ry = new double[1000];
        double[] rz = new double[1000];
        double[] cenX = new double[1000];
        double[] cenY = new double[1000];
        double[] cenZ = new double[1000];
        Rect3D bounds2;
        Material[] basematerial = new Material[100];
        Material buffermaterial;
        int helix_i = 0;
        int t = 0;
        bool use_fl = false;
        bool select_fl = false;
        public Model3D our_Model { get; set; }
        string type;
        string outfl;
        public Parameter LoadParams { get; set; }


        private const int DEGREES_OF_FREEDOM = 3;
        private const int VERTEBRA_MATERIAL_ID = 2;
        private const int INNERVERTEBRAL_DISK_MATERIAL_ID = 3;
        private const double STEP_WIDTH = 23.75; // step of the tetraherdral model by width
        private const double STEP_HEIGHT = 40; // step of the tetraherdral model by height

        //IRepository<string, List<Tetrahedron>> repository;
        //IRepository<string, List<Triangle>> trnglRepository;
        IRepository<string, List<Tetrahedron>> tetrahedralRepository;

        double forceValue = 1;

        MemoryStream ms = new MemoryStream();

        public MainWindow()
        {
            InitializeComponent();
            this.our_Model = demo;
            overall_grid.DataContext = this;
            type = "LoadModel";
            imageTransform.Children.Add(new ScaleTransform(1.0, 1.0));
            imageTransform.Children.Add(new RotateTransform(0));
            myCanvas.LayoutTransform = imageTransform;
            OpenInstrumentWindow(0);
            storageIO = new StorageIO(dataSource);
            storageIO.InitDB();

            CloseFileImmediately();

            CheckNeedSave();
        }

        public void OpenInstrumentWindow(int direction)
        {

            var x = 200.0;
            var y = 200.0;

            if (instrumentWindow != null)
            {
                instrumentWindow.Hide();
                instrumentWindow.Closing -= InstrumentWindow_Closing;
                x = instrumentWindow.Left;
                y = instrumentWindow.Top;
            }

            instrumentWindow = new InstrumentWindow(direction);
            instrumentWindow.Left = x;
            instrumentWindow.Top = y;
            instrumentWindow.WindowStyle = WindowStyle.ToolWindow;
            instrumentWindow.Show();
            instrumentWindow.Activate();
            instrumentWindow.Closing += InstrumentWindow_Closing;
        }

        private void InstrumentWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!needClose)
            {
                (sender as Window).Hide();
                e.Cancel = true;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitMenus();
            RefreshTree();
        }

        private void InitMenus()
        {
            PatientMenu = new ContextMenu();

            var DeletePatientMenuItem = new MenuItem();
            DeletePatientMenuItem.Header = "Удалить пациента";
            DeletePatientMenuItem.Click += DeletePatientMenuItem_Click; ;

            PatientMenu.Items.Add(DeletePatientMenuItem);

            StudyMenu = new ContextMenu();

            var ShowDiffersMenuItem = new MenuItem();
            ShowDiffersMenuItem.Header = "Сравнить снимки";
            ShowDiffersMenuItem.Click += ShowDiffersMenuItem_Click;

            StudyMenu.Items.Add(ShowDiffersMenuItem);

            var DeleteStudyMenuItem = new MenuItem();
            DeleteStudyMenuItem.Header = "Удалить исследование";
            DeleteStudyMenuItem.Click += DeleteStudyMenuItem_Click;

            StudyMenu.Items.Add(DeleteStudyMenuItem);
        }

        private void DeleteStudyMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var item = ImagesListTreeView.SelectedItem as TreeViewItem;
            var tag = item.Tag as Tuple<int, int>;

            var images = storageIO.DeleteStudyComplete(storageIO.GetStudy(tag.Item2));

            if (images.Contains(image_hash))
            {
                CloseFile();
            }

            foreach (var hash in images)
            {
                var path = System.IO.Path.Combine("images", hash);
                if (Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                }
            }
            RefreshTree();
        }

        private void TreeViewItem_MouseRightButtonDown(object sender, MouseEventArgs e)
        {
            var item = sender as TreeViewItem;
            if (item != null)
            {
                item.Focus();
                e.Handled = true;
            }
        }

        private void DeletePatientMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var item = ImagesListTreeView.SelectedItem as TreeViewItem;

            //if (ite)

            var tag = item.Tag as Tuple<int, int>;

            var images = storageIO.DeletePatientComplete(storageIO.GetPatient(tag.Item2));

            if (images.Contains(image_hash))
            {
                CloseFile();
            }

            foreach (var hash in images)
            {
                var path = System.IO.Path.Combine("images", hash);
                if (Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                }
            }
            RefreshTree();
        }

        private void ShowDiffersMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var item = ImagesListTreeView.SelectedItem as TreeViewItem;
            var tag = item.Tag as Tuple<int, int>;
            var items = item.Items;

            var patientItem = item.Parent as TreeViewItem;
            var patientTag = patientItem.Tag as Tuple<int, int>;

            var item1 = items[0] as TreeViewItem;
            var item2 = items[1] as TreeViewItem;

            var imgtag1 = item1.Tag as Tuple<int, int, string>;
            var imgtag2 = item2.Tag as Tuple<int, int, string>;

            var patient = storageIO.GetPatient(patientTag.Item2);
            var study = storageIO.GetStudy(tag.Item2);

            var img1 = storageIO.GetImage(imgtag1.Item2);
            var img2 = storageIO.GetImage(imgtag2.Item2);

            var image_spine_file = System.IO.Path.Combine("images", imgtag1.Item3, "spines.xml");
            SpineStorage storage1 = SpineReader.ReadFromFile(image_spine_file);
            image_spine_file = System.IO.Path.Combine("images", imgtag2.Item3, "spines.xml");
            SpineStorage storage2 = SpineReader.ReadFromFile(image_spine_file);

            ParametersResultWindow window = new ParametersResultWindow(storage1, img1, storage2, img2, study.Date, patient);
            window.ShowDialog();

        }

        private void AddImageMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var item = ImagesListTreeView.SelectedItem as TreeViewItem;
            var tag = item.Tag as Tuple<int, int>;
            var dialog = new OpenFileDialog();

            if (dialog.ShowDialog() == true)
            {
                var filename = dialog.FileName;

                try
                {
                    var dcmfile = DicomFile.Open(filename);
                    var hash = "";
                    using (var md5 = MD5.Create())
                    {
                        using (var stream = File.OpenRead(filename))
                        {
                            hash = Utils.RemoveCharFromStr(BitConverter.ToString(md5.ComputeHash(stream)), '-').ToLower();

                            if (!Directory.Exists(System.IO.Path.Combine("images", hash)))
                            {
                                var dir = Directory.CreateDirectory(System.IO.Path.Combine("images", hash));
                                File.Copy(filename, System.IO.Path.Combine(dir.FullName, "image.dcm"));
                            }

                        }
                    }

                    var i = new SpineLib.DB.Image();
                    i.Hash = hash;
                    i.StudyID = tag.Item2;
                    storageIO.InsertImage(i);

                    var spine_file = System.IO.Path.Combine("images", hash, "spines.xml");
                    var sp_st = SpineStorage.GenerateStorageForAddedFile(dcmfile);
                    SpineReader.WriteToFile(sp_st, spine_file);


                    RefreshTree();
                }
                catch (Exception)
                {
                    MessageBox.Show("Данный файл не является DICOM-изображением", "Ошибка");
                    return;
                }


            }
        }


        private void RefreshTree()
        {

            ImagesListTreeView.Items.Clear();
            var pats = storageIO.GetPatients();

            foreach (var patient in pats)
            {
                TreeViewItem item = new TreeViewItem();
                item.Tag = new Tuple<int, int>(0, patient.ID);
                item.Header = patient.Name + " " + patient.Patronymic + " " + patient.Surname;
                item.ContextMenu = PatientMenu;

                var studies = storageIO.GetStudiesByPatient(patient);
                foreach (var study in studies)
                {
                    TreeViewItem item1 = new TreeViewItem();
                    item1.Tag = new Tuple<int, int>(1, study.ID);
                    item1.Header = study.Date.ToShortDateString();
                    item1.ContextMenu = StudyMenu;

                    var images = storageIO.GetImagesByStudy(study);
                    foreach (var image in images)
                    {
                        TreeViewItem item2 = new TreeViewItem();
                        item2.Tag = new Tuple<int, int, string>(2, image.ID, image.Hash);
                        switch (image.State)
                        {
                            case 0:
                                item2.Header = "Лежа";
                                break;
                            case 1:
                                item2.Header = "Стоя";
                                break;
                        }
                        item2.MouseDoubleClick += ImagesTreeViewItemClick;
                        item1.Items.Add(item2);
                    }

                    item.Items.Add(item1);
                }


                ImagesListTreeView.Items.Add(item);

            }

        }

        private void ImagesTreeViewItemClick(object sender, MouseButtonEventArgs e)
        {

            CloseFile();

            var node = e.Source as TreeViewItem;
            image_hash = (node.Tag as Tuple<int, int, string>).Item3;
            tmphash = image_hash;

            if (image_hash != null)
            {
                var filename = System.IO.Path.Combine("images", image_hash, "image.dcm");
                currentFile = DicomFile.Open(filename);

                var image_param = DicomUtils.GetWindowParameters(currentFile);

                default_imageWidth = imageWidth = image_param.Item1;
                default_imageCenter = imageCenter = image_param.Item2;



                foreach (var item in SpineConstants.SpineNames)
                {
                    spines_points[item] = new SpineDescription();
                }

                int count = SpineConstants.SpineNames.Count;
                for (int i = 0; i < count - 1; i++)
                {
                    process_points[SpineConstants.InterSpineNames[i]] = new SpinousProcessDescription();
                }


                raw = DicomUtils.ExtractRawValues(currentFile);

                LoadValues(image_hash);

                RefreshImage();


                needSave = false;
                CheckNeedSave();

            }

        }

        private void LoadValues(string hash)
        {
            if (currentFile != null)
            {
                if (spines_storage != null)
                {
                    SpineReader.WriteToFile(spines_storage, image_spine_file);
                }
            }

            if (hash != null)
            {
                image_spine_file = System.IO.Path.Combine("images", hash, "spines.xml");
                image_hash = hash;

                spines_storage = SpineReader.ReadFromFile(image_spine_file);

                rotatingAngle = spines_storage.GetRotatingAngle();

                //(imageTransform.Children[1] as RotateTransform).Angle = rotatingAngle;


                OpenInstrumentWindow(spines_storage.imageDirection);

                RefreshData(spines_storage);
            }
        }

        private void RefreshData(SpineStorage storage)
        {
            if (storage.MarkerLine != null) {
                markerLine = storage.MarkerLine;
            }

            RefreshPointsDescription(spines_storage);
            RefreshProcessPointsDescription(spines_storage);

            foreach (var name in spines_storage.Keys)
            {
                AddPointsRectangles(name, spines_storage.GetDescription(name));
            }

            foreach (var name in spines_storage.SpinousProcessKeys)
            {
                AddProcessRectangles(name, spines_storage.GetSpinousProcessDescription(name));
            }         

            AddMarkersRectangles(spines_storage);

            RefreshMarkerLine();
        }
        
        private void AddMarkersRectangles(SpineStorage storage)
        {
            var p0 = new Tuple<int, int, int>(2, 0, 0);
            var p1 = new Tuple<int, int, int>(2, 0, 1);
            var p2 = new Tuple<int, int, int>(2, 0, 2);
            var p3 = new Tuple<int, int, int>(2, 0, 3);

            if (storage.GetMarkersCount() != 0)
            {
                var point0 = storage.GetMarkerPoint(0);
                marker_points[p0] = new System.Windows.Point(point0.X, point0.Y);

                var point1 = storage.GetMarkerPoint(1);
                marker_points[p1] = new System.Windows.Point(point1.X, point1.Y);

                var point2 = storage.GetMarkerPoint(2);
                marker_points[p2] = new System.Windows.Point(point2.X, point2.Y);

                var point3 = storage.GetMarkerPoint(3);
                marker_points[p3] = new System.Windows.Point(point3.X, point3.Y);


                var label = new Label();
                label.FontSize = 18;
                label.Foreground = System.Windows.Media.Brushes.Red;
                label.Content = "Г";
                Canvas.SetZIndex(label, 2);
                Canvas.SetLeft(label, marker_points[p0].X - 5);
                Canvas.SetTop(label, marker_points[p0].Y - 5);
                label.Tag = p0;
                label.MouseDown += Rect_MouseDown;
                label.MouseUp += Rect_MouseUp;
                markers_rectangles[p0] = label;
                myCanvas.Children.Add(label);

                label = new Label();
                label.FontSize = 18;
                label.Foreground = System.Windows.Media.Brushes.Red;
                label.Content = "Н";
                Canvas.SetZIndex(label, 2);
                Canvas.SetLeft(label, marker_points[p1].X - 5);
                Canvas.SetTop(label, marker_points[p1].Y - 5);
                label.Tag = p1;
                label.MouseDown += Rect_MouseDown;
                label.MouseUp += Rect_MouseUp;
                markers_rectangles[p1] = label;
                myCanvas.Children.Add(label);

                label = new Label();
                label.FontSize = 18;
                if (spines_storage.imageDirection == 0)
                {
                    label.Content = "С";
                }
                else
                {
                    label.Content = "Ж";
                }
                Canvas.SetZIndex(label, 2);
                label.Foreground = System.Windows.Media.Brushes.Red;
                Canvas.SetLeft(label, marker_points[p2].X - 5);
                Canvas.SetTop(label, marker_points[p2].Y - 5);
                label.Tag = p2;
                label.MouseDown += Rect_MouseDown;
                label.MouseUp += Rect_MouseUp;
                markers_rectangles[p2] = label;
                myCanvas.Children.Add(label);


                label = new Label();
                label.FontSize = 18;
                label.Foreground = System.Windows.Media.Brushes.Red;
                if (spines_storage.imageDirection == 0)
                {
                    label.Content = "НС";
                }
                else
                {
                    label.Content = "С";
                }
                Canvas.SetZIndex(label, 2);
                Canvas.SetLeft(label, marker_points[p3].X - 5);
                Canvas.SetTop(label, marker_points[p3].Y - 5);
                label.Tag = p3;
                label.MouseDown += Rect_MouseDown;
                label.MouseUp += Rect_MouseUp;
                markers_rectangles[p3] = label;
                myCanvas.Children.Add(label);
            }

        }

        private void Rect_MouseUp(object sender, MouseButtonEventArgs e)
        {
            currentMovingRect = sender as FrameworkElement;
            Tuple<int, int, int> tag = currentMovingRect.Tag as Tuple<int, int, int>;
            needSave = true;
            CheckNeedSave();
            currentMovingRect = null;
        }

        private void Rect_MouseDown(object sender, MouseButtonEventArgs e)
        {
            currentMovingRect = sender as FrameworkElement;
            Tuple<int, int, int> tag = currentMovingRect.Tag as Tuple<int, int, int>;
            SelectValuesInInstrumentWindow(tag);
        }

        private void SelectValuesInInstrumentWindow(Tuple<int, int, int> tag)
        {
            if (tag.Item1 == 2 || tag.Item1 == 3)
            {
                instrumentWindow.TypeComboBox.SelectedIndex = tag.Item1;
                instrumentWindow.PointComboBox.SelectedIndex = tag.Item3;
            }
            else
            {
                instrumentWindow.TypeComboBox.SelectedIndex = tag.Item1;
                instrumentWindow.NameComboBox.SelectedIndex = tag.Item2;
                instrumentWindow.PointComboBox.SelectedIndex = tag.Item3;
            }
        }

        private void RefreshPointsDescription(SpineStorage spines_storage)
        {
            spines_points.Clear();
            var keys = spines_storage.Keys;
            foreach (var name in SpineConstants.SpineNames)
            {
                if (keys.Contains(name))
                {
                    var val = spines_storage.GetDescription(name);
                    spines_points[name] = val;

                }
            }
        }

        private void AddPointsRectangles(string spine_name, SpineDescription description)
        {

            int type = 0;
            int name = SpineConstants.SpineNames.IndexOf(spine_name);

            int point = 0;
            System.Windows.Shapes.Rectangle rect = new System.Windows.Shapes.Rectangle();
            rect.Width = 5;
            rect.Height = 5;
            rect.Fill = new SolidColorBrush(Colors.Blue);
            rect.Tag = new Tuple<int, int, int>(type, name, point);
            rect.MouseDown += Rect_MouseDown;
            rect.MouseUp += Rect_MouseUp;
            myCanvas.Children.Add(rect);
            Canvas.SetZIndex(rect, 2);
            Canvas.SetLeft(rect, description.UpLeft.X - 3);
            Canvas.SetTop(rect, description.UpLeft.Y - 3);
            var upleft_point = new Tuple<int, int, int>(type, name, point);
            spine_points_rectangles[upleft_point] = rect;

            point = 1;
            rect = new System.Windows.Shapes.Rectangle();
            rect.Width = 5;
            rect.Height = 5;
            rect.Fill = new SolidColorBrush(Colors.Red);
            rect.Tag = new Tuple<int, int, int>(type, name, point);
            rect.MouseDown += Rect_MouseDown;
            rect.MouseUp += Rect_MouseUp;
            myCanvas.Children.Add(rect);
            Canvas.SetZIndex(rect, 2);
            Canvas.SetLeft(rect, description.DownLeft.X - 3);
            Canvas.SetTop(rect, description.DownLeft.Y - 3);
            var downleft_point = new Tuple<int, int, int>(type, name, point);
            spine_points_rectangles[downleft_point] = rect;

            point = 2;
            rect = new System.Windows.Shapes.Rectangle();
            rect.Width = 5;
            rect.Height = 5;
            rect.Fill = new SolidColorBrush(Colors.Green);
            rect.Tag = new Tuple<int, int, int>(type, name, point);
            rect.MouseDown += Rect_MouseDown;
            rect.MouseUp += Rect_MouseUp;
            myCanvas.Children.Add(rect);
            Canvas.SetZIndex(rect, 2);
            Canvas.SetLeft(rect, description.DownRight.X - 3);
            Canvas.SetTop(rect, description.DownRight.Y - 3);
            var downright_point = new Tuple<int, int, int>(type, name, point);
            spine_points_rectangles[downright_point] = rect;

            point = 3;
            rect = new System.Windows.Shapes.Rectangle();
            rect.Width = 5;
            rect.Height = 5;
            rect.Fill = new SolidColorBrush(Colors.Yellow);
            rect.Tag = new Tuple<int, int, int>(type, name, point);
            rect.MouseDown += Rect_MouseDown;
            rect.MouseUp += Rect_MouseUp;
            myCanvas.Children.Add(rect);
            Canvas.SetZIndex(rect, 2);
            Canvas.SetLeft(rect, description.UpRight.X - 3);
            Canvas.SetTop(rect, description.UpRight.Y - 3);
            var upright_point = new Tuple<int, int, int>(type, name, point);
            spine_points_rectangles[upright_point] = rect;

            var key = new Tuple<Tuple<int, int, int>, Tuple<int, int, int>>(upleft_point, downleft_point);
            var line = new System.Windows.Shapes.Line();
            line.Stroke = System.Windows.Media.Brushes.Red;
            line.StrokeThickness = 2;
            line.X1 = description.UpLeft.X;
            line.Y1 = description.UpLeft.Y;
            line.X2 = description.DownLeft.X;
            line.Y2 = description.DownLeft.Y;
            if (markerLine != null) {
                if (markerLine.Item1.Equals(upleft_point) && markerLine.Item2.Equals(downleft_point))
                {
                    line.Stroke = System.Windows.Media.Brushes.Orange;
                }
            }
            line.MouseDown += MarkerLineMouseDown;
            line.Tag = key;
            myCanvas.Children.Add(line);
            Canvas.SetZIndex(line, 1);
            drawn_spine_lines[key] = line;

            key = new Tuple<Tuple<int, int, int>, Tuple<int, int, int>>(downleft_point, downright_point);
            line = new System.Windows.Shapes.Line();
            line.Stroke = System.Windows.Media.Brushes.Red;
            line.StrokeThickness = 2;
            line.X1 = description.DownLeft.X;
            line.Y1 = description.DownLeft.Y;
            line.X2 = description.DownRight.X;
            line.Y2 = description.DownRight.Y;
            if (markerLine != null)
            {
                if (markerLine.Item1.Equals(downleft_point) && markerLine.Item2.Equals(downright_point))
                {
                    line.Stroke = System.Windows.Media.Brushes.Orange;
                }
            }
            line.MouseDown += MarkerLineMouseDown;
            line.Tag = key;
            myCanvas.Children.Add(line);
            Canvas.SetZIndex(line, 1);
            drawn_spine_lines[key] = line;

            key = new Tuple<Tuple<int, int, int>, Tuple<int, int, int>>(downright_point, upright_point);
            line = new System.Windows.Shapes.Line();
            line.Stroke = System.Windows.Media.Brushes.Red;
            line.StrokeThickness = 2;
            line.X1 = description.DownRight.X;
            line.Y1 = description.DownRight.Y;
            line.X2 = description.UpRight.X;
            line.Y2 = description.UpRight.Y;
            if (markerLine != null)
            {
                if (markerLine.Item1.Equals(downright_point) && markerLine.Item2.Equals(upright_point))
                {
                    line.Stroke = System.Windows.Media.Brushes.Orange;
                }
            }
            line.MouseDown += MarkerLineMouseDown;
            line.Tag = key;
            myCanvas.Children.Add(line);
            Canvas.SetZIndex(line, 1);
            drawn_spine_lines[key] = line;

            key = new Tuple<Tuple<int, int, int>, Tuple<int, int, int>>(upright_point, upleft_point);
            line = new System.Windows.Shapes.Line();
            line.Stroke = System.Windows.Media.Brushes.Red;
            line.StrokeThickness = 2;
            line.X1 = description.UpRight.X;
            line.Y1 = description.UpRight.Y;
            line.X2 = description.UpLeft.X;
            line.Y2 = description.UpLeft.Y;
            if (markerLine != null)
            {
                if (markerLine.Item1.Equals(upright_point) && markerLine.Item2.Equals(upleft_point))
                {
                    line.Stroke = System.Windows.Media.Brushes.Orange;
                }
            }
            line.MouseDown += MarkerLineMouseDown;
            line.Tag = key;
            myCanvas.Children.Add(line);
            Canvas.SetZIndex(line, 1);
            drawn_spine_lines[key] = line;

        }

        private void AddProcessRectangles(string process_name, SpinousProcessDescription description)
        {

            int type = 1;
            int name = SpineConstants.InterSpineNames.IndexOf(process_name);

            int point = 0;
            System.Windows.Shapes.Rectangle rect = new System.Windows.Shapes.Rectangle();
            rect.Width = 5;
            rect.Height = 5;
            rect.Fill = new SolidColorBrush(Colors.Violet);
            rect.Tag = new Tuple<int, int, int>(type, name, point);
            rect.MouseDown += Rect_MouseDown;
            rect.MouseUp += Rect_MouseUp;
            myCanvas.Children.Add(rect);
            Panel.SetZIndex(rect, 2);
            Canvas.SetLeft(rect, description.UpPoint.X - 3);
            Canvas.SetTop(rect, description.UpPoint.Y - 3);
            var up_point = new Tuple<int, int, int>(type, name, point);
            process_points_rectangles[up_point] = rect;

            point = 1;
            rect = new System.Windows.Shapes.Rectangle();
            rect.Width = 5;
            rect.Height = 5;
            rect.Fill = new SolidColorBrush(Colors.Tomato);
            rect.Tag = new Tuple<int, int, int>(type, name, point);
            rect.MouseDown += Rect_MouseDown;
            rect.MouseUp += Rect_MouseUp;
            myCanvas.Children.Add(rect);
            Panel.SetZIndex(rect, 2);
            Canvas.SetLeft(rect, description.VertexPoint.X - 3);
            Canvas.SetTop(rect, description.VertexPoint.Y - 3);
            var vertex_point = new Tuple<int, int, int>(type, name, point);
            process_points_rectangles[vertex_point] = rect;

            point = 2;
            rect = new System.Windows.Shapes.Rectangle();
            rect.Width = 5;
            rect.Height = 5;
            rect.Fill = new SolidColorBrush(Colors.Salmon);
            rect.Tag = new Tuple<int, int, int>(type, name, point);
            rect.MouseDown += Rect_MouseDown;
            rect.MouseUp += Rect_MouseUp;
            myCanvas.Children.Add(rect);
            Panel.SetZIndex(rect, 2);
            Canvas.SetLeft(rect, description.DownPoint.X - 3);
            Canvas.SetTop(rect, description.DownPoint.Y - 3);
            var down_point = new Tuple<int, int, int>(type, name, point);
            process_points_rectangles[down_point] = rect;

            var key = new Tuple<Tuple<int, int, int>, Tuple<int, int, int>>(up_point, vertex_point);
            var line = new System.Windows.Shapes.Line();
            line.Stroke = System.Windows.Media.Brushes.LightSkyBlue;
            line.StrokeThickness = 2;
            line.X1 = description.UpPoint.X;
            line.Y1 = description.UpPoint.Y;
            line.X2 = description.VertexPoint.X;
            line.Y2 = description.VertexPoint.Y;
            if (markerLine != null)
            {
                if (markerLine.Item1.Equals(up_point) && markerLine.Item2.Equals(vertex_point))
                {
                    line.Stroke = System.Windows.Media.Brushes.Orange;
                }
            }
            line.MouseDown += MarkerLineMouseDown;
            line.Tag = key;
            myCanvas.Children.Add(line);
            Panel.SetZIndex(line, 1);
            drawn_process_lines[key] = line;

            key = new Tuple<Tuple<int, int, int>, Tuple<int, int, int>>(vertex_point, down_point);
            line = new System.Windows.Shapes.Line();
            line.Stroke = System.Windows.Media.Brushes.LightSkyBlue;
            line.StrokeThickness = 2;
            line.X1 = description.VertexPoint.X;
            line.Y1 = description.VertexPoint.Y;
            line.X2 = description.DownPoint.X;
            line.Y2 = description.DownPoint.Y;
            if (markerLine != null)
            {
                if (markerLine.Item1.Equals(vertex_point) && markerLine.Item2.Equals(down_point))
                {
                    line.Stroke = System.Windows.Media.Brushes.Orange;
                }
            }
            line.MouseDown += MarkerLineMouseDown;
            line.Tag = key;
            myCanvas.Children.Add(line);
            Canvas.SetZIndex(line, 1);
            drawn_process_lines[key] = line;
        }

        private void MarkerLineMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Middle)
            {
                var line = (sender as System.Windows.Shapes.Line);
                var key = (line.Tag as Tuple<Tuple<int, int, int>, Tuple<int, int, int>>);
                if (markerLine != null)
                {
                    if (drawn_spine_lines.ContainsKey(markerLine))
                    {
                        drawn_spine_lines[markerLine].Stroke = System.Windows.Media.Brushes.Red;
                    }
                    else if (drawn_process_lines.ContainsKey(markerLine))
                    {
                        drawn_process_lines[markerLine].Stroke = System.Windows.Media.Brushes.LightSkyBlue;
                    }
                }

                spines_storage.MarkerLine = key;
                markerLine = key;
                RefreshMarkerLine();
                if (drawn_spine_lines.ContainsKey(key))
                {
                    drawn_spine_lines[key].Stroke = System.Windows.Media.Brushes.Orange;
                }
                else if (drawn_process_lines.ContainsKey(key))
                {
                    drawn_process_lines[key].Stroke = System.Windows.Media.Brushes.Orange;
                }

                needSave = true;
                CheckNeedSave();
            }
        }

        private void RefreshMarkerLine() {
            if (markerLine != null) {
                var point1_ind = markerLine.Item1;
                var point2_ind = markerLine.Item2;
                if (point1_ind.Item1 == 0 && point2_ind.Item1 == 0) {
                    var rect1 = spine_points_rectangles[point1_ind];
                    var rect2 = spine_points_rectangles[point2_ind];

                    var point1 = new System.Windows.Point(Canvas.GetLeft(rect1) + 3, Canvas.GetTop(rect1) + 3);
                    var point2 = new System.Windows.Point(Canvas.GetLeft(rect2) + 3, Canvas.GetTop(rect2) + 3);

                    spines_storage.MarkerLength = System.Windows.Point.Subtract(point1, point2).Length;
                }
            }
        }

        private void RefreshProcessPointsDescription(SpineStorage spines_storage)
        {
            process_points.Clear();
            var keys = spines_storage.SpinousProcessKeys;
            foreach (var name in SpineConstants.SpineNames)
            {
                var index = SpineConstants.SpineNames.IndexOf(name);
                if (index < SpineConstants.SpineNames.Count - 1)
                {
                    var nm = SpineConstants.InterSpineNames[index];
                    if (keys.Contains(nm))
                    {
                        var val = spines_storage.GetSpinousProcessDescription(nm);
                        process_points[nm] = val;
                    }
                }


            }
        }

        private void RefreshImage()
        {
            if (raw != null)
            {
                if (newpix == null)
                {
                    newpix = DicomUtils.ChangeWindowWidthCenter(raw, imageWidth, imageCenter);
                }
                else {
                    DicomUtils.ChangeWindowWidthCenter(raw, newpix, imageWidth, imageCenter);
                }

                if (pixels == null)
                {
                    pixels = DicomUtils.CreateBitmap(newpix);
                }
                else
                {
                    DicomUtils.ChangeBitmap(newpix, pixels);
                    needSave = true;
                    CheckNeedSave();

                }

                pixels = DicomUtils.RotateImg(pixels, spines_storage.GetRotatingAngle(), System.Drawing.Color.Black);

                ms.Position = 0;
                ms.SetLength(0);
                pixels.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                


                BitmapImage bi = new BitmapImage();
                bi.BeginInit();
                bi.StreamSource = ms;
                bi.EndInit();

                image.Source = bi;


            }
        }

        private void image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            startPoint = e.GetPosition(sender as IInputElement);


            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (e.ClickCount == 2)
                {
                    int type = instrumentWindow.TypeComboBox.SelectedIndex;
                    int name = instrumentWindow.NameComboBox.SelectedIndex;
                    int point = instrumentWindow.PointComboBox.SelectedIndex;

                    #region Spine
                    if (type == 0)
                    {
                        if (name >= 0)
                        {
                            if (point >= 0)
                            {
                                var spine_name = SpineConstants.SpineNames[name];

                                if (!spines_points.ContainsKey(spine_name))
                                {
                                    spines_points[spine_name] = new SpineDescription();
                                }

                                var current_point = new Tuple<int, int, int>(type, name, point);

                                if (spine_points_rectangles.ContainsKey(current_point))
                                {
                                    return;
                                }

                                var prev_point = new Tuple<int, int, int>(type, name, (point + 3) % 4);
                                var next_point = new Tuple<int, int, int>(type, name, (point + 1) % 4);

                                System.Windows.Shapes.Rectangle rect = new System.Windows.Shapes.Rectangle();
                                rect.Width = 5;
                                rect.Height = 5;
                                switch (point)
                                {

                                    case 0:
                                        spines_points[spine_name].UpLeft = new System.Drawing.Point((int)startPoint.X, (int)startPoint.Y);
                                        rect.Fill = new SolidColorBrush(Colors.Blue);

                                        if (spine_points_rectangles.ContainsKey(prev_point))
                                        {
                                            var pair_prev = new Tuple<Tuple<int, int, int>, Tuple<int, int, int>>(prev_point, current_point);
                                            if (!drawn_spine_lines.ContainsKey(pair_prev))
                                            {
                                                var line = new System.Windows.Shapes.Line();
                                                line.Stroke = System.Windows.Media.Brushes.Red;
                                                line.StrokeThickness = 2;
                                                line.X1 = spines_points[spine_name].UpRight.X;
                                                line.Y1 = spines_points[spine_name].UpRight.Y;
                                                line.X2 = startPoint.X;
                                                line.Y2 = startPoint.Y;
                                                line.MouseDown += this.MarkerLineMouseDown;
                                                line.Tag = pair_prev;
                                                myCanvas.Children.Add(line);
                                                Canvas.SetZIndex(line, 1);
                                                drawn_spine_lines[pair_prev] = line;
                                            }
                                        }
                                        if (spine_points_rectangles.ContainsKey(next_point))
                                        {
                                            var pair_next = new Tuple<Tuple<int, int, int>, Tuple<int, int, int>>(current_point, next_point);
                                            if (!drawn_spine_lines.ContainsKey(pair_next))
                                            {
                                                var line = new System.Windows.Shapes.Line();
                                                line.Stroke = System.Windows.Media.Brushes.Red;
                                                line.StrokeThickness = 2;
                                                line.X1 = startPoint.X;
                                                line.Y1 = startPoint.Y;
                                                line.X2 = spines_points[spine_name].DownLeft.X;
                                                line.Y2 = spines_points[spine_name].DownLeft.Y;
                                                line.MouseDown += this.MarkerLineMouseDown;
                                                line.Tag = pair_next;
                                                myCanvas.Children.Add(line);
                                                Canvas.SetZIndex(line, 1);
                                                drawn_spine_lines[pair_next] = line;
                                            }
                                        }
                                        break;
                                    case 1:
                                        spines_points[spine_name].DownLeft = new System.Drawing.Point((int)startPoint.X, (int)startPoint.Y);
                                        rect.Fill = new SolidColorBrush(Colors.Red);

                                        if (spine_points_rectangles.ContainsKey(prev_point))
                                        {
                                            var pair_prev = new Tuple<Tuple<int, int, int>, Tuple<int, int, int>>(prev_point, current_point);
                                            if (!drawn_spine_lines.ContainsKey(pair_prev))
                                            {
                                                var line = new System.Windows.Shapes.Line();
                                                line.Stroke = System.Windows.Media.Brushes.Red;
                                                line.StrokeThickness = 2;
                                                line.X1 = spines_points[spine_name].UpLeft.X;
                                                line.Y1 = spines_points[spine_name].UpLeft.Y;
                                                line.X2 = startPoint.X;
                                                line.Y2 = startPoint.Y;
                                                line.MouseDown += this.MarkerLineMouseDown;
                                                line.Tag = pair_prev;
                                                myCanvas.Children.Add(line);
                                                Canvas.SetZIndex(line, 1);
                                                drawn_spine_lines[pair_prev] = line;
                                            }
                                        }
                                        if (spine_points_rectangles.ContainsKey(next_point))
                                        {
                                            var pair_next = new Tuple<Tuple<int, int, int>, Tuple<int, int, int>>(current_point, next_point);
                                            if (!drawn_spine_lines.ContainsKey(pair_next))
                                            {
                                                var line = new System.Windows.Shapes.Line();
                                                line.Stroke = System.Windows.Media.Brushes.Red;
                                                line.StrokeThickness = 2;
                                                line.X1 = startPoint.X;
                                                line.Y1 = startPoint.Y;
                                                line.X2 = spines_points[spine_name].DownRight.X;
                                                line.Y2 = spines_points[spine_name].DownRight.Y;
                                                line.MouseDown += this.MarkerLineMouseDown;
                                                line.Tag = pair_next;
                                                myCanvas.Children.Add(line);
                                                Canvas.SetZIndex(line, 1);
                                                drawn_spine_lines[pair_next] = line;
                                            }
                                        }
                                        break;
                                    case 2:
                                        spines_points[spine_name].DownRight = new System.Drawing.Point((int)startPoint.X, (int)startPoint.Y);
                                        rect.Fill = new SolidColorBrush(Colors.Green);

                                        if (spine_points_rectangles.ContainsKey(prev_point))
                                        {
                                            var pair_prev = new Tuple<Tuple<int, int, int>, Tuple<int, int, int>>(prev_point, current_point);
                                            if (!drawn_spine_lines.ContainsKey(pair_prev))
                                            {
                                                var line = new System.Windows.Shapes.Line();
                                                line.Stroke = System.Windows.Media.Brushes.Red;
                                                line.StrokeThickness = 2;
                                                line.X1 = spines_points[spine_name].DownLeft.X;
                                                line.Y1 = spines_points[spine_name].DownLeft.Y;
                                                line.X2 = startPoint.X;
                                                line.Y2 = startPoint.Y;
                                                line.MouseDown += this.MarkerLineMouseDown;
                                                line.Tag = pair_prev;
                                                myCanvas.Children.Add(line);
                                                Canvas.SetZIndex(line, 1);
                                                drawn_spine_lines[pair_prev] = line;
                                            }
                                        }
                                        if (spine_points_rectangles.ContainsKey(next_point))
                                        {
                                            var pair_next = new Tuple<Tuple<int, int, int>, Tuple<int, int, int>>(current_point, next_point);
                                            if (!drawn_spine_lines.ContainsKey(pair_next))
                                            {
                                                var line = new System.Windows.Shapes.Line();
                                                line.Stroke = System.Windows.Media.Brushes.Red;
                                                line.StrokeThickness = 2;
                                                line.X1 = startPoint.X;
                                                line.Y1 = startPoint.Y;
                                                line.X2 = spines_points[spine_name].UpRight.X;
                                                line.Y2 = spines_points[spine_name].UpRight.Y;
                                                line.MouseDown += this.MarkerLineMouseDown;
                                                line.Tag = pair_next;
                                                myCanvas.Children.Add(line);
                                                Canvas.SetZIndex(line, 1);
                                                drawn_spine_lines[pair_next] = line;
                                            }
                                        }
                                        break;
                                    case 3:
                                        spines_points[spine_name].UpRight = new System.Drawing.Point((int)startPoint.X, (int)startPoint.Y);
                                        rect.Fill = new SolidColorBrush(Colors.Yellow);

                                        if (spine_points_rectangles.ContainsKey(prev_point))
                                        {
                                            var pair_prev = new Tuple<Tuple<int, int, int>, Tuple<int, int, int>>(prev_point, current_point);
                                            if (!drawn_spine_lines.ContainsKey(pair_prev))
                                            {
                                                var line = new System.Windows.Shapes.Line();
                                                line.Stroke = System.Windows.Media.Brushes.Red;
                                                line.StrokeThickness = 2;
                                                line.X1 = spines_points[spine_name].DownRight.X;
                                                line.Y1 = spines_points[spine_name].DownRight.Y;
                                                line.X2 = startPoint.X;
                                                line.Y2 = startPoint.Y;
                                                line.MouseDown += this.MarkerLineMouseDown;
                                                line.Tag = pair_prev;
                                                myCanvas.Children.Add(line);
                                                Canvas.SetZIndex(line, 1);
                                                drawn_spine_lines[pair_prev] = line;
                                            }
                                        }
                                        if (spine_points_rectangles.ContainsKey(next_point))
                                        {
                                            var pair_next = new Tuple<Tuple<int, int, int>, Tuple<int, int, int>>(current_point, next_point);
                                            if (!drawn_spine_lines.ContainsKey(pair_next))
                                            {
                                                var line = new System.Windows.Shapes.Line();
                                                line.Stroke = System.Windows.Media.Brushes.Red;
                                                line.StrokeThickness = 2;
                                                line.X1 = startPoint.X;
                                                line.Y1 = startPoint.Y;
                                                line.X2 = spines_points[spine_name].UpLeft.X;
                                                line.Y2 = spines_points[spine_name].UpLeft.Y;
                                                line.MouseDown += this.MarkerLineMouseDown;
                                                line.Tag = pair_next;
                                                myCanvas.Children.Add(line);
                                                Canvas.SetZIndex(line, 1);
                                                drawn_spine_lines[pair_next] = line;
                                            }
                                        }
                                        break;
                                }

                                rect.Tag = current_point;
                                rect.MouseDown += Rect_MouseDown;
                                rect.MouseUp += Rect_MouseUp;
                                myCanvas.Children.Add(rect);
                                Canvas.SetZIndex(rect, 2);
                                Canvas.SetLeft(rect, startPoint.X - 3);
                                Canvas.SetTop(rect, startPoint.Y - 3);
                                spine_points_rectangles[current_point] = rect;

                                int count = 0;
                                for (byte i = 0; i < 4; i++)
                                {
                                    if (spine_points_rectangles.ContainsKey(new Tuple<int, int, int>(type, name, i)))
                                    {
                                        count++;
                                    }
                                }
                                if (count == 4)
                                {
                                    spines_storage.AddDescription(spine_name, spines_points[spine_name]);
                                }
                                SwitchToNextPointInstrument(current_point);
                            }
                        }
                    }
                    #endregion
                    #region Process
                    else if (type == 1)
                    {
                        if (name >= 0)
                        {
                            if (point >= 0)
                            {
                                var process_name = instrumentWindow.NameComboBox.SelectedItem as string;

                                var current_point = new Tuple<int, int, int>(type, name, point);

                                Tuple<int, int, int> prev_point = null;
                                Tuple<int, int, int> next_point = null;

                                switch (point)
                                {
                                    case 0:
                                        next_point = new Tuple<int, int, int>(type, name, 1);
                                        break;
                                    case 1:
                                        prev_point = new Tuple<int, int, int>(type, name, 0);
                                        next_point = new Tuple<int, int, int>(type, name, 2);
                                        break;
                                    case 2:
                                        prev_point = new Tuple<int, int, int>(type, name, 1);
                                        break;
                                }

                                if (!process_points.ContainsKey(process_name))
                                {
                                    process_points[process_name] = new SpinousProcessDescription();
                                }

                                if (process_points_rectangles.ContainsKey(current_point))
                                {
                                    return;
                                }


                                System.Windows.Shapes.Rectangle rect = new System.Windows.Shapes.Rectangle();
                                rect.Width = 5;
                                rect.Height = 5;

                                switch (point)
                                {

                                    case 0:
                                        process_points[process_name].UpPoint = new System.Drawing.Point((int)startPoint.X, (int)startPoint.Y);
                                        rect.Fill = new SolidColorBrush(Colors.Violet);

                                        if (process_points_rectangles.ContainsKey(next_point))
                                        {
                                            var pair_next = new Tuple<Tuple<int, int, int>, Tuple<int, int, int>>(current_point, next_point);
                                            if (!drawn_process_lines.ContainsKey(pair_next))
                                            {
                                                var line = new System.Windows.Shapes.Line();
                                                line.Stroke = System.Windows.Media.Brushes.LightSkyBlue;
                                                line.StrokeThickness = 2;
                                                line.X1 = startPoint.X;
                                                line.Y1 = startPoint.Y;
                                                line.X2 = process_points[process_name].VertexPoint.X;
                                                line.Y2 = process_points[process_name].VertexPoint.Y;
                                                line.MouseDown += this.MarkerLineMouseDown;
                                                line.Tag = pair_next;
                                                myCanvas.Children.Add(line);
                                                Canvas.SetZIndex(line, 1);
                                                drawn_process_lines[pair_next] = line;
                                            }
                                        }
                                        break;
                                    case 1:
                                        process_points[process_name].VertexPoint = new System.Drawing.Point((int)startPoint.X, (int)startPoint.Y);
                                        rect.Fill = new SolidColorBrush(Colors.Tomato);


                                        if (process_points_rectangles.ContainsKey(next_point))
                                        {
                                            var pair_next = new Tuple<Tuple<int, int, int>, Tuple<int, int, int>>(current_point, next_point);
                                            if (!drawn_process_lines.ContainsKey(pair_next))
                                            {
                                                var line = new System.Windows.Shapes.Line();
                                                line.Stroke = System.Windows.Media.Brushes.LightSkyBlue;
                                                line.StrokeThickness = 2;
                                                line.X1 = startPoint.X;
                                                line.Y1 = startPoint.Y;
                                                line.X2 = process_points[process_name].DownPoint.X;
                                                line.Y2 = process_points[process_name].DownPoint.Y;
                                                line.MouseDown += this.MarkerLineMouseDown;
                                                line.Tag = pair_next;
                                                myCanvas.Children.Add(line);
                                                Canvas.SetZIndex(line, 1);
                                                drawn_process_lines[pair_next] = line;
                                            }
                                        }


                                        if (process_points_rectangles.ContainsKey(prev_point))
                                        {
                                            var pair_prev = new Tuple<Tuple<int, int, int>, Tuple<int, int, int>>(prev_point, current_point);
                                            if (!drawn_process_lines.ContainsKey(pair_prev))
                                            {
                                                var line = new System.Windows.Shapes.Line();
                                                line.Stroke = System.Windows.Media.Brushes.LightSkyBlue;
                                                line.StrokeThickness = 2;
                                                line.X1 = process_points[process_name].UpPoint.X;
                                                line.Y1 = process_points[process_name].UpPoint.Y;
                                                line.X2 = startPoint.X;
                                                line.Y2 = startPoint.Y;
                                                line.MouseDown += this.MarkerLineMouseDown;
                                                line.Tag = pair_prev;
                                                myCanvas.Children.Add(line);
                                                Canvas.SetZIndex(line, 1);
                                                drawn_process_lines[pair_prev] = line;
                                            }
                                        }
                                        break;
                                    case 2:
                                        process_points[process_name].DownPoint = new System.Drawing.Point((int)startPoint.X, (int)startPoint.Y);
                                        rect.Fill = new SolidColorBrush(Colors.Salmon);


                                        if (process_points_rectangles.ContainsKey(prev_point))
                                        {
                                            var pair_prev = new Tuple<Tuple<int, int, int>, Tuple<int, int, int>>(prev_point, current_point);
                                            if (!drawn_process_lines.ContainsKey(pair_prev))
                                            {
                                                var line = new System.Windows.Shapes.Line();
                                                line.Stroke = System.Windows.Media.Brushes.LightSkyBlue;
                                                line.StrokeThickness = 2;
                                                line.X1 = process_points[process_name].VertexPoint.X;
                                                line.Y1 = process_points[process_name].VertexPoint.Y;
                                                line.X2 = startPoint.X;
                                                line.Y2 = startPoint.Y;
                                                line.MouseDown += this.MarkerLineMouseDown;
                                                line.Tag = pair_prev;
                                                myCanvas.Children.Add(line);
                                                Canvas.SetZIndex(line, 1);
                                                drawn_process_lines[pair_prev] = line;
                                            }
                                        }


                                        break;
                                }
                                rect.Tag = current_point;
                                rect.MouseDown += Rect_MouseDown;
                                rect.MouseUp += Rect_MouseUp;
                                myCanvas.Children.Add(rect);
                                Canvas.SetZIndex(rect, 2);
                                Canvas.SetLeft(rect, startPoint.X - 3);
                                Canvas.SetTop(rect, startPoint.Y - 3);
                                process_points_rectangles[current_point] = rect;

                                int count = 0;
                                for (byte i = 0; i < 3; i++)
                                {
                                    if (process_points_rectangles.ContainsKey(new Tuple<int, int, int>(type, name, i)))
                                    {
                                        count++;
                                    }
                                }
                                if (count == 3)
                                {
                                    spines_storage.AddSpinousProcessDescription(process_name, process_points[process_name]);
                                }
                                SwitchToNextPointInstrument(current_point);
                            }
                        }
                    }
                    #endregion
                    #region Markers
                    else if (type == 2)
                    {
                        name = 0;
                        if (point >= 0)
                        {
                            var current_point = new Tuple<int, int, int>(type, name, point);

                            if (markers_rectangles.ContainsKey(current_point))
                            {
                                return;
                            }

                            marker_points[current_point] = startPoint;


                            var label = new Label();
                            label.FontSize = 18;
                            label.Foreground = System.Windows.Media.Brushes.Red;
                            Canvas.SetZIndex(label, 2);
                            Canvas.SetLeft(label, startPoint.X - 5);
                            Canvas.SetTop(label, startPoint.Y - 5);

                            switch (point)
                            {
                                case 0:
                                    label.Content = "Г";
                                    break;
                                case 1:
                                    label.Content = "Н";
                                    break;
                                case 2:
                                    if (spines_storage.imageDirection == 0)
                                    {
                                        label.Content = "С";
                                    }
                                    else
                                    {
                                        label.Content = "Ж";
                                    }
                                    break;
                                case 3:
                                    if (spines_storage.imageDirection == 0)
                                    {
                                        label.Content = "НС";
                                    }
                                    else
                                    {
                                        label.Content = "С";
                                    }
                                    break;
                            }

                            label.Tag = current_point;
                            label.MouseDown += Rect_MouseDown;
                            label.MouseUp += Rect_MouseUp;
                            markers_rectangles[current_point] = label;
                            myCanvas.Children.Add(label);



                            int count = 0;
                            for (byte i = 0; i < 4; i++)
                            {
                                if (markers_rectangles.ContainsKey(new Tuple<int, int, int>(type, name, i)))
                                {
                                    count++;
                                }
                            }
                            if (count == 4)
                            {
                                int i = 0;
                                foreach (var val in marker_points.Values)
                                {
                                    spines_storage.SetMarkerPoint(i, new System.Drawing.Point((int)val.X, (int)val.Y));
                                    i++;
                                }
                                spines_storage.RecalcDirections(pixels.Width, pixels.Height);
                            }

                            SwitchToNextPointInstrument(current_point);

                        }
                    }
                    #endregion
                }


                needSave = true;
                CheckNeedSave();

                leftMouseButtonDown = true;
            }
            else if (e.RightButton == MouseButtonState.Pressed)
            {
                rightMouseButtonDown = true;
            }
            else if (e.MiddleButton == MouseButtonState.Pressed)
            {
                middleMouseButtonDown = true;
            }
        }

        private System.Windows.Point RotatePoint(System.Windows.Point original, int angle, int width, int height)
        {

            if (angle < 0)
            {
                angle = (angle + 360) % 360;
            }

            if (angle == 0)
                return original;
            else if (angle == 90)
                return new System.Windows.Point(height - original.Y, original.X);
            else if (angle == 180)
                return new System.Windows.Point(width - original.X, height - original.Y);
            else if (angle == 270)
                return new System.Windows.Point(original.Y, width - original.X);
            else
                return original;
        }

        private void SwitchToNextPointInstrument(Tuple<int, int, int> rect)
        {
            var type = rect.Item1;
            var name = rect.Item2;
            var point = rect.Item3;
            if (type == 0)
            {
                if (point < 3)
                {
                    point++;
                }
                else
                {
                    if (name < SpineConstants.SpineNames.Count - 1)
                    {
                        name++;
                        point = 0;
                    }
                }
            }
            else if (type == 1)
            {
                if (point < 2)
                {
                    point++;
                }
                else
                {
                    if (name < SpineConstants.InterSpineNames.Count - 1)
                    {
                        name++;
                        point = 0;
                    }
                }
            }
            else if (type == 2)
            {
                if (point < 1)
                {
                    point++;
                }
            }

            else if (type == 3)
            {
                if (point < 3)
                {
                    point++;
                }
            }

            instrumentWindow.TypeComboBox.SelectedIndex = type;
            if (type != 2)
            {
                instrumentWindow.NameComboBox.SelectedIndex = name;
            }
            instrumentWindow.PointComboBox.SelectedIndex = point;
        }

        private void image_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (leftMouseButtonDown)
            {
                leftMouseButtonDown = false;
            }
            else if (rightMouseButtonDown)
            {
                rightMouseButtonDown = false;
            }
            else if (middleMouseButtonDown)
            {
                middleMouseButtonDown = false;
            }
        }

        private void myCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            var pos = e.GetPosition(sender as IInputElement);

            var diff = pos - startPoint;
            if (leftMouseButtonDown)
            {
                if (currentMovingRect != null)
                {
                    Canvas.SetTop(currentMovingRect, pos.Y);
                    Canvas.SetLeft(currentMovingRect, pos.X);
                    UpdateRectangle(currentMovingRect);
                    RefreshMarkerLine();
                }

            }
            else if (rightMouseButtonDown)
            {
                var diffX = (short)(diff.X / (imageTransform.Children[0] as ScaleTransform).ScaleX / 5) ;
                var diffY = (short)(diff.Y / (imageTransform.Children[0] as ScaleTransform).ScaleY / 5) ;
                imageWidth += diffX;

                if (imageWidth < 1)
                    imageWidth = 1;

                imageCenter += diffY;
                if (spines_storage != null)
                {
                    spines_storage.windowWidth = imageWidth;
                    spines_storage.windowCenter = imageCenter;
                }
                RefreshImage();

                needSave = true;
                CheckNeedSave();
            }
            else if (middleMouseButtonDown)
            {
            }
        }

        private void UpdateRectangle(FrameworkElement currentMovingRect)
        {
            Tuple<int, int, int> tag = currentMovingRect.Tag as Tuple<int, int, int>;

            System.Windows.Point pos;
            double X = 0, Y = 0;
            switch (tag.Item1)
            {
                case 0:
                case 1:
                case 2:
                    X = Canvas.GetLeft(currentMovingRect) + 3;
                    Y = Canvas.GetTop(currentMovingRect) + 3;
                    break;
                case 3:
                    X = Canvas.GetLeft(currentMovingRect) + 5;
                    Y = Canvas.GetTop(currentMovingRect) + 5;
                    break;
            }
            pos = new System.Windows.Point(X, Y);

            #region Process points
            if (tag.Item1 == 1)
            {
                var current_point = tag;
                var type = tag.Item1;
                var name = tag.Item2;
                var point = tag.Item3;

                var process_name = instrumentWindow.NameComboBox.SelectedItem as string;

                Tuple<int, int, int> prev_point = null;
                Tuple<int, int, int> next_point = null;

                switch (point)
                {
                    case 0:
                        next_point = new Tuple<int, int, int>(type, name, 1);
                        break;
                    case 1:
                        prev_point = new Tuple<int, int, int>(type, name, 0);
                        next_point = new Tuple<int, int, int>(type, name, 2);
                        break;
                    case 2:
                        prev_point = new Tuple<int, int, int>(type, name, 1);
                        break;
                }


                switch (point)
                {
                    case 0:
                        process_points[process_name].UpPoint = new System.Drawing.Point((int)pos.X, (int)pos.Y);
                        break;
                    case 1:
                        process_points[process_name].VertexPoint = new System.Drawing.Point((int)pos.X, (int)pos.Y);
                        break;
                    case 2:
                        process_points[process_name].DownPoint = new System.Drawing.Point((int)pos.X, (int)pos.Y);
                        break;
                }

                spines_storage.AddSpinousProcessDescription(process_name, process_points[process_name]);

                switch (point)
                {

                    case 0:
                        if (process_points_rectangles.ContainsKey(next_point))
                        {
                            var pair_next = new Tuple<Tuple<int, int, int>, Tuple<int, int, int>>(current_point, next_point);
                            if (drawn_process_lines.ContainsKey(pair_next))
                            {
                                drawn_process_lines[pair_next].X1 = pos.X;
                                drawn_process_lines[pair_next].Y1 = pos.Y;
                            }
                        }
                        break;
                    case 1:
                        if (process_points_rectangles.ContainsKey(next_point))
                        {
                            var pair_next = new Tuple<Tuple<int, int, int>, Tuple<int, int, int>>(current_point, next_point);
                            if (drawn_process_lines.ContainsKey(pair_next))
                            {
                                drawn_process_lines[pair_next].X1 = pos.X;
                                drawn_process_lines[pair_next].Y1 = pos.Y;
                            }
                        }
                        if (process_points_rectangles.ContainsKey(prev_point))
                        {
                            var pair_prev = new Tuple<Tuple<int, int, int>, Tuple<int, int, int>>(prev_point, current_point);
                            if (drawn_process_lines.ContainsKey(pair_prev))
                            {
                                drawn_process_lines[pair_prev].X2 = pos.X;
                                drawn_process_lines[pair_prev].Y2 = pos.Y;
                            }
                        }
                        break;
                    case 2:
                        if (process_points_rectangles.ContainsKey(prev_point))
                        {
                            var pair_prev = new Tuple<Tuple<int, int, int>, Tuple<int, int, int>>(prev_point, current_point);
                            if (drawn_process_lines.ContainsKey(pair_prev))
                            {
                                drawn_process_lines[pair_prev].X2 = pos.X;
                                drawn_process_lines[pair_prev].Y2 = pos.Y;
                            }
                        }
                        break;
                }
            }
            #endregion
            #region Spine points
            else if (tag.Item1 == 0)
            {
                var current_point = tag;
                var type = tag.Item1;
                var name = tag.Item2;
                var point = tag.Item3;


                var spine_name = SpineConstants.SpineNames[name];

                var prev_point = new Tuple<int, int, int>(type, name, (point + 3) % 4);
                var next_point = new Tuple<int, int, int>(type, name, (point + 1) % 4);

                switch (point)
                {
                    case 0:
                        spines_points[spine_name].UpLeft = new System.Drawing.Point((int)pos.X, (int)pos.Y);
                        break;
                    case 1:
                        spines_points[spine_name].DownLeft = new System.Drawing.Point((int)pos.X, (int)pos.Y);
                        break;
                    case 2:
                        spines_points[spine_name].DownRight = new System.Drawing.Point((int)pos.X, (int)pos.Y);
                        break;
                    case 3:
                        spines_points[spine_name].UpRight = new System.Drawing.Point((int)pos.X, (int)pos.Y);
                        break;
                }

                spines_storage.AddDescription(spine_name, spines_points[spine_name]);

                switch (point)
                {

                    case 0:
                        if (spine_points_rectangles.ContainsKey(prev_point))
                        {
                            var pair_prev = new Tuple<Tuple<int, int, int>, Tuple<int, int, int>>(prev_point, current_point);
                            if (drawn_spine_lines.ContainsKey(pair_prev))
                            {
                                drawn_spine_lines[pair_prev].X2 = pos.X;
                                drawn_spine_lines[pair_prev].Y2 = pos.Y;
                            }
                        }
                        if (spine_points_rectangles.ContainsKey(next_point))
                        {
                            var pair_next = new Tuple<Tuple<int, int, int>, Tuple<int, int, int>>(current_point, next_point);
                            if (drawn_spine_lines.ContainsKey(pair_next))
                            {
                                drawn_spine_lines[pair_next].X1 = pos.X;
                                drawn_spine_lines[pair_next].Y1 = pos.Y;
                            }
                        }
                        break;
                    case 1:
                        if (spine_points_rectangles.ContainsKey(prev_point))
                        {
                            var pair_prev = new Tuple<Tuple<int, int, int>, Tuple<int, int, int>>(prev_point, current_point);
                            if (drawn_spine_lines.ContainsKey(pair_prev))
                            {
                                drawn_spine_lines[pair_prev].X2 = pos.X;
                                drawn_spine_lines[pair_prev].Y2 = pos.Y;
                            }
                        }
                        if (spine_points_rectangles.ContainsKey(next_point))
                        {
                            var pair_next = new Tuple<Tuple<int, int, int>, Tuple<int, int, int>>(current_point, next_point);
                            if (drawn_spine_lines.ContainsKey(pair_next))
                            {
                                drawn_spine_lines[pair_next].X1 = pos.X;
                                drawn_spine_lines[pair_next].Y1 = pos.Y;
                            }
                        }
                        break;
                    case 2:
                        if (spine_points_rectangles.ContainsKey(prev_point))
                        {
                            var pair_prev = new Tuple<Tuple<int, int, int>, Tuple<int, int, int>>(prev_point, current_point);
                            if (drawn_spine_lines.ContainsKey(pair_prev))
                            {
                                drawn_spine_lines[pair_prev].X2 = pos.X;
                                drawn_spine_lines[pair_prev].Y2 = pos.Y;
                            }
                        }
                        if (spine_points_rectangles.ContainsKey(next_point))
                        {
                            var pair_next = new Tuple<Tuple<int, int, int>, Tuple<int, int, int>>(current_point, next_point);
                            if (drawn_spine_lines.ContainsKey(pair_next))
                            {
                                drawn_spine_lines[pair_next].X1 = pos.X;
                                drawn_spine_lines[pair_next].Y1 = pos.Y;
                            }
                        }
                        break;
                    case 3:
                        if (spine_points_rectangles.ContainsKey(prev_point))
                        {
                            var pair_prev = new Tuple<Tuple<int, int, int>, Tuple<int, int, int>>(prev_point, current_point);
                            if (drawn_spine_lines.ContainsKey(pair_prev))
                            {
                                drawn_spine_lines[pair_prev].X2 = pos.X;
                                drawn_spine_lines[pair_prev].Y2 = pos.Y;
                            }
                        }
                        if (spine_points_rectangles.ContainsKey(next_point))
                        {
                            var pair_next = new Tuple<Tuple<int, int, int>, Tuple<int, int, int>>(current_point, next_point);
                            if (drawn_spine_lines.ContainsKey(pair_next))
                            {
                                drawn_spine_lines[pair_next].X1 = pos.X;
                                drawn_spine_lines[pair_next].Y1 = pos.Y;
                            }
                        }
                        break;
                }


            }
            #endregion
            #region Markers
            else if (tag.Item1 == 2)
            {
                var current_point = tag;
                var type = tag.Item1;
                var name = tag.Item2;
                var point = tag.Item3;


                marker_points[current_point] = pos;

                if (spines_storage != null)
                {
                    spines_storage.SetMarkerPoint(point, new System.Drawing.Point((int)pos.X, (int)pos.Y));
                    spines_storage.RecalcDirections(pixels.Width, pixels.Height);
                }
            }
            #endregion

            needSave = true;
            CheckNeedSave();
        }

        private void ScaleButton_Click(object sender, RoutedEventArgs e)
        {
            RibbonButton item = sender as RibbonButton;
            if (item != null)
            {
                double scale = double.Parse((item.Tag as string), CultureInfo.InvariantCulture);
                imageTransform.Children[0] = new ScaleTransform(scale, scale);
            }
        }

        private void Rotate90CCWButton_Click(object sender, RoutedEventArgs e)
        {
            rotatingAngle -= 90;
            if (spines_storage != null)
            {
                spines_storage.SetRotatingAngle(rotatingAngle);
                RefreshImage();
                RefreshPoints(-90);
                needSave = true;
                CheckNeedSave();
            }
        }

        private void Rotate90CWButton_Click(object sender, RoutedEventArgs e)
        {
            rotatingAngle += 90;
            if (spines_storage != null)
            {
                spines_storage.SetRotatingAngle(rotatingAngle);
                RefreshImage();
                RefreshPoints(90);
                needSave = true;
                CheckNeedSave();
            }
        }

        private void RefreshPoints(int angle)
        {

            foreach (var key in spine_points_rectangles.Keys)
            {

                var spine_key = SpineConstants.SpineNames[key.Item2];
                var spine = spines_points[spine_key];

                var rect = spine_points_rectangles[key];

                var X = Canvas.GetLeft(rect) + 3;
                var Y = Canvas.GetTop(rect) + 3;

                var p = RotatePoint(new System.Windows.Point(X, Y), angle, pixels.Width, pixels.Height);

                switch (key.Item3)
                {
                    case 0:
                        spine.UpLeft = new System.Drawing.Point((int)p.X, (int)p.Y);
                        break;
                    case 1:
                        spine.DownLeft = new System.Drawing.Point((int)p.X, (int)p.Y);
                        break;
                    case 2:
                        spine.DownRight = new System.Drawing.Point((int)p.X, (int)p.Y);
                        break;
                    case 3:
                        spine.UpRight = new System.Drawing.Point((int)p.X, (int)p.Y);
                        break;
                }

                Canvas.SetLeft(rect, p.X - 3);
                Canvas.SetTop(rect, p.Y - 3);
            }

            foreach (var key in process_points_rectangles.Keys)
            {
                var process_key = SpineConstants.InterSpineNames[key.Item2];
                var process = process_points[process_key];

                var rect = process_points_rectangles[key];

                var X = Canvas.GetLeft(rect) + 3;
                var Y = Canvas.GetTop(rect) + 3;

                var p = RotatePoint(new System.Windows.Point(X, Y), angle, pixels.Width, pixels.Height);

                switch (key.Item3)
                {
                    case 0:
                        process.UpPoint = new System.Drawing.Point((int)p.X, (int)p.Y);
                        break;
                    case 1:
                        process.VertexPoint = new System.Drawing.Point((int)p.X, (int)p.Y);
                        break;
                    case 2:
                        process.DownPoint = new System.Drawing.Point((int)p.X, (int)p.Y);
                        break;
                }


                Canvas.SetLeft(rect, p.X - 3);
                Canvas.SetTop(rect, p.Y - 3);
            }

            foreach (var key in markers_rectangles.Keys)
            {
                var rect = markers_rectangles[key];

                var X = Canvas.GetLeft(rect) + 5;
                var Y = Canvas.GetTop(rect) + 5;

                var p = RotatePoint(new System.Windows.Point(X, Y), angle, pixels.Width, pixels.Height);

                marker_points[key] = p;

                Canvas.SetLeft(rect, p.X - 5);
                Canvas.SetTop(rect, p.Y - 5);
            }

            int i = 0;
            foreach (var mp in marker_points.Values)
            {
                spines_storage.SetMarkerPoint(i, new System.Drawing.Point((int)mp.X, (int)mp.Y));
                i++;
            }

            foreach (var key in drawn_spine_lines.Keys)
            {
                var k1 = key.Item1;
                var k2 = key.Item2;

                var v = drawn_spine_lines[key];

                var rect1 = spine_points_rectangles[k1];
                var rect2 = spine_points_rectangles[k2];

                v.X1 = Canvas.GetLeft(rect1) + 3;
                v.Y1 = Canvas.GetTop(rect1) + 3;
                v.X2 = Canvas.GetLeft(rect2) + 3;
                v.Y2 = Canvas.GetTop(rect2) + 3;
            }

            foreach (var key in drawn_process_lines.Keys)
            {
                var k1 = key.Item1;
                var k2 = key.Item2;

                var v = drawn_process_lines[key];

                var rect1 = process_points_rectangles[k1];
                var rect2 = process_points_rectangles[k2];

                v.X1 = Canvas.GetLeft(rect1) + 3;
                v.Y1 = Canvas.GetTop(rect1) + 3;
                v.X2 = Canvas.GetLeft(rect2) + 3;
                v.Y2 = Canvas.GetTop(rect2) + 3;
            }
         
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            needClose = true;
            instrumentWindow.Close();

            if (spines_storage != null)
            {
                SpineReader.WriteToFile(spines_storage, image_spine_file);
            }


            Application.Current.Shutdown();
        }

        private void SelectionToolWindowButton_Click(object sender, RoutedEventArgs e)
        {
            instrumentWindow.Show();
            instrumentWindow.Activate();
        }

        private void ExportXLSButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentFile != null)
            {
                SaveFileDialog dialog = new SaveFileDialog
                {
                    Filter = "Excel файл (*.xls)|*.xls",
                    FilterIndex = 1,
                    RestoreDirectory = true
                };

                bool success = true;

                if (dialog.ShowDialog() == true)
                {
                    try
                    {
                        var filename = dialog.FileName;

                        DataExporter.ExportToXLS(filename, spines_storage);
                    }

                    catch (Exception)
                    {
                        success = false;
                    }

                    if (!success)
                    {
                        MessageBox.Show("Не удалось сохранить файл", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else
                    {
                        MessageBox.Show("Файл успешно сохранен", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }

            }
        }

        private void ExportXMLButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentFile != null)
            {
                SaveFileDialog dialog = new SaveFileDialog
                {
                    Filter = "xml файл (*.xml)|*.xml",
                    FilterIndex = 1,
                    RestoreDirectory = true
                };

                bool success = true;

                if (dialog.ShowDialog() == true)
                {
                    try
                    {
                        var filename = dialog.FileName;

                        DataExporter.ExportToXML(filename, spines_storage);
                    }
                    catch (Exception)
                    {
                        success = false;
                    }

                    if (!success)
                    {
                        MessageBox.Show("Не удалось сохранить файл", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else
                    {
                        MessageBox.Show("Файл успешно сохранен", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
        }

        private void ShowParametersWindowButton_Click(object sender, RoutedEventArgs e)
        {
            var parametersForm = new ParametersWindow(spines_storage);
            parametersForm.ShowDialog();
        }

        private void ScaleGeneticButton_Click(object sender, RoutedEventArgs e)
        {
            if (spines_storage != null)
            {
                var tr = imageTransform.Children[0] as ScaleTransform;
                ScaleWindow window = new ScaleWindow((int)(tr.ScaleX * 100));
                window.ShowDialog();
                var scale = window.GetScale();
                imageTransform.Children[0] = new ScaleTransform(scale, scale);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void AddPatientButtonMenu_Click(object sender, RoutedEventArgs e)
        {
            PatientNameWindow window = new PatientNameWindow();
            var result = window.ShowDialog();
            if (result.HasValue && result.Value) {
                var name = window.NameTextBox.Text;
                var patronymic = window.PatronymicTextBox.Text;
                var surname = window.SurnameTextBox.Text;
                var age = int.Parse(window.AgeTextBox.Text);

                if (surname.Length != 0 && name.Length != 0)
                {
                    var p = new Patient
                    {
                        Name = name,
                        Patronymic = patronymic,
                        Surname = surname,
                        Age = age
                    };
                    storageIO.InsertPatient(p);
                    RefreshTree();
                }
            }
        }

        private void CloseFile()
        {

            if (spines_storage != null)
            {
                if (needSave)
                {
                    // Dialog
                    var result = MessageBox.Show("Вы действительно закрыть без сохранения", "Предупреждение", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        CloseFileImmediately();
                    }
                    else if (result == MessageBoxResult.No)
                    {
                        SaveFile();
                        CloseFileImmediately();
                    }
                }
                else
                {
                    CloseFileImmediately();
                }
            }

        }

        private void CloseFileImmediately()
        {

            myCanvas.Children.Clear();

            spines_points = new Dictionary<string, SpineDescription>();
            spine_points_rectangles = new Dictionary<Tuple<int, int, int>, System.Windows.Shapes.Rectangle>();

            process_points = new Dictionary<string, SpinousProcessDescription>();
            process_points_rectangles = new Dictionary<Tuple<int, int, int>, System.Windows.Shapes.Rectangle>();

            drawn_spine_lines = new Dictionary<Tuple<Tuple<int, int, int>, Tuple<int, int, int>>, System.Windows.Shapes.Line>();
            drawn_process_lines = new Dictionary<Tuple<Tuple<int, int, int>, Tuple<int, int, int>>, System.Windows.Shapes.Line>();

            currentMovingRect = null;

            markers_rectangles = new Dictionary<Tuple<int, int, int>, Label>();
            marker_points = new Dictionary<Tuple<int, int, int>, System.Windows.Point>();

            image.Source = null;
            spines_storage = null;

            imageWidth = 2000;
            imageCenter = -1000;

            raw = null;
            pixels = null;

            (imageTransform.Children[0] as ScaleTransform).ScaleX = 1;
            (imageTransform.Children[0] as ScaleTransform).ScaleY = 1;
            (imageTransform.Children[1] as RotateTransform).Angle = 0;
            currentFile = null;
            image_hash = "";
            image_spine_file = null;

            markerLine = null;
        }

        private void SaveFile()
        {

            if (spines_storage != null)
            {
                SpineReader.WriteToFile(spines_storage, image_spine_file);
                needSave = false;
                CheckNeedSave();
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFile();
        }

        private void OpenDirButton_Click(object sender, RoutedEventArgs e)
        {
            ItemList.Items.Clear();
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                OpenDirectory(dialog.SelectedPath);
            }
        }

        private void OpenDirectory(string directory)
        {
            var files = DicomUtils.GetDicomFilesFromDirectory(directory);
            OpenFiles(files);
        }

        private void OpenFiles(List<string> files)
        {
            foreach (var file in files)
            {
                var dcmFile = DicomFile.Open(file);
                var wind1252Bytes = dcmFile.Dataset.Get<byte[]>(DicomTag.PatientName);

                Encoding wind1252 = Encoding.GetEncoding(1251);
                Encoding utf8 = Encoding.Unicode;
                byte[] utf8Bytes = Encoding.Convert(wind1252, utf8, wind1252Bytes);
                char[] utf8Chars = new char[utf8.GetCharCount(utf8Bytes, 0, utf8Bytes.Length)];
                utf8.GetChars(utf8Bytes, 0, utf8Bytes.Length, utf8Chars, 0);
                var name = new string(utf8Chars);

                var date = dcmFile.Dataset.Get<string>(DicomTag.StudyDate);

                StackPanel panel = new StackPanel();
                panel.HorizontalAlignment = HorizontalAlignment.Stretch;

                Label labelname = new Label
                {
                    Content = name
                };

                Label labeldate = new Label
                {
                    Content = date
                };

                panel.Children.Add(labelname);
                panel.Children.Add(labeldate);

                Bitmap bmap = DicomUtils.CreateDefaultBitmap(dcmFile);
                Bitmap resized = new Bitmap(bmap, new System.Drawing.Size(128, 128));

                System.Windows.Controls.Image image = new System.Windows.Controls.Image();

                MemoryStream ms = new MemoryStream();
                resized.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                ms.Position = 0;
                BitmapImage bi = new BitmapImage();
                bi.BeginInit();
                bi.StreamSource = ms;
                bi.EndInit();

                image.Source = bi;

                panel.Children.Add(image);

                ListViewItem item = new ListViewItem();
                item.HorizontalAlignment = HorizontalAlignment.Center;
                item.Content = panel;
                item.Tag = file;

                item.MouseMove += Item_MouseMove;

                ItemList.Items.Add(item);

            }
        }

        private void Item_MouseMove(object sender, MouseEventArgs e)
        {
            ListViewItem lvi = sender as ListViewItem;
            if (lvi != null && e.LeftButton == MouseButtonState.Pressed)
            {
                var dataObj = new DataObject();
                dataObj.SetData("Filename", lvi.Tag);
                DragDrop.DoDragDrop(lvi,
                                     dataObj,
                                     DragDropEffects.Copy);
            }
        }

        private void OpenDiskButton_Click(object sender, RoutedEventArgs e)
        {
            ItemList.Items.Clear();
            var drives = DriveInfo.GetDrives()
                               .Where(d => d.DriveType == DriveType.CDRom);
            if (drives.Count() != 0)
            {
                var drive = drives.First();
                OpenDirectory(drive.RootDirectory.FullName);
            }
        }

        private void OpenFileButton_Click(object sender, RoutedEventArgs e)
        {
            ItemList.Items.Clear();
            var dialog = new OpenFileDialog();
            dialog.Multiselect = true;
            if (dialog.ShowDialog() == true)
            {
                OpenFiles(dialog.FileNames.ToList());
            }
        }

        private void ImagesListTreeView_Drop(object sender, DragEventArgs e)
        {
            var item = e.Data.GetData("Filename") as string;

            var dicomfile = DicomFile.Open(item);

            var date = dicomfile.Dataset.Get<string>(DicomTag.StudyDate);

            var year = int.Parse(date.Substring(0, 4), CultureInfo.InvariantCulture);
            var month = int.Parse(date.Substring(4, 2), CultureInfo.InvariantCulture);
            var day = int.Parse(date.Substring(6, 2), CultureInfo.InvariantCulture);

            var date_ = new DateTime(year, month, day);

            var sdw = new ImageAddWindow(storageIO, date_);


            if (sdw.ShowDialog() == true)
            {
                if (sdw.PatientPickerBox.SelectedIndex > -1)
                {


                    var direction = (byte)sdw.DirectionPickerBox.SelectedIndex;
                    var state = (byte)sdw.StatePickerBox.SelectedIndex;

                    var id = (int)(sdw.PatientPickerBox.SelectedItem as ComboBoxItem).Tag;

                    var patient = storageIO.GetPatient(id);
                    var studies = storageIO.GetStudiesByPatient(patient);
                    var studiesDate = studies.Where(x => x.Date.Equals(sdw.DatePickerBox.SelectedDate));

                    Study std = null;

                    if (studiesDate.Count() == 0)
                    {
                        var study = new Study
                        {
                            Date = sdw.DatePickerBox.SelectedDate.Value,
                            PatientID = id
                        };
                        storageIO.InsertStudy(study);
                        var study_id = storageIO.GetLastInsertID();
                        std = storageIO.GetStudy(study_id);
                    }
                    else
                    {
                        std = studiesDate.First();
                    }

                    try
                    {
                        var dcmfile = DicomFile.Open(item);
                        var hash = "";
                        using (var md5 = MD5.Create())
                        {
                            using (var stream = File.OpenRead(item))
                            {
                                hash = Utils.RemoveCharFromStr(BitConverter.ToString(md5.ComputeHash(stream)), '-').ToLower();

                                if (!Directory.Exists(System.IO.Path.Combine("images", hash)))
                                {
                                    var dir = Directory.CreateDirectory(System.IO.Path.Combine("images", hash));
                                    File.Copy(item, System.IO.Path.Combine(dir.FullName, "image.dcm"));
                                }

                            }
                        }

                        var i = new SpineLib.DB.Image
                        {
                            Hash = hash,
                            StudyID = std.ID,
                            State = state
                        };
                        storageIO.InsertImage(i);

                        var spine_file = System.IO.Path.Combine("images", hash, "spines.xml");
                        var sp_st = SpineStorage.GenerateStorageForAddedFile(dcmfile);
                        sp_st.imageDirection = direction;
                        SpineReader.WriteToFile(sp_st, spine_file);

                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Данный файл не является DICOM-изображением", "Ошибка");
                        return;
                    }

                    RefreshTree();

                }
            }

        }

        private void CloseFileButton_Click(object sender, RoutedEventArgs e)
        {
            CloseFile();
        }

        private void CheckNeedSave()
        {
            SaveFileButton.IsEnabled = needSave;
        }

        private void myCanvas_MouseLeave(object sender, MouseEventArgs e)
        {
            if (leftMouseButtonDown)
            {
                leftMouseButtonDown = false;
            }
            else if (rightMouseButtonDown)
            {
                rightMouseButtonDown = false;
            }
            else if (middleMouseButtonDown)
            {
                middleMouseButtonDown = false;
            }
        }

        private void ChangeConstrastButton_Click(object sender, RoutedEventArgs e)
        {
            imageCenter = default_imageCenter;
            imageWidth = default_imageWidth;
            RefreshImage();
        }
        
        private void BasementMarkerPhysSizeClick(object sender, RoutedEventArgs e)
        {
            if (spines_storage != null)
            {
                var mwindow = new MarkerSizeWindow(spines_storage.MarkerSize);
                mwindow.ShowDialog();
                spines_storage.MarkerSize = mwindow.GetMarkerSize();
                needSave = true;
                CheckNeedSave();
            }
        }

        private void Select_Click(object sender, RoutedEventArgs e)
        {
            if (select_fl == false)
            {
                select_fl = true;
            }
            else
            {
                select_fl = false;
            }
        }
        private void MainViewport_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (select_fl == true)
            {
                System.Windows.Point mouse_pos = e.GetPosition(m_helix_viewport);
                HitTestResult result = VisualTreeHelper.HitTest(m_helix_viewport, mouse_pos);
                RayMeshGeometry3DHitTestResult mesh_result = result as RayMeshGeometry3DHitTestResult;
                if (mesh_result == null)
                {
                    //nothing
                }
                else
                {
                    for (int a = 0; a <= helix_i; a++)
                    {
                        if (mesh_result.ModelHit == mpath[a])
                        {
                            if (use_fl == true)
                            {
                                mpath[t].Material = buffermaterial;
                                mpath[t].BackMaterial = buffermaterial;
                            }
                            t = a;
                            a = helix_i;
                            use_fl = true;
                            buffermaterial = basematerial[t];
                            mpath[t].Material = new DiffuseMaterial(new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#ff81B1DB")));
                            mpath[t].BackMaterial = new DiffuseMaterial(new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#ff81B1DB")));
                        }
                    }
                }
            }
        }
        public void AddObjets(Material Mmaterial, Point3D point1, Point3D point2, string MODELPATH)
        {
            ModelImporter importer = new ModelImporter();
            MeshBuilder newMesh = new MeshBuilder();
            List<Point3D> paths = new List<Point3D>() { point1, point2 };
            bool loading = false;
            switch (type)
            {
                case "Cube":
                    newMesh.AddBox(point1, 10, 10, 10);
                    break;
                case "Sphere":
                    newMesh.AddSphere(point1, 10, 360, 100);
                    break;
                case "Cylinder":
                    newMesh.AddTube(paths, 10, 1000, true); ;
                    break;
                default:
                    {
                        loading = true;
                    }
                    break;
            }
            importer.DefaultMaterial = Mmaterial;
            if (loading == true)
            {
                var Model = importer.Load(MODELPATH);
                int a1 = Model.Children.Count;
                for (int a2 = 0; a2 < a1; a2++)
                {
                    mpath[helix_i] = Model.Children[a2] as GeometryModel3D;
                    Params(Mmaterial);
                }
            }
            else
            {
                mpath[helix_i] = new GeometryModel3D(newMesh.ToMesh(), Mmaterial);
                Params(Mmaterial);
            }

        }
        public void Params(Material Mmaterial)
        {
            demo.Children.Add(mpath[helix_i]);
            basematerial[helix_i] = Mmaterial;
            mx[helix_i] = my[helix_i] = mz[helix_i] = 0;
            sx[helix_i] = sy[helix_i] = sz[helix_i] = 1;
            rx[helix_i] = 0;
            bounds2 = mpath[helix_i].Bounds;
            cenX[helix_i] = bounds2.X + bounds2.SizeX / 2;
            cenY[helix_i] = bounds2.Y + bounds2.SizeY / 2;
            cenZ[helix_i] = bounds2.Z + bounds2.SizeZ / 2;
            helix_i++;
        }
        public void Add_Click(object sender, RoutedEventArgs e)
        {
            select_fl = false;
            type = "LoadModel";
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.DefaultExt = ".stl";
            openFileDialog.Filter = "STL and OBJ Files (*.stl, *.obj)|*.stl; *.obj";
            openFileDialog.InitialDirectory = @"c:\";
            Nullable<bool> result = openFileDialog.ShowDialog();
            if (result == true)
            {
                AddObjets(new DiffuseMaterial(new SolidColorBrush(Colors.Orange)), new Point3D(20, 20, 20), new Point3D(20, 40, 20), openFileDialog.FileName);
            }
        }
        public void AddPoints_Click(object sender, RoutedEventArgs e)
        {
            foreach (var mp in mpath)
            {
                demo.Children.Remove(mp);
            }
            //Array.Clear(mpath, 0, mpath.Length);
            string[] pozvons = new string[] { "L1_full.stl", "L2_full.stl", "L3_full.stl", "L4_full.stl", "L5_full.stl", "l1-l2.stl", "l2-l3.stl", "l3-l4.stl", "l4-l5.stl" };
            foreach (var pozvon in pozvons)
            {
                try
                {
                    var linepath = System.IO.Path.Combine(Environment.CurrentDirectory + "\\" + "images" + "\\" + tmphash + "\\" + pozvon);
                    AddObjets(new DiffuseMaterial(new SolidColorBrush(Colors.Beige)), new Point3D(20, 20, 20), new Point3D(20, 40, 20), linepath);
                }
                catch (Exception ex)
                {

                }
            }
        }
        public void LoadXMLClick(object sender, RoutedEventArgs e)
        {
            var filename = System.IO.Path.Combine("images", tmphash, "spines.xml");
            //var linepath = System.IO.Path.Combine(Environment.CurrentDirectory + "\\" + "images" + "\\" + tmphash + "\\" + spine.Key + ".xyz");
            LoadParams = new Parameter(XDocument.Load(filename));
            foreach (var spine in LoadParams.Spines)
            {
                FileStream file = new FileStream(Environment.CurrentDirectory + "\\" + "images" + "\\" + tmphash + "\\" + spine.Key + "_full.stl", FileMode.Create);
                StlFile stl = new StlFile
                {
                    SolidName = spine.Key,
                    Triangles = spine.Model.GetStlTriangles()
                };
                stl.Save(file, true);
                file.Close();

                FileStream fileu = new FileStream(Environment.CurrentDirectory + "\\" + "images" + "\\" + tmphash + "\\" + spine.Key + "_full_up.stl", FileMode.Create);
                StlFile stlu = new StlFile
                {
                    SolidName = spine.Key,
                    Triangles = spine.Model.GetStlTrianglesU(spine)
                };
                stlu.Save(fileu, true);
                fileu.Close();

                FileStream filed = new FileStream(Environment.CurrentDirectory + "\\" + "images" + "\\" + tmphash + "\\" + spine.Key + "_full_down.stl", FileMode.Create);
                StlFile stld = new StlFile
                {
                    SolidName = spine.Key,
                    Triangles = spine.Model.GetStlTrianglesD(spine)
                };
                stld.Save(filed, true);
                filed.Close();           
            }
            foreach (var spine in LoadParams.SpinesCut)
            {
                FileStream file = new FileStream(Environment.CurrentDirectory + "\\" + "images" + "\\" + tmphash + "\\" + spine.Key + ".stl", FileMode.Create);
                StlFile stl = new StlFile
                {
                    SolidName = spine.Key,
                    Triangles = spine.Model.GetStlTriangles()
                };
                stl.Save(file, true);
                file.Close();

                FileStream fileu = new FileStream(Environment.CurrentDirectory + "\\" + "images" + "\\" + tmphash + "\\" + spine.Key + "_up.stl", FileMode.Create);
                StlFile stlu = new StlFile
                {
                    SolidName = spine.Key,
                    Triangles = spine.Model.GetStlTrianglesU(spine)
                };
                stlu.Save(fileu, true);
                fileu.Close();

                FileStream filed = new FileStream(Environment.CurrentDirectory + "\\" + "images" + "\\" + tmphash + "\\" + spine.Key + "_down.stl", FileMode.Create);
                StlFile stld = new StlFile
                {
                    SolidName = spine.Key,
                    Triangles = spine.Model.GetStlTrianglesD(spine)
                };
                stld.Save(filed, true);
                filed.Close();
            }
            foreach (var mp in mpath)
            {
                demo.Children.Remove(mp);
            }
            string[] cutpozvons = new string[] { "L1.stl", "L2.stl", "L3.stl", "L4.stl", "L5.stl"};
            string[] cutpozvdown = new string[] { "L2_down.stl", "L3_down.stl", "L4_down.stl", "L5_down.stl" };
            string[] cutpozvup = new string[] { "L1_up.stl", "L2_up.stl", "L3_up.stl", "L4_up.stl" };
            string[] pozvons = new string[] { "L1_full.stl", "L2_full.stl", "L3_full.stl", "L4_full.stl", "L5_full.stl" };
            string[] pozvdown = new string[] { "L2_full_down.stl", "L3_full_down.stl", "L4_full_down.stl", "L5_full_down.stl" };
            string[] pozvup = new string[] { "L1_full_up.stl", "L2_full_up.stl", "L3_full_up.stl", "L4_full_up.stl" };
            string[] diskname = new string[] { "l1-l2.stl", "l2-l3.stl", "l3-l4.stl", "l4-l5.stl" };
            for (int i = 0; i < diskname.Length; i++)
            {
                try
                {
                    if (!File.Exists(Environment.CurrentDirectory + "\\" + "images" + "\\" + tmphash + "\\" + cutpozvons[i]))
                    {
                        throw new Exception();
                    }
                    if (i == 3)
                    {
                        if(!File.Exists(Environment.CurrentDirectory + "\\" + "images" + "\\" + tmphash + "\\" + cutpozvons[i]) || !File.Exists(Environment.CurrentDirectory + "\\" + "images" + "\\" + tmphash + "\\" + cutpozvons[i+1]))
                            throw new Exception();
                    }
                    var uppath = System.IO.Path.Combine(Environment.CurrentDirectory + "\\" + "images" + "\\" + tmphash + "\\" + cutpozvup[i]);
                    var downpath = System.IO.Path.Combine(Environment.CurrentDirectory + "\\" + "images" + "\\" + tmphash + "\\" + cutpozvdown[i]);
                    var diskpath = System.IO.Path.Combine(Environment.CurrentDirectory + "\\" + "images" + "\\" + tmphash + "\\" + diskname[i]);

                    Process process = new Process();
                    process.StartInfo.FileName = "VolotDiscReconstructor.exe";
                    process.StartInfo.Arguments = uppath + " " + downpath + " " + diskpath;
                    process.EnableRaisingEvents = true;
                    process.StartInfo.UseShellExecute = false;

                    // Говорим что нужно редиректить выходной поток  
                    process.StartInfo.RedirectStandardOutput = true;

                    // В StringBuilder будем добавлять полученные данные
                    //Запускаем процесс
                    process.Start();
                }
                catch (Exception ex)
                {

                }
            }
            MessageBox.Show(@"Готово!");
        }

        #region Trubenok_Calc
        private List<MeshGenerator.Elements.Triangle> ReadDisk(int firstVertNum, int secondVertNum, string basePath = "")
        {
            IRepository<string, List<MeshGenerator.Elements.Triangle>> repository = new StlTriangularRepository<string>();
            List<MeshGenerator.Elements.Triangle> triangles = repository.Read($"{basePath}l{firstVertNum}-l{secondVertNum}");

            return triangles;
        }
        private List<MeshGenerator.Elements.Triangle> ReadVertebra(int vertebraNum, string basePath = "")
        {
            IRepository<string, List<MeshGenerator.Elements.Triangle>> repository = new StlTriangularRepository<string>();
            List<MeshGenerator.Elements.Triangle> triangles = repository.Read($"{basePath}L{vertebraNum}");

            return triangles;
        }
        private void ShiftModel(ref List<List<MeshGenerator.Elements.Triangle>> stlModel, double shiftX, double shiftY, double shiftZ)
        {
            for (int i = 0; i < stlModel.Count; i++)
            {
                for (int j = 0; j < stlModel[i].Count; j++)
                {
                    for (int k = 0; k < stlModel[i][j].Nodes.Count; k++)
                    {
                        stlModel[i][j].Nodes[k].X += shiftX;
                        stlModel[i][j].Nodes[k].Y += shiftY;
                        stlModel[i][j].Nodes[k].Z += shiftZ;
                    }
                }
            }
        }
        private FeModel GenerateTetrahedralModel(double width, double height, double stepWidth, double stepHeight, int materialId)
        {
            IScene scene = new TetrahedralScene(width, height, stepWidth, stepHeight);
            scene.Initialize();

            FeModel feModel = new FeModel(scene.Nodes, new List<MeshGenerator.Elements.Triangle>(), scene.Tetrahedrons);

            foreach (var node in feModel.Nodes)
            {
                node.IdMaterial = materialId;
            }
            feModel.Tetrahedrons.AsParallel().ForAll(tn =>
            {
                tn.Nodes.ForEach(node => node.IdMaterial = materialId);
            });
            return feModel;
        }
        private FeModel GetGeneralModelFromScene(FeModel scene, List<List<MeshGenerator.Elements.Triangle>> vertebras, List<List<MeshGenerator.Elements.Triangle>> disks)
        {
            FeModel feModel = null;
            List<Node> nodes = new List<Node>();
            List<Tetrahedron> tetrahedrons = new List<Tetrahedron>();

            scene.Tetrahedrons.AsParallel().ForAll(tetrahedron =>
            {
                if (IsInsideStlArea(disks, tetrahedron))
                {
                    tetrahedron.Nodes.ForEach(nd => nd.IdMaterial = INNERVERTEBRAL_DISK_MATERIAL_ID);
                    tetrahedrons.Add(tetrahedron);
                }
                else if (IsInsideStlArea(vertebras, tetrahedron))
                {
                    tetrahedrons.Add(tetrahedron);
                }
            });
            scene.Nodes.ForEach(node =>
            {
                bool isHave = false;
                for (int i = 0; i < tetrahedrons.Count; i++)
                {
                    tetrahedrons[i].Nodes.ForEach(nd =>
                    {
                        if (nd.GlobalIndex == node.GlobalIndex)
                        {
                            nodes.Add(nd);
                            isHave = true;
                        }
                    });
                    if (isHave)
                    {
                        break;
                    }
                }
            });

            tetrahedrons.AsParallel().ForAll(tn =>
            {
                for (int i = 0; i < tn.Nodes.Count; i++)
                {
                    tn.Nodes[i] = nodes.FirstOrDefault(nd => nd.GlobalIndex == tn.Nodes[i].GlobalIndex);
                }
            });

            Parallel.For(0, nodes.Count, counter =>
            {
                nodes[counter].GlobalIndex = counter;
            });

            feModel = new FeModel(nodes, tetrahedrons);

            return feModel;
        }
        private bool IsInsideStlArea(List<List<MeshGenerator.Elements.Triangle>> stlModel, Tetrahedron tetrahedron)
        {
            List<MeshGenerator.Elements.Triangle> xy = new List<MeshGenerator.Elements.Triangle>();
            foreach (var ml in stlModel)
            {
                var trngls = ml.Where(trngl => trngl.IsInTriangleXY(tetrahedron.Center))
                    .OrderBy(trngl => trngl.Center.Z).ToList();
                if (trngls.Count % 2 == 0) xy.AddRange(trngls);
            }

            List<MeshGenerator.Elements.Triangle> xz = new List<MeshGenerator.Elements.Triangle>();
            foreach (var ml in stlModel)
            {
                var trngls = ml.Where(trngl => trngl.IsInTriangleXZ(tetrahedron.Center))
                    .OrderBy(trngl => trngl.Center.Y)
                    .ToList();
                if (trngls.Count % 2 == 0) xz.AddRange(trngls);
            }

            List<MeshGenerator.Elements.Triangle> yz = new List<MeshGenerator.Elements.Triangle>();
            foreach (var ml in stlModel)
            {
                var trngls = ml.Where(trngl => trngl.IsInTriangleYZ(tetrahedron.Center))
                    .OrderBy(trngl => trngl.Center.X)
                    .ToList();
                if (trngls.Count % 2 == 0) yz.AddRange(trngls);
            }
            xy.RemoveAll(trngl => trngl is null);
            xz.RemoveAll(trngl => trngl is null);
            yz.RemoveAll(trngl => trngl is null);

            return IsBetweenTriangles(xy, tetrahedron, Direction.Z)
                && IsBetweenTriangles(xz, tetrahedron, Direction.Y)
                && IsBetweenTriangles(yz, tetrahedron, Direction.X);
        }
        private bool IsBetweenTriangles(List<MeshGenerator.Elements.Triangle> triangles, Tetrahedron tetrahedron, Direction direction)
        {
            for (int i = 0; i < triangles.Count; i += 2)
            {
                switch (direction)
                {
                    case Direction.X:
                        if (tetrahedron.Center.X >= triangles[i].Center.X && tetrahedron.Center.X <= triangles[i + 1].Center.X)
                        {
                            return true;
                        }
                        break;
                    case Direction.Y:
                        if (tetrahedron.Center.Y >= triangles[i].Center.Y && tetrahedron.Center.Y <= triangles[i + 1].Center.Y)
                        {
                            return true;
                        }
                        break;
                    case Direction.Z:
                        if (tetrahedron.Center.Z >= triangles[i].Center.Z && tetrahedron.Center.Z <= triangles[i + 1].Center.Z)
                        {
                            return true;
                        }
                        break;
                    default:
                        throw new Exception("Wrong direction.");
                }
            }
            return false;
        }
        private int TrueIndexOfCenter(int currentIndex, List<Node> nearNodes, int currentNodeIndex)
        {
            bool isInTetrahedron = false;
            model.Tetrahedrons.AsParallel().ForAll(tetrahedron =>
            {
                tetrahedron.Nodes.ForEach(nd =>
                {
                    if (nd.GlobalIndex == currentIndex)
                    {
                        isInTetrahedron = true;
                    }
                });
            });
            if (!isInTetrahedron)
            {
                if (currentNodeIndex < nearNodes.Count)
                {
                    currentIndex = nearNodes[currentNodeIndex].GlobalIndex;
                    TrueIndexOfCenter(currentIndex, nearNodes, currentNodeIndex + 1);
                }
                else
                {
                    return -1;
                }
            }

            return currentIndex;
        }
        private void TotalEpure(List<Node> epureNodes, double[] results, string outFileName)
        {
            string workpath = Environment.CurrentDirectory + "\\" + "images" + "\\" + tmphash + "\\";
            using (StreamWriter writer = new StreamWriter(workpath + $"{outFileName}.txt"))
            {
                writer.WriteLine("{0, 5}|{1, 10}|{2, 10}|{3, 10}|{4, 10}|{5, 10}|{6, 10}", "Num", "X", "Y", "Z", "DX", "DY", "DZ");
                for (int i = 0; i < epureNodes.Count; i++)
                {
                    writer.Write("{0, 5}|{1, 10:f3}|{2, 10:f3}|{3, 10:f3}", i, epureNodes[i].X, epureNodes[i].Y, epureNodes[i].Z);
                    writer.WriteLine("|{0, 25:f20}|{1, 25:f20}|{2, 25:f20}|", results[i * DEGREES_OF_FREEDOM], results[i * DEGREES_OF_FREEDOM + 1], results[i * DEGREES_OF_FREEDOM + 2]);
                }
            }
        }


        public void MakeCalc_Click(object sender, RoutedEventArgs e)
        {
            string workpath = Environment.CurrentDirectory + "\\" + "images" + "\\" + tmphash + "\\";
            //Array.Clear(mpath, 0, mpath.Length);
            string[] pozvons = new string[] { "L1", "L2", "L3", "L4", "L5", "l1-l2", "l2-l3", "l3-l4", "l4-l5" };
            foreach (var pozvon in pozvons)
            {
                try
                {
                    if (!File.Exists(Environment.CurrentDirectory + "\\" + "images" + "\\" + tmphash + "\\" + pozvon + ".stl"))
                    {
                        throw new Exception();
                    }                   
                }
                catch (Exception ex)
                {
                }              
            }

            //trnglRepository = new StlTriangularRepository<string>();
            tetrahedralRepository = new StlTetrahedralRepository<string>();
            List<List<MeshGenerator.Elements.Triangle>> vertebras = new List<List<MeshGenerator.Elements.Triangle>>();
            List<List<MeshGenerator.Elements.Triangle>> disks = new List<List<MeshGenerator.Elements.Triangle>>();

            vertebras.Add(ReadVertebra(1, Environment.CurrentDirectory + "\\" + "images" + "\\" + tmphash + "\\"));
            int countVertebras = 5;
            for (int i = 2; i <= countVertebras; i++)
            {
                disks.Add(ReadDisk(i - 1, i, Environment.CurrentDirectory + "\\" + "images" + "\\" + tmphash + "\\"));
                vertebras.Add(ReadVertebra(i, Environment.CurrentDirectory + "\\" + "images" + "\\" + tmphash + "\\"));
            }
            List<MeshGenerator.Elements.Triangle> allVertebras = new List<MeshGenerator.Elements.Triangle>();
            vertebras.ForEach(trngls => allVertebras.AddRange(trngls));

            double shiftX = Math.Abs(allVertebras.Min(tngl => tngl.Center.X));
            double shiftY = Math.Abs(allVertebras.Min(tngl => tngl.Center.Y));
            double shiftZ = Math.Abs(allVertebras.Min(tngl => tngl.Center.Z));

            double minX = allVertebras.Min(tngl => tngl.Center.X);
            double minY = allVertebras.Min(tngl => tngl.Center.Y);
            double minZ = allVertebras.Min(tngl => tngl.Center.Z);
            double maxX = allVertebras.Max(tngl => tngl.Center.X);
            double maxY = allVertebras.Max(tngl => tngl.Center.Y);
            double maxZ = allVertebras.Max(tngl => tngl.Center.Z);
            double avX = (minX + maxX) / 2.0;
            double avY = (minY + maxY) / 2.0;
            double lenght = maxX - minX;
            double width = maxY - minY;
            width = (lenght > width) ? lenght : width;
            double height = maxZ - minZ;

            ShiftModel(ref vertebras, shiftX, shiftY, shiftZ);
            ShiftModel(ref disks, shiftX, shiftY, shiftZ);

            FeModel scene = GenerateTetrahedralModel(width, height + STEP_HEIGHT, STEP_WIDTH, STEP_HEIGHT, VERTEBRA_MATERIAL_ID);
            model = GetGeneralModelFromScene(scene, vertebras, disks);

            //load = new Force(SelectedSide.TOP, new Node((int)avX, (int)avY, maxZ - 10), forceValue, true, model.Triangles);
            //conditions = new VolumeBoundaryCondition(SelectedSide.BOTTOM, new Node((int)avX, (int)avY, minZ + 5), model.Triangles);

            load = new ConcentratedForce(SelectedSide.TOP, forceValue, true, model.Nodes, height / 10.0);
            conditions = new VolumeBoundaryCondition(SelectedSide.BOTTOM, model.Nodes, height / 20.0);

            int concentratedIndex = load.LoadVectors.FirstOrDefault().Key;
            int step = (int)(STEP_HEIGHT / 4.0);
            Node tmpNode = model.Nodes.FirstOrDefault(nd => nd.GlobalIndex == concentratedIndex);
            List<Node> nearNodes = new List<Node>(model.Nodes.Where(nd =>
                (nd.X > model.Nodes[concentratedIndex].X - step && nd.X < model.Nodes[concentratedIndex].X + step)
                && (nd.Y > model.Nodes[concentratedIndex].Y - step && nd.Y < model.Nodes[concentratedIndex].Y + step)
                && (nd.Z > model.Nodes[concentratedIndex].Z - step && nd.Z < model.Nodes[concentratedIndex].Z + step))
                .ToList());
            nearNodes.Remove(tmpNode);

            concentratedIndex = TrueIndexOfCenter(concentratedIndex, nearNodes, 0);
            if (concentratedIndex != load.LoadVectors.FirstOrDefault().Key)
            {
                LoadVector vector = new LoadVector(load.LoadVectors.FirstOrDefault().Value.Value, VectorDirection.Z);
                load.LoadVectors.Clear();
                load.LoadVectors.Add(concentratedIndex, vector);
            }

            tetrahedralRepository.Create(workpath + "Calc_File_" + "in", model.Tetrahedrons);
            //trnglRepository.Create(model.Id + "in", model.Triangles);
            //trnglRepository.Create(model.Id + "load", ((Force)load).LoadedTriangles);
            //trnglRepository.Create(model.Id + "fix", ((VolumeBoundaryCondition)conditions).FixedTriangles);
            MessageBox.Show(@"Сетка создана. Начинается вычисление. Дождитесь завершения.");
            
            solver = new StressStrainSparseMatrix(model);
            solution = new StaticMechanicSparseSolution(solver, model);

            var begin = DateTime.Now;
            solution.Solve(TypeOfFixation.RIGID, conditions, load);

            TimeSpan endSolve = DateTime.Now - begin;
            double[] results = solution.Results;

            using (StreamWriter writer = new StreamWriter(Environment.CurrentDirectory + "\\" + "images" + "\\" + tmphash + "\\" + "results.txt"))
            {
                writer.WriteLine($"Total time solving SLAE: {endSolve.TotalSeconds} sec.");
                writer.WriteLine();
                double max = solution.Results[2];
                for (int i = 2; i < solution.Results.Length; i += 3)
                {
                    if (Math.Abs(solution.Results[i]) > Math.Abs(max))
                    {
                        max = solution.Results[i];
                    }
                }
                writer.WriteLine($"Max deformation: {max} mm.");
                writer.WriteLine();
                for (int i = 0; i < solution.Results.Length; i++)
                {
                    writer.WriteLine(solution.Results[i]);
                }
            }

            TotalEpure(model.Nodes, solution.Results, "TotalEpureSpine");

            List<Tetrahedron> outList = ApplyResultsToTetrahedrons(results);
            tetrahedralRepository.Create(workpath + "Calc_File_" + "out", outList);
            tetrahedralRepository.Create2(workpath + "Calc_File_" + "out2", outList);

            MessageBox.Show($"Total time solving SLAE: {endSolve.TotalSeconds} sec.");
        }

        private List<Tetrahedron> ApplyResultsToTetrahedrons(double[] results)
        {
            string workpath = Environment.CurrentDirectory + "\\" + "images" + "\\" + tmphash + "\\";

            List<double> formax = new List<double>();
            List<Tetrahedron> list = new List<Tetrahedron>();
            StreamWriter wt1 = new StreamWriter(workpath + "test.color");
            double max1 = 0;

            foreach (var item in model.Tetrahedrons)
            {
                Tetrahedron tmp = new Tetrahedron(item);

                tmp.Nodes.ForEach(n =>
                {
                    n.X += results[n.GlobalIndex * 3];
                    n.Y += results[n.GlobalIndex * 3 + 1];
                    n.Z += results[n.GlobalIndex * 3 + 2];
                    double z = Math.Abs(solution.Results[n.GlobalIndex * 3 + 2]);
                    n.DefColor = z;
                    formax.Add(z);
                });
                list.Add(tmp);
            }
            max1 = formax.Max();
            wt1.WriteLine($"{max1}");
            wt1.Close();
            return list;
        }

        private FeModel ReadDisk(int firstVertNum, int secondVertNum, int diskMaterialId, string wpath)
        {
            repository = new GgenRepository<string>();

            List<Tetrahedron> tetrahedrons = repository.Read(wpath + $"l{firstVertNum}-l{secondVertNum}");
            List<Node> nodes = ((GgenRepository<string>)repository).Nodes.Values.ToList();
            List<MeshGenerator.Elements.Triangle> triangles = ((GgenRepository<string>)repository).Triangles;

            foreach (var node in nodes)
            {
                node.IdMaterial = diskMaterialId;
            }
            foreach (var tetrahedron in tetrahedrons)
            {
                foreach (var node in tetrahedron.Nodes)
                {
                    node.IdMaterial = diskMaterialId;
                }
            }

            return new FeModel(nodes, triangles, tetrahedrons);
        }

        #endregion

        public void OpenFolder_Click(object sender, RoutedEventArgs e)
        {
            string workpath = Environment.CurrentDirectory + "\\" + "images" + "\\" + tmphash + "\\";
            System.Diagnostics.Process.Start("explorer.exe", workpath);
        }

        public void ShowView_Click(object sender, RoutedEventArgs e)
        {
            ViewWindow subWindow = new ViewWindow(tmphash);
            subWindow.Tmphs = tmphash;
            subWindow.Show();
        }

    }
}
