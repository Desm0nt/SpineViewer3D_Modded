using SpineLib;
using SpineLib.Geometry;
using SpineLib.Geometry.Descriptions;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Volot
{
    /// <summary>
    /// Interaction logic for ParametersWindow.xaml
    /// </summary>
    public partial class ParametersWindow : Window
    {
        private SpineStorage storage;
        
        public ParametersWindow(SpineStorage storage)
        {
            InitializeComponent();
            this.storage = storage;
        }


        private void Calculate()
        {
            if (storage != null)
            {
                #region Spines
                foreach (var key in storage.Keys)
                {
                    var spine = storage.GetDescription(key);

                    IDescriptionCalculator<SpineDescription> calc = null;

                    if (storage.direction == 0)
                    {
                        calc = new SpineLib.Geometry.DescriptionCalculators.Spine.LeftSide(spine);
                    }
                    else if (storage.direction == 1)
                    {
                        calc = new SpineLib.Geometry.DescriptionCalculators.Spine.RightSide(spine);
                    }
                    else if (storage.direction == 2)
                    {
                        calc = new SpineLib.Geometry.DescriptionCalculators.Spine.FrontSide(spine);
                    }
                    else if (storage.direction == 3)
                    {
                        calc = new SpineLib.Geometry.DescriptionCalculators.Spine.BackSide(spine);
                    }

                    if (SpinesDataGrid.Columns.Count == 0)
                    {
                        var ks = calc.Keys;
                        SpinesDataGrid.Columns.Add(new DataGridTextColumn() {
                            Header = "Позвонок",
                            Binding = new Binding("[0]"),
                        });
                        int index = 1;
                        foreach (var key_col in ks)
                        {
                            SpinesDataGrid.Columns.Add(
                                new DataGridTextColumn()
                                {
                                    Header = calc.GetParameterDescription(key_col),
                                    Binding = new Binding(string.Format("[{0}]", index++)),
                                });
                        }
                    }

                    List<string> obj = new List<string>();

                    obj.Add(key);
                    var keys = calc.Keys;
                    foreach (var key_col in keys)
                    {
                        var value = calc.GetParameter(key_col);
                        if (calc.IsParameterLinear(key_col)) {
                            value /= storage.MarkerLength;
                            value *= storage.MarkerSize;
                        }

                        obj.Add(string.Format("{0:0.000}", value));
                    }

                    SpinesDataGrid.Items.Add(obj);            
                }
                #endregion

                #region Interspines
                foreach (var key in storage.Keys)
                {
                    int keyind = SpineConstants.SpineNames.IndexOf(key);
                    if (keyind != SpineConstants.SpineNames.Count - 1)
                    {
                        var next = SpineConstants.SpineNames[(keyind + 1) % SpineConstants.SpineNames.Count];
                        if (storage.Keys.Contains(next))
                        {
                            var inter = new InterspineDescription();
                            inter.UpSpine = key;
                            inter.DownSpine = next;
                            inter.storage = storage;

                            IDescriptionCalculator<InterspineDescription> calc = null;

                            if (storage.direction == 0)
                            {
                                calc = new SpineLib.Geometry.DescriptionCalculators.Interspine.LeftSide(inter);
                            }
                            else if (storage.direction == 1)
                            {
                                calc = new SpineLib.Geometry.DescriptionCalculators.Interspine.RightSide(inter);
                            }
                            else if (storage.direction == 2)
                            {
                                calc = new SpineLib.Geometry.DescriptionCalculators.Interspine.FrontSide(inter);
                            }
                            else if (storage.direction == 3)
                            {
                                calc = new SpineLib.Geometry.DescriptionCalculators.Interspine.BackSide(inter);
                            }

                            if (InterspineDataGrid.Columns.Count == 0)
                            {
                                var ks = calc.Keys;
                                InterspineDataGrid.Columns.Add(new DataGridTextColumn()
                                {
                                    Header = "Диск",
                                    Binding = new Binding("[0]"),
                                });
                                int index = 1;
                                foreach (var key_col in ks)
                                {
                                    InterspineDataGrid.Columns.Add(
                                        new DataGridTextColumn()
                                        {
                                            Header = calc.GetParameterDescription(key_col),
                                            Binding = new Binding(string.Format("[{0}]", index++)),
                                        });
                                }
                            }

                            List<string> obj = new List<string>();

                            obj.Add(key + "-" + next);
                            var keys = calc.Keys;
                            foreach (var key_col in keys)
                            {
                                var value = calc.GetParameter(key_col);
                                if (calc.IsParameterLinear(key_col))
                                {
                                    value /= storage.MarkerLength;
                                    value *= storage.MarkerSize;
                                }

                                obj.Add(string.Format("{0:0.000}", value));
                            }

                            InterspineDataGrid.Items.Add(obj);
                        }
                    }
                }
                #endregion

                #region Processes
                foreach (var sp_key in storage.SpinousProcessKeys)
                {
                    IDescriptionCalculator<SpinousProcessDescription> calc = null;

                    var inter = storage.GetSpinousProcessDescription(sp_key);

                    if (storage.direction == 0)
                    {
                        calc = new SpineLib.Geometry.DescriptionCalculators.SpinousProcess.LeftSide(inter);
                        }
                    else if (storage.direction == 1)
                    {
                        calc = new SpineLib.Geometry.DescriptionCalculators.SpinousProcess.RightSide(inter);
                    }

                    if (ProcessDataGrid.Columns.Count == 0)
                    {
                        var ks = calc.Keys;
                        ProcessDataGrid.Columns.Add(new DataGridTextColumn()
                        {
                            Header = "Участок",
                            Binding = new Binding("[0]"),
                        });
                        int index = 1;
                        foreach (var key_col in ks)
                        {
                            ProcessDataGrid.Columns.Add(
                                new DataGridTextColumn()
                                {
                                    Header = calc.GetParameterDescription(key_col),
                                    Binding = new Binding(string.Format("[{0}]", index++)),
                                });
                        }
                    }

                    List<string> obj = new List<string>();

                    obj.Add(sp_key);
                    var keys = calc.Keys;
                    foreach (var key_col in keys)
                    {
                        var value = calc.GetParameter(key_col);
                        if (calc.IsParameterLinear(key_col))
                        {
                            value /= storage.MarkerLength;
                            value *= storage.MarkerSize;
                        }

                        obj.Add(string.Format("{0:0.000}", value));
                    }

                    ProcessDataGrid.Items.Add(obj);
                }
                #endregion
                
            }
        }

        private void SpinesDataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            Calculate();
        }
    }
}
