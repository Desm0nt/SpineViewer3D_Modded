using SpineLib;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Volot
{
    /// <summary>
    /// Interaction logic for InstrumentWindow.xaml
    /// </summary>
    public partial class InstrumentWindow : Window
    {

        private readonly List<string> spine_points = new List<string>() { "Верхняя левая точка",
                                                                          "Нижняя левая точка",
                                                                          "Нижняя правая точка",
                                                                          "Верхняя правая точка", };

        private readonly List<string> process_points = new List<string>() {"Верхняя точка",
                                                                            "Угловая точка",
                                                                            "Нижняя точка"};


        private readonly List<string> marker_points;

        public InstrumentWindow(int imageDirection)
        {
            InitializeComponent();
            if (imageDirection == 0)
            {
                marker_points = new List<string>() { "Голова",
                                                        "Ноги",
                                                        "Лево",
                                                        "Право"};
            }
            else if (imageDirection == 1) {
                marker_points = new List<string>() { "Голова",
                                                        "Ноги",
                                                        "Живот",
                                                        "Спина"};
            }
        }

        private void TypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox box = sender as ComboBox;
            if (box.SelectedIndex == 0)
            {
                NameComboBox.Items.Clear();
                foreach (var item in SpineConstants.SpineNames)
                {
                    NameComboBox.Items.Add(item);
                }

                PointComboBox.Items.Clear();
                foreach (var item in spine_points)
                {
                    PointComboBox.Items.Add(item);
                }
            }
            else if (box.SelectedIndex == 1)
            {
                NameComboBox.Items.Clear();
                int count = SpineConstants.SpineNames.Count;
                for (int i = 0; i < count - 1; i++)
                {
                    var item = SpineConstants.SpineNames[i];
                    var item2 = SpineConstants.SpineNames[i + 1];
                    NameComboBox.Items.Add(SpineConstants.InterSpineNames[i]);
                }
                PointComboBox.Items.Clear();
                foreach (var item in process_points)
                {
                    PointComboBox.Items.Add(item);
                }
            }
            else if (box.SelectedIndex == 2)
            {
                NameComboBox.Items.Clear();
                PointComboBox.Items.Clear();
                foreach (var item in marker_points)
                {
                    PointComboBox.Items.Add(item);
                }
            }
        }

        private void NameComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PointComboBox.SelectedIndex = 0;
        }
    }
}
