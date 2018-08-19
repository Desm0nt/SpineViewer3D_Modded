using Microsoft.Win32;
using SpineLib;
using SpineLib.DB;
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
    /// Interaction logic for ParametersResultWindow.xaml
    /// </summary>
    public partial class ParametersResultWindow : Window
    {
        private SpineStorage storage_lay;
        private SpineStorage storage_stay;
        private DateTime studyDate;
        private Patient patient;

        public ParametersResultWindow(SpineStorage firstStorage, 
                                      SpineLib.DB.Image firstImage,
                                      SpineStorage secondStorage,
                                      SpineLib.DB.Image secondImage,
                                      DateTime studyDate,
                                      Patient patient)
        {
            InitializeComponent();
            if (firstImage.State == 0 && secondImage.State == 1)
            {
                storage_lay = firstStorage;
                storage_stay = secondStorage;
            }
            else if (firstImage.State == 1 && secondImage.State == 0)
            {
                storage_lay = secondStorage;
                storage_stay = firstStorage;
            }
            else {
                MessageBox.Show("Снимки должны быть в разных положениях (стоя и лежа)", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
            this.studyDate = studyDate;
            this.patient = patient;
        }


        private void Calculate()
        {
            if (storage_lay != null && storage_stay != null)
            {
                #region Spines
                foreach (var key in storage_lay.Keys)
                {
                    if (storage_stay.ContainDescription(key))
                    {
                        var spine_first = storage_lay.GetDescription(key);
                        var spine_second = storage_stay.GetDescription(key);

                        IDescriptionCalculator<SpineDescription> calc_first = null;
                        IDescriptionCalculator<SpineDescription> calc_second = null;

                        if (storage_lay.direction == 0)
                        {
                            calc_first = new SpineLib.Geometry.DescriptionCalculators.Spine.LeftSide(spine_first);
                        }
                        else if (storage_lay.direction == 1)
                        {
                            calc_first = new SpineLib.Geometry.DescriptionCalculators.Spine.RightSide(spine_first);
                        }
                        else if (storage_lay.direction == 2)
                        {
                            calc_first = new SpineLib.Geometry.DescriptionCalculators.Spine.FrontSide(spine_first);
                        }
                        else if (storage_lay.direction == 3)
                        {
                            calc_first = new SpineLib.Geometry.DescriptionCalculators.Spine.BackSide(spine_first);
                        }

                        if (storage_stay.direction == 0)
                        {
                            calc_second = new SpineLib.Geometry.DescriptionCalculators.Spine.LeftSide(spine_second);
                        }
                        else if (storage_stay.direction == 1)
                        {
                            calc_second = new SpineLib.Geometry.DescriptionCalculators.Spine.RightSide(spine_second);
                        }
                        else if (storage_stay.direction == 2)
                        {
                            calc_second = new SpineLib.Geometry.DescriptionCalculators.Spine.FrontSide(spine_second);
                        }
                        else if (storage_stay.direction == 3)
                        {
                            calc_second = new SpineLib.Geometry.DescriptionCalculators.Spine.BackSide(spine_second);
                        }

                        if (SpinesDataGrid.Columns.Count == 0)
                        {
                            SpinesDataGrid.Columns.Add(new DataGridTextColumn()
                            {
                                Header = "Название",
                                Binding = new Binding("[0]"),
                            });

                            SpinesDataGrid.Columns.Add(new DataGridTextColumn()
                            {
                                Header = "Лежа",
                                Binding = new Binding("[1]"),
                            });

                            SpinesDataGrid.Columns.Add(new DataGridTextColumn()
                            {
                                Header = "Стоя",
                                Binding = new Binding("[2]"),
                            });

                            SpinesDataGrid.Columns.Add(new DataGridTextColumn()
                            {
                                Header = "Сравнение",
                                Binding = new Binding("[3]"),
                            });
                        }

                        var ks = calc_first.Keys;


                        List<string> obj = new List<string>();
                        obj.Add("");
                        obj.Add(key);
                        obj.Add(key);
                        obj.Add("");

                        SpinesDataGrid.Items.Add(obj);

                        foreach (var key_col in ks)
                        {
                            obj = new List<string>();
                            var desc = calc_first.GetParameterDescription(key_col);
                            var var_first = calc_first.GetParameter(key_col);
                            if (calc_first.IsParameterLinear(key_col)) {
                                var_first /= storage_lay.MarkerLength;
                                var_first *= storage_lay.MarkerSize;
                            }
                            var var_second = calc_second.GetParameter(key_col);
                            if (calc_second.IsParameterLinear(key_col))
                            {
                                var_second /= storage_stay.MarkerLength;
                                var_second *= storage_stay.MarkerSize;
                            }
                            obj.Add(desc);
                            obj.Add(string.Format("{0:0.000}", var_first));
                            obj.Add(string.Format("{0:0.000}", var_second));

                            if (calc_first.IsParameterLinear(key_col) && calc_second.IsParameterLinear(key_col))
                            {
                                var diff = Math.Abs((var_first - var_second) / var_first) * 100;

                                if (var_first < var_second)
                                {
                                    obj.Add(string.Format("Увеличение на {0} %", string.Format("{0:0.000}", diff)));
                                }
                                else
                                {
                                    obj.Add(string.Format("Уменьшение на {0} %", string.Format("{0:0.000}", diff)));
                                }
                            }
                            else {
                                if (var_first < var_second)
                                {
                                    obj.Add(string.Format("Увеличение на {0}", string.Format("{0:0.000}", var_second - var_first)));
                                }
                                else
                                {
                                    obj.Add(string.Format("Уменьшение на {0}", string.Format("{0:0.000}", var_first - var_second)));
                                }
                            }

                            SpinesDataGrid.Items.Add(obj);
                        }




                    }

                }
                #endregion

                #region Interspines
                foreach (var key in storage_lay.Keys)
                {
                    InterspineDescription inter_first = null;
                    InterspineDescription inter_second = null;

                    int keyind = SpineConstants.SpineNames.IndexOf(key);
                    if (keyind != SpineConstants.SpineNames.Count - 1)
                    {
                        var next = SpineConstants.SpineNames[(keyind + 1) % SpineConstants.SpineNames.Count];
                        if (storage_lay.Keys.Contains(next))
                        {
                            inter_first = new InterspineDescription();
                            inter_first.UpSpine = key;
                            inter_first.DownSpine = next;
                            inter_first.storage = storage_lay;


                            if (storage_stay.Keys.Contains(next) && storage_stay.Keys.Contains(key))
                            {
                                inter_second = new InterspineDescription();
                                inter_second.UpSpine = key;
                                inter_second.DownSpine = next;
                                inter_second.storage = storage_stay;

                                IDescriptionCalculator<InterspineDescription> calc_first_ = null;
                                IDescriptionCalculator<InterspineDescription> calc_second_ = null;

                                if (storage_lay.direction == 0)
                                {
                                    calc_first_ = new SpineLib.Geometry.DescriptionCalculators.Interspine.LeftSide(inter_first);
                                }
                                else if (storage_lay.direction == 1)
                                {
                                    calc_first_ = new SpineLib.Geometry.DescriptionCalculators.Interspine.RightSide(inter_first);
                                }
                                else if (storage_lay.direction == 2)
                                {
                                    calc_first_ = new SpineLib.Geometry.DescriptionCalculators.Interspine.FrontSide(inter_first);
                                }
                                else if (storage_lay.direction == 3)
                                {
                                    calc_first_ = new SpineLib.Geometry.DescriptionCalculators.Interspine.BackSide(inter_first);
                                }

                                if (storage_stay.direction == 0)
                                {
                                    calc_second_ = new SpineLib.Geometry.DescriptionCalculators.Interspine.LeftSide(inter_second);
                                }
                                else if (storage_stay.direction == 1)
                                {
                                    calc_second_ = new SpineLib.Geometry.DescriptionCalculators.Interspine.RightSide(inter_second);
                                }
                                else if (storage_stay.direction == 2)
                                {
                                    calc_second_ = new SpineLib.Geometry.DescriptionCalculators.Interspine.FrontSide(inter_second);
                                }
                                else if (storage_stay.direction == 3)
                                {
                                    calc_second_ = new SpineLib.Geometry.DescriptionCalculators.Interspine.BackSide(inter_second);
                                }

                                if (InterspineDataGrid.Columns.Count == 0)
                                {
                                    InterspineDataGrid.Columns.Add(new DataGridTextColumn()
                                    {
                                        Header = "Название",
                                        Binding = new Binding("[0]"),
                                    });

                                    InterspineDataGrid.Columns.Add(new DataGridTextColumn()
                                    {
                                        Header = "Лежа",
                                        Binding = new Binding("[1]"),
                                    });

                                    InterspineDataGrid.Columns.Add(new DataGridTextColumn()
                                    {
                                        Header = "Стоя",
                                        Binding = new Binding("[2]"),
                                    });

                                    InterspineDataGrid.Columns.Add(new DataGridTextColumn()
                                    {
                                        Header = "Сравнение",
                                        Binding = new Binding("[3]"),
                                    });
                                }

                                var ks = calc_first_.Keys;


                                List<string> obj = new List<string>();
                                obj.Add("");
                                obj.Add(SpineConstants.InterSpineNames[keyind]);
                                obj.Add(SpineConstants.InterSpineNames[keyind]);
                                obj.Add("");

                                InterspineDataGrid.Items.Add(obj);

                                foreach (var key_col in ks)
                                {
                                    obj = new List<string>();
                                    var desc = calc_first_.GetParameterDescription(key_col);
                                    var var_first = calc_first_.GetParameter(key_col);
                                    if (calc_first_.IsParameterLinear(key_col))
                                    {
                                        var_first /= storage_lay.MarkerLength;
                                        var_first *= storage_lay.MarkerSize;
                                    }
                                    var var_second = calc_second_.GetParameter(key_col);
                                    if (calc_second_.IsParameterLinear(key_col))
                                    {
                                        var_second /= storage_stay.MarkerLength;
                                        var_second *= storage_stay.MarkerSize;
                                    }

                                    obj.Add(desc);
                                    obj.Add(string.Format("{0:0.000}", var_first));
                                    obj.Add(string.Format("{0:0.000}", var_second));

                                    if (calc_first_.IsParameterLinear(key_col) && calc_second_.IsParameterLinear(key_col))
                                    {
                                        var diff = Math.Abs((var_first - var_second) / var_first) * 100;

                                        if (var_first < var_second)
                                        {
                                            obj.Add(string.Format("Увеличение на {0} %", string.Format("{0:0.000}", diff)));
                                        }
                                        else
                                        {
                                            obj.Add(string.Format("Уменьшение на {0} %", string.Format("{0:0.000}", diff)));
                                        }
                                    }
                                    else
                                    {
                                        if (var_first < var_second)
                                        {
                                            obj.Add(string.Format("Увеличение на {0}", string.Format("{0:0.000}", var_second - var_first)));
                                        }
                                        else
                                        {
                                            obj.Add(string.Format("Уменьшение на {0}", string.Format("{0:0.000}", var_first - var_second)));
                                        }
                                    }

                                    InterspineDataGrid.Items.Add(obj);
                                }
                            }
                        }
                    }


                }

            }
            #endregion

                #region Processes
                foreach (var sp_key in storage_lay.SpinousProcessKeys)
                {
                    if (storage_stay.ContainSpinousProcessDescription(sp_key))
                    {
                        IDescriptionCalculator<SpinousProcessDescription> calc_first__ = null;
                        IDescriptionCalculator<SpinousProcessDescription> calc_second__ = null;

                        var desc_first = storage_lay.GetSpinousProcessDescription(sp_key);
                        var desc_second = storage_stay.GetSpinousProcessDescription(sp_key);

                        if (storage_lay.direction == 0)
                        {
                            calc_first__ = new SpineLib.Geometry.DescriptionCalculators.SpinousProcess.LeftSide(desc_first);
                        }
                        else if (storage_lay.direction == 1)
                        {

                            calc_first__ = new SpineLib.Geometry.DescriptionCalculators.SpinousProcess.RightSide(desc_first);
                        }

                        if (storage_stay.direction == 0)
                        {
                            calc_second__ = new SpineLib.Geometry.DescriptionCalculators.SpinousProcess.LeftSide(desc_second);
                            }
                        else if (storage_stay.direction == 1)
                        {
                            calc_second__ = new SpineLib.Geometry.DescriptionCalculators.SpinousProcess.RightSide(desc_second);
                        }


                        if (ProcessDataGrid.Columns.Count == 0)
                        {
                            ProcessDataGrid.Columns.Add(new DataGridTextColumn()
                            {
                                Header = "Название",
                                Binding = new Binding("[0]"),
                            });

                            ProcessDataGrid.Columns.Add(new DataGridTextColumn()
                            {
                                Header = "Лежа",
                                Binding = new Binding("[1]"),
                            });

                            ProcessDataGrid.Columns.Add(new DataGridTextColumn()
                            {
                                Header = "Стоя",
                                Binding = new Binding("[2]"),
                            });

                            ProcessDataGrid.Columns.Add(new DataGridTextColumn()
                            {
                                Header = "Сравнение",
                                Binding = new Binding("[3]"),
                            });
                        }

                        var ks = calc_first__.Keys;


                        List<string> obj = new List<string>();
                        obj.Add("");
                        obj.Add(sp_key);
                        obj.Add(sp_key);
                        obj.Add("");

                        ProcessDataGrid.Items.Add(obj);

                        foreach (var key_col in ks)
                        {
                            obj = new List<string>();
                            var desc = calc_first__.GetParameterDescription(key_col);
                            var var_first = calc_first__.GetParameter(key_col);
                            if (calc_first__.IsParameterLinear(key_col))
                            {
                                var_first /= storage_lay.MarkerLength;
                                var_first *= storage_lay.MarkerSize;
                            }
                            var var_second = calc_second__.GetParameter(key_col);
                            if (calc_second__.IsParameterLinear(key_col))
                            {
                                var_second /= storage_stay.MarkerLength;
                                var_second *= storage_stay.MarkerSize;
                            }

                            obj.Add(desc);
                            obj.Add(string.Format("{0:0.000}", var_first));
                            obj.Add(string.Format("{0:0.000}", var_second));

                            if (calc_first__.IsParameterLinear(key_col) && calc_second__.IsParameterLinear(key_col))
                            {
                                var diff = Math.Abs((var_first - var_second) / var_first) * 100;

                                if (var_first < var_second)
                                {
                                    obj.Add(string.Format("Увеличение на {0} %", string.Format("{0:0.000}", diff)));
                                }
                                else
                                {
                                    obj.Add(string.Format("Уменьшение на {0} %", string.Format("{0:0.000}", diff)));
                                }
                            }
                            else
                            {
                                if (var_first < var_second)
                                {
                                    obj.Add(string.Format("Увеличение на {0}", string.Format("{0:0.000}", var_second - var_first)));
                                }
                                else
                                {
                                    obj.Add(string.Format("Уменьшение на {0}", string.Format("{0:0.000}", var_first - var_second)));
                                }
                            }

                        ProcessDataGrid.Items.Add(obj);
                        }

                    }

                }
                #endregion
    
        } 
       
        private void SpinesDataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            Calculate();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (storage_lay != null && storage_stay != null)
            {
                SaveFileDialog dialog = new SaveFileDialog();
                dialog.Filter = "Excel файл (*.xls)|*.xls";
                dialog.FilterIndex = 1;
                dialog.RestoreDirectory = true;

                bool success = true;

                if (dialog.ShowDialog() == true)
                {
                    try
                    {
                        var filename = dialog.FileName;

                        DataExporter.ExportToXLSCompare(filename, storage_lay, storage_stay);
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

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (storage_lay != null && storage_stay != null)
            {
                SaveFileDialog dialog = new SaveFileDialog();
                dialog.Filter = "Word файл (*.docx)|*.docx";
                dialog.FilterIndex = 1;
                dialog.RestoreDirectory = true;

                bool success = true;

                if (dialog.ShowDialog() == true)
                {
                    try
                    {
                        var filename = dialog.FileName;
                        
                        DataExporter.ExportToDOCCompare(filename,
                            patient,
                            studyDate.ToShortDateString(),
                            storage_lay, storage_stay);
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
    }
}
