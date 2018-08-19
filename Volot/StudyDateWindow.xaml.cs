using Dicom;
using Microsoft.Win32;
using SpineLib;
using SpineLib.DB;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Volot
{
    /// <summary>
    /// Interaction logic for ImageAddWindow.xaml
    /// </summary>
    public partial class ImageAddWindow : Window
    {


        public ImageAddWindow(StorageIO storage, DateTime date)
        {
            InitializeComponent();

            var patients = storage.GetPatients();

            DatePickerBox.SelectedDate = date;

            foreach (var patient in patients)
            {
                ComboBoxItem item = new ComboBoxItem();
                item.Tag = patient.ID;
                item.Content = patient.Surname + " " + patient.Name + " " + patient.Patronymic;
                PatientPickerBox.Items.Add(item);
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

       
    }
}
