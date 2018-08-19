using Dicom;
using Microsoft.Win32;
using SpineLib;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SpineSelector
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        System.Windows.Point? First = null;
        System.Windows.Point? Second = null;

        Dictionary<int, System.Windows.Point> first = new Dictionary<int, System.Windows.Point>();
        Dictionary<int, System.Windows.Point> second = new Dictionary<int, System.Windows.Point>();
        Bitmap pixels;
        short[,] newpix;
        Bitmap rotpixels;

        Dictionary<int, System.Windows.Point> first_neg = new Dictionary<int, System.Windows.Point>();
        Dictionary<int, System.Windows.Point> second_neg = new Dictionary<int, System.Windows.Point>();

        string fname = "image";
            
        int index_neg = 0;
        int index = 0;

        private TransformGroup group = new TransformGroup();
        int angle = 0;


        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            if (openFileDialog.ShowDialog() == true) {
                var filename = openFileDialog.FileName;
                fname = Path.GetFileName(filename);

                var dicom = DicomFile.Open(filename);

                var raw = DicomUtils.ExtractRawValues(dicom);

                var wc = DicomUtils.GetWindowParameters(dicom);

                newpix = DicomUtils.ChangeWindowWidthCenter(raw, wc.Item1, wc.Item2);

                rotpixels = pixels = DicomUtils.CreateBitmap(newpix);

                angle = 0;
                group.Children.Add(new ScaleTransform(2, 2));
                myCanvas.LayoutTransform = group;


                //image.Source = this.imageSourceForImageControl(pixels); //image1 is your control

                MemoryStream ms = new MemoryStream();
                pixels.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                ms.Position = 0;
                BitmapImage bi = new BitmapImage();
                bi.BeginInit();
                bi.StreamSource = ms;
                bi.EndInit();

                image.Source = bi;


            }


        }

        private void image_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed
                && e.ClickCount == 2)
            {
                System.Windows.Point p = e.GetPosition(myCanvas);
                
                int x = (int)p.X;
                int y = (int)p.Y;

                if (First == null)
                {
                    First = p;
                }
                else if (Second == null) {
                    System.Windows.Shapes.Rectangle r = new System.Windows.Shapes.Rectangle();
                    r.Fill = System.Windows.Media.Brushes.Transparent;
                    r.Stroke = System.Windows.Media.Brushes.Green;
                    r.StrokeThickness = 3;
                    r.Width = x - First.Value.X;
                    r.Height = y - First.Value.Y;
                    r.Tag = index;
                    r.MouseDown += R_MouseDown;
                    Canvas.SetTop(r, First.Value.Y);
                    Canvas.SetLeft(r, First.Value.X);
                    first.Add(index, First.Value);
                    myCanvas.Children.Add(r);
                    second.Add(index, p);

                    index++;
                    First = null;
                }
            }
            else if (e.RightButton == System.Windows.Input.MouseButtonState.Pressed
             && e.ClickCount == 2)
            {
                System.Windows.Point p = e.GetPosition(myCanvas);

                int x = (int)p.X;
                int y = (int)p.Y;

                if (First == null)
                {
                    First = p;
                }
                else if (Second == null)
                {
                    System.Windows.Shapes.Rectangle r = new System.Windows.Shapes.Rectangle();
                    r.Fill = System.Windows.Media.Brushes.Transparent;
                    r.Stroke = System.Windows.Media.Brushes.Red;
                    r.StrokeThickness = 3;
                    r.Width = x - First.Value.X;
                    r.Height = y - First.Value.Y;
                    r.Tag = index_neg;
                    r.MouseDown += R_MouseDown_neg;
                    Canvas.SetTop(r, First.Value.Y);
                    Canvas.SetLeft(r, First.Value.X);
                    first_neg.Add(index_neg, First.Value);
                    myCanvas.Children.Add(r);
                    second_neg.Add(index_neg, p);

                    index_neg++;
                    First = null;
                }
            }


        }

        private void R_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.MiddleButton == System.Windows.Input.MouseButtonState.Pressed) {
                int index = (int)((sender as System.Windows.Shapes.Rectangle).Tag);
                myCanvas.Children.Remove(sender as System.Windows.Shapes.Rectangle);
                first.Remove(index);
                second.Remove(index);
                (sender as System.Windows.Shapes.Rectangle).MouseDown -= R_MouseDown;
            }
        }

        private void R_MouseDown_neg(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.MiddleButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                int index_neg = (int)((sender as System.Windows.Shapes.Rectangle).Tag);
                myCanvas.Children.Remove(sender as System.Windows.Shapes.Rectangle);
                first_neg.Remove(index_neg);
                second_neg.Remove(index_neg);
                (sender as System.Windows.Shapes.Rectangle).MouseDown -= R_MouseDown_neg;
            }
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            
            foreach (var i in first.Keys)
            {
                var pf = first[i];
                var ps = second[i];

                int width = (int)(ps.X - pf.X);
                int height = (int)(ps.Y - pf.Y);

                Rectangle rect = new Rectangle((int)pf.X, (int)pf.Y, width, height);

                Bitmap bmap = rotpixels.Clone(rect, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

                bmap.Save(fname +"_" + i + ".png", System.Drawing.Imaging.ImageFormat.Png);

            }
            
            foreach (var i in first_neg.Keys)
            {
                var pf = first_neg[i];
                var ps = second_neg[i];

                int width = (int)(ps.X - pf.X);
                int height = (int)(ps.Y - pf.Y);

                Rectangle rect = new Rectangle((int)pf.X, (int)pf.Y, width, height);

                Bitmap bmap = rotpixels.Clone(rect, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

                bmap.Save(fname + "_" + i + "neg.png", System.Drawing.Imaging.ImageFormat.Png);

            }



        }

        private void CW_Click(object sender, RoutedEventArgs e)
        {
            angle += 90;
            RenewBitmap();
        }

        private void CCW_Click(object sender, RoutedEventArgs e)
        {
            angle -= 90;
            RenewBitmap();
        }

        public void RenewBitmap() {
            pixels = DicomUtils.CreateBitmap(newpix);


            var bmp = DicomUtils.RotateImg(pixels, angle, System.Drawing.Color.Black);
            rotpixels = bmp;

            MemoryStream ms = new MemoryStream();
            bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            ms.Position = 0;
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.StreamSource = ms;
            bi.EndInit();

            image.Source = bi;
        }

    }
}
