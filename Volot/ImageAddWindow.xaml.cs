using Dicom;
using Microsoft.Win32;
using SpineLib;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Volot
{
    /// <summary>
    /// Interaction logic for StudyDateWindow.xaml
    /// </summary>
    public partial class StudyDateWindow : Window
    {
        private string firstDirection = null;
        private string secondDirection = null;

        public string firstFilename = null;
        public string secondFilename = null;

        public StudyDateWindow()
        {
            InitializeComponent();
            CheckOkAvailability();
        }

        public void CheckOkAvailability() {
            if (firstFilename == null || secondFilename == null)
            {
                OkButton.IsEnabled = false;
                OkButton.ToolTip = new Label() { Content = "Добавлены не все картинки" };
            }
            else {
                //if (firstDirection != null && secondDirection != null) {
                    //if (firstDirection.Equals(secondDirection))
                    //{
                        OkButton.IsEnabled = true;
                        OkButton.ToolTip = null;
                    //}
                    //else {
                    //    OkButton.IsEnabled = false;
                    //    OkButton.ToolTip = new Label() { Content = "Невозможно добавить картинки, так как они сняты с разных направлений" };
                    //}
            //    }
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void LayImageButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();

            bool trueFile = true;

            if (dialog.ShowDialog() == true)
            {
                var filename = dialog.FileName;

                try
                {
                    var dcmfile = DicomFile.Open(filename);
                    firstFilename = filename;
                    firstDirection = DicomUtils.GetDirectionParameter(dcmfile);
                }
                catch (Exception)
                {
                    trueFile = false;
                }
            }

            if (!trueFile)
            {
                firstFilename = null;
                firstDirection = null;
            }
            else {
                LayImageLabel.Content = System.IO.Path.GetFileName(firstFilename);
            }
            CheckOkAvailability();
        }

        private void StayImageButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();

            bool trueFile = true;

            if (dialog.ShowDialog() == true)
            {
                var filename = dialog.FileName;

                try
                {
                    var dcmfile = DicomFile.Open(filename);
                    secondFilename = filename;
                    secondDirection = DicomUtils.GetDirectionParameter(dcmfile);
                }
                catch (Exception)
                {
                    trueFile = false;
                }
            }

            if (!trueFile)
            {
                secondFilename = null;
                secondDirection = null;
            }
            else
            {
                StayImageLabel.Content = System.IO.Path.GetFileName(secondFilename);
            }
            CheckOkAvailability();
        }
    }
}
