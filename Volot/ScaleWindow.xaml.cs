using System.Windows;

namespace Volot
{
    /// <summary>
    /// Interaction logic for ScaleWindow.xaml
    /// </summary>
    public partial class ScaleWindow : Window
    {
        private int scale;
        public ScaleWindow(int scale)
        {
            InitializeComponent();
            this.scale = scale;
            scaleEdit.Text = scale.ToString();
        }

        public float GetScale() {

            bool parsed;
            int result;

            parsed = int.TryParse(scaleEdit.Text, out result);

            if (parsed)
            {
                if (result <= 0)
                {
                    return 1.0f;
                }
                else {
                    return 1.0f * result / 100;
                }
            }
            else {
                return 1.0f;
            }
        }
    }
}
