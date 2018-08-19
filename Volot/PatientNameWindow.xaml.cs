using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace Volot
{
    /// <summary>
    /// Interaction logic for PatientNameWindow.xaml
    /// </summary>
    public partial class PatientNameWindow : Window
    {

        public PatientNameWindow()
        {
            InitializeComponent();
        }

        private void AgeTextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            e.Handled = Regex.IsMatch(e.Text, "[^0-9]+");
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
