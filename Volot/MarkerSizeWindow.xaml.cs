using System.Globalization;
using System.Windows;

namespace Volot
{
    /// <summary>
    /// Interaction logic for MarkerSizeWindow.xaml
    /// </summary>
    public partial class MarkerSizeWindow : Window
    {
        private double clipHeight;
        public MarkerSizeWindow(double clipHeight)
        {
            InitializeComponent();
            this.clipHeight = clipHeight;
            clipHeightEdit.Text = clipHeight.ToString();
        }

        public double GetMarkerSize()
        {
            return double.Parse(clipHeightEdit.Text, CultureInfo.InvariantCulture);
        }
    }
}
