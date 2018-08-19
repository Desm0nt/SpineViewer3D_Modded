using Dicom;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

namespace SpineLib
{
    public class DicomUtils
    {

        public static short[,] ExtractRawValues(DicomFile file)
        {
            short rescale_intercept = 0;
            short rescale_slope = 1;

            bool exception = false;
            try
            {
                rescale_slope = short.Parse(file.Dataset.Get<string>(DicomTag.RescaleSlope), CultureInfo.InvariantCulture);
            }
            catch (Exception)
            {
                exception = true;
            }

            try
            {
                rescale_intercept = short.Parse(file.Dataset.Get<string>(DicomTag.RescaleIntercept), CultureInfo.InvariantCulture);
            }
            catch (Exception)
            {

                exception = true;
            }

            ushort[] pixels = file.Dataset.Get<ushort[]>(DicomTag.PixelData);

            ushort width = file.Dataset.Get<ushort>(DicomTag.Columns);
            ushort height = file.Dataset.Get<ushort>(DicomTag.Rows);

            short[,] newpixels = new short[height, width];

            if (!exception)
            {
                Parallel.For(0, height, i =>
                {
                    for (int j = 0; j < width; j++)
                    {
                        newpixels[i, j] = (short)(pixels[i * width + j] * rescale_slope + rescale_intercept);
                    }
                }
                 );
            }
            else {
                Parallel.For(0, height, i =>
                {
                    for (int j = 0; j < width; j++)
                    {
                        newpixels[i, j] = (short)pixels[i * width + j];
                    }
                });
            }
            return newpixels;
            
        }

        public static Tuple<float, float> GetPixelPhysicalSize(DicomFile file)
        {
            float width = 1;
            float height = 1;
            try
            {
                var str = file.Dataset.Get<string>(DicomTag.PixelSpacing);
                var data = str.Split('\\');
                if (data.Length == 2)
                {
                    width = float.Parse(data[0], CultureInfo.InvariantCulture);
                    height = float.Parse(data[1], CultureInfo.InvariantCulture);
                }
                else
                {
                    width = height = float.Parse(data[0], CultureInfo.InvariantCulture);
                }
            }
            catch (Exception)
            {

            }


            return new Tuple<float, float>(width, height);
        }

        public static Tuple<short, short> GetWindowParameters(DicomFile file)
        {
            short width = 40;
            short center = 350;
            try
            {
                var str = file.Dataset.Get<string>(DicomTag.WindowWidth);
                var str1 = file.Dataset.Get<string>(DicomTag.WindowCenter);
                width = (short)float.Parse(str, CultureInfo.InvariantCulture);
                center = (short)float.Parse(str1, CultureInfo.InvariantCulture);
            }
            catch (Exception)
            {

            }


            return new Tuple<short, short>(width, center);
        }

        public static short[,] ChangeWindowWidthCenter(short[,] pixels, short width, short center)
        {
            short[,] newpixels = new short[pixels.GetLength(0), pixels.GetLength(1)];

            if (width < 1)
                width = 1;

            Parallel.For(0, newpixels.GetLength(0), i =>
            {
                for (int j = 0; j < pixels.GetLength(1); j++)
                {
                    var x = pixels[i, j];
                    if (x <= center - 0.5 - (width - 1) / 2) { newpixels[i, j] = 0; }
                    else if (x > center - 0.5 + (width - 1) / 2) { newpixels[i, j] = 255; }
                    else { newpixels[i, j] = (short)(((x - (center - 0.5)) / (width - 1) + 0.5) * (255 - 0) + 0); }
                }
            });

            return newpixels;
        }

        public static short[,] ChangeWindowWidthCenter(short[,] pixels, short[,] newpixels, short width, short center)
        {

            if (width < 1)
                width = 1;

            Parallel.For(0, newpixels.GetLength(0), i =>
            {
                for (int j = 0; j < pixels.GetLength(1); j++)
                {
                    var x = pixels[i, j];
                    if (x <= center - 0.5 - (width - 1) / 2) { newpixels[i, j] = 0; }
                    else if (x > center - 0.5 + (width - 1) / 2) { newpixels[i, j] = 255; }
                    else { newpixels[i, j] = (byte)(((x - (center - 0.5)) / (width - 1) + 0.5) * (255 - 0) + 0); }
                }
            });

            return newpixels;
        }

        public static Bitmap CreateBitmap(short[,] pixels)
        {
            int width = pixels.GetLength(1);
            int height = pixels.GetLength(0);
            Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb);

            try
            {
                BitmapData src = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
                unsafe
                {
                    byte* srcPointer = (byte*)src.Scan0;

                    for (int i = 0; i < height; i++)
                    {
                        for (int j = 0; j < width; j++)
                        {
                            srcPointer[0] = (byte)pixels[i, j];
                            srcPointer[1] = (byte)pixels[i, j];
                            srcPointer[2] = (byte)pixels[i, j];

                            srcPointer += 3;//srcPointer += BytePerPixel;

                        }
                        
                    }
                }
                bitmap.UnlockBits(src);
            }
            catch (InvalidOperationException e) {

            }

            return bitmap;
        }

        public static void ChangeBitmap(short[,] pixels, Bitmap bitmap) {
            int width = pixels.GetLength(1);
            int height = pixels.GetLength(0);

            if (width != bitmap.Width || height != bitmap.Height) {
                return;
            }         

            try
            {
                BitmapData src = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
                unsafe
                {
                    byte* srcPointer = (byte*)src.Scan0;

                    for (int i = 0; i < height; i++)
                    {
                        for (int j = 0; j < width; j++)
                        {
                            srcPointer[0] = (byte)pixels[i, j];
                            srcPointer[1] = (byte)pixels[i, j];
                            srcPointer[2] = (byte)pixels[i, j];

                            srcPointer += 3;//srcPointer += BytePerPixel;
                        }
                    }
                }
                bitmap.UnlockBits(src);
            }
            catch (InvalidOperationException e)
            {

            }
            
        }

        public static string GetDirectionParameter(DicomFile file)
        {
            string direction = "AP";
            try
            {
                direction = file.Dataset.Get<string>(DicomTag.ViewPosition);
               
            }
            catch (Exception)
            {

            }


            return direction;
        }

        public static List<string> GetDicomFilesFromDirectory(string directory) {
            if (Directory.Exists(directory)) {
                var list = new List<string>();
                var files = Directory.GetFiles(directory, "*.*", SearchOption.AllDirectories);

                foreach (var file in files) {
                    try
                    {

                        FileAttributes attr = File.GetAttributes(file);
                        if (!attr.HasFlag(FileAttributes.Directory)) { 
                            var dcm = DicomFile.Open(file);
                            var str = dcm.Dataset.Get<string>(DicomTag.PatientName); //hack fo check true dicom file
                            if (dcm != null && str != null)
                            {
                                list.Add(file);
                            }
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
                return list;
            }
            else
            {
                return null;
            }
        }

        public static List<string> FilterDicoms(List<string> dicoms)
        {
            var list = new List<string>();
            foreach (var file in dicoms)
            {
                try
                {

                    FileAttributes attr = File.GetAttributes(file);
                    if (!attr.HasFlag(FileAttributes.Directory))
                    {
                        var dcm = DicomFile.Open(file);
                        var str = dcm.Dataset.Get<string>(DicomTag.PatientName); //hack fo check true dicom file
                        if (dcm != null && str != null)
                        {
                            list.Add(file);
                        }
                    }
                }
                catch (Exception)
                {
                }
            }
            return list;
        }

        public static Bitmap CreateDefaultBitmap(DicomFile file) {
            var raw = ExtractRawValues(file);
            var wc = GetWindowParameters(file);
            var n = ChangeWindowWidthCenter(raw, wc.Item1, wc.Item2);
            return CreateBitmap(n);
        }

        public static Bitmap RotateImg(Bitmap bmp, float angle, Color bkColor)
        {
            angle = angle % 360;
            if (angle > 180)
                angle -= 360;

            PixelFormat pf = default(PixelFormat);
            if (bkColor == Color.Transparent)
            {
                pf = PixelFormat.Format32bppArgb;
            }
            else
            {
                pf = bmp.PixelFormat;
            }

            float sin = (float)Math.Abs(Math.Sin(angle * Math.PI / 180.0)); // this function takes radians
            float cos = (float)Math.Abs(Math.Cos(angle * Math.PI / 180.0)); // this one too
            float newImgWidth = sin * bmp.Height + cos * bmp.Width;
            float newImgHeight = sin * bmp.Width + cos * bmp.Height;
            float originX = 0f;
            float originY = 0f;

            if (angle > 0)
            {
                if (angle <= 90)
                    originX = sin * bmp.Height;
                else
                {
                    originX = newImgWidth;
                    originY = newImgHeight - sin * bmp.Width;
                }
            }
            else
            {
                if (angle >= -90)
                    originY = sin * bmp.Width;
                else
                {
                    originX = newImgWidth - sin * bmp.Height;
                    originY = newImgHeight;
                }
            }

            Bitmap newImg = new Bitmap((int)newImgWidth, (int)newImgHeight, pf);
            Graphics g = Graphics.FromImage(newImg);
            g.Clear(bkColor);
            g.TranslateTransform(originX, originY); // offset the origin to our calculated values
            g.RotateTransform(angle); // set up rotate
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBilinear;
            g.DrawImageUnscaled(bmp, 0, 0); // draw the image at 0, 0
            g.Dispose();

            return newImg;
        }

    }
}
