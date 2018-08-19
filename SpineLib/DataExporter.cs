using SpineLib.Geometry;
using SpineLib.Geometry.Descriptions;
using System.IO;
using System.Xml.Linq;
using NPOI.SS.UserModel;
using NPOI.HSSF.UserModel;
using System;
using Novacode;
using System.Linq;
using System.Globalization;
using System.Collections.Generic;
using SpineLib.DB;

namespace SpineLib
{
    public class DataExporter
    {
        public static void ExportToXML(string filename, SpineStorage storage) {
            var keys = storage.Keys;

            XDocument doc = new XDocument();

            doc.Add(new XElement("storage"));

            var root = doc.Element("storage");

            XElement spines = new XElement("spines");

            foreach (var key in keys)
            {
                var state = storage.GetDescription(key);


                IDescriptionCalculator<SpineDescription> calc = null;

                if (storage.direction == 0)
                {
                    calc = new Geometry.DescriptionCalculators.Spine.LeftSide(state);
                }
                else if (storage.direction == 1)
                {
                    calc = new Geometry.DescriptionCalculators.Spine.RightSide(state);
                }
                else if (storage.direction == 2)
                {
                    calc = new Geometry.DescriptionCalculators.Spine.FrontSide(state);
                }
                else if (storage.direction == 3)
                {
                    calc = new Geometry.DescriptionCalculators.Spine.BackSide(state);
                }



                var element = new XElement("spine");
                var attr = new XAttribute("key", key);
                element.Add(attr);

                var points_element = new XElement("points");

                element.Add(points_element);

                var geom_element = new XElement("geomertry");

                var ks = calc.Keys;

                foreach (var key_param in ks)
                {
                    var geom_node = new XElement("param");
                    attr = new XAttribute("key", key_param);
                    geom_node.Add(attr);
                    attr = new XAttribute("value", string.Format("{0:0.000}", calc.GetParameter(key_param)));
                    geom_node.Add(attr);
                    geom_element.Add(geom_node);
                }

                element.Add(geom_element);

                spines.Add(element);
            }

            root.Add(spines);

            XElement disks = new XElement("disks");

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
                            calc = new Geometry.DescriptionCalculators.Interspine.LeftSide(inter);
                        }
                        else if (storage.direction == 1)
                        {
                            calc = new Geometry.DescriptionCalculators.Interspine.RightSide(inter);
                        }
                        else if (storage.direction == 2)
                        {
                            calc = new Geometry.DescriptionCalculators.Interspine.FrontSide(inter);
                        }
                        else if (storage.direction == 3)
                        {
                            calc = new Geometry.DescriptionCalculators.Interspine.BackSide(inter);
                        }

                        var ks = calc.Keys;

                        var disk_node = new XElement("disk");

                        var attr = new XAttribute("key", SpineConstants.InterSpineNames[keyind]);
                        disk_node.Add(attr);

                        foreach (var key_param in ks)
                        {
                            var param_node = new XElement("param");
                            attr = new XAttribute("key", key_param);
                            param_node.Add(attr);
                            attr = new XAttribute("value", string.Format("{0:0.000}", calc.GetParameter(key_param)));
                            param_node.Add(attr);
                            disk_node.Add(param_node);
                        }


                        disks.Add(disk_node);
                    }
                }
            }

            root.Add(disks);

            XElement processes = new XElement("processes");

            foreach (var key in storage.SpinousProcessKeys)
            {
                var state = storage.GetSpinousProcessDescription(key);

                var element = new XElement("process");
                var attr = new XAttribute("key", key);
                element.Add(attr);

                var points_element = new XElement("points");

                var upleft_el = new XElement("point");
                upleft_el.SetAttributeValue("x", state.VertexPoint.X);
                upleft_el.SetAttributeValue("y", state.VertexPoint.Y);
                var downleft_el = new XElement("point");
                downleft_el.SetAttributeValue("x", state.UpPoint.X);
                downleft_el.SetAttributeValue("y", state.UpPoint.Y);
                var downright_el = new XElement("point");
                downright_el.SetAttributeValue("x", state.DownPoint.X);
                downright_el.SetAttributeValue("y", state.DownPoint.Y);

                points_element.Add(upleft_el);
                points_element.Add(downleft_el);
                points_element.Add(downright_el);

                element.Add(points_element);

                var geom_element = new XElement("geometry");

                IDescriptionCalculator<SpinousProcessDescription> calc1 = null;

                if (storage.direction == 0)
                {
                    calc1 = new Geometry.DescriptionCalculators.SpinousProcess.LeftSide(state);
                }
                else if (storage.direction == 1)
                {
                    calc1 = new Geometry.DescriptionCalculators.SpinousProcess.RightSide(state);
                }

                var ks = calc1.Keys;

                foreach (var key_param in ks)
                {
                    var geom_node = new XElement("param");
                    attr = new XAttribute("key", key_param);
                    geom_node.Add(attr);
                    attr = new XAttribute("value", string.Format("{0:0.000}", calc1.GetParameter(key_param)));
                    geom_node.Add(attr);
                    geom_element.Add(geom_node);
                }


                element.Add(geom_element);

                processes.Add(element);
            }

            root.Add(processes);

            doc.Save(filename);
        }

        public static void ExportToXLS(string filename, SpineStorage storage)
        {
            var keys = storage.Keys;

            FileStream sw;
            
            IWorkbook workbook = new HSSFWorkbook();

            ISheet sheet1 = workbook.CreateSheet("Позвонки");

            IRow row;

            #region Spines info
            var index = 1;
            bool headerAdded = false;
            foreach (var key in keys)
            {
                var state = storage.GetDescription(key);

                IDescriptionCalculator<SpineDescription> calc = null;

                if (storage.direction == 0)
                {
                    calc = new Geometry.DescriptionCalculators.Spine.LeftSide(state);
                }
                else if (storage.direction == 1)
                {
                    calc = new Geometry.DescriptionCalculators.Spine.RightSide(state);
                }
                else if (storage.direction == 2)
                {
                    calc = new Geometry.DescriptionCalculators.Spine.FrontSide(state);
                }
                else if (storage.direction == 3)
                {
                    calc = new Geometry.DescriptionCalculators.Spine.BackSide(state);
                }

                if (!headerAdded)
                {
                    var ks_ = calc.Keys;
                    row = sheet1.GetRow(0);
                    if (row == null)
                    {
                        row = sheet1.CreateRow(0);
                    }
                    row.CreateCell(0).SetCellValue("Позвонок");
                    sheet1.AutoSizeColumn(0);
                    int st_ = 1;
                    foreach (var key_col in ks_)
                    {
                        row.CreateCell(st_).SetCellValue(calc.GetParameterDescription(key_col));
                        sheet1.AutoSizeColumn(st_);
                        st_++;
                    }
                    headerAdded = true;
                }


                var ks = calc.Keys;
                row = sheet1.GetRow(index);
                if (row == null)
                {
                    row = sheet1.CreateRow(index);
                }
                row.CreateCell(0).SetCellValue(key);
                int st = 1;
                foreach (var key_col in ks)
                {
                    var value = calc.GetParameter(key_col);
                    if (calc.IsParameterLinear(key_col)) {
                        value /= storage.MarkerLength;
                        value *= storage.MarkerSize;
                    }

                    ICell cell = row.CreateCell(st);
                    cell.SetCellType(CellType.Numeric);
                    cell.SetCellValue(double.Parse(string.Format("{0:#.###}", value), CultureInfo.InvariantCulture));
                    sheet1.AutoSizeColumn(st);
                    st++;
                }

                index++;
            }


            #endregion

            sheet1 = workbook.CreateSheet("Межпозвоночные диски");
            index = 0;
            headerAdded = false;

            #region Interspines info
            index++;
            headerAdded = false;

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
                            calc = new Geometry.DescriptionCalculators.Interspine.LeftSide(inter);
                        }
                        else if (storage.direction == 1)
                        {
                            calc = new Geometry.DescriptionCalculators.Interspine.RightSide(inter);
                        }
                        else if (storage.direction == 2)
                        {
                            calc = new Geometry.DescriptionCalculators.Interspine.FrontSide(inter);
                        }
                        else if (storage.direction == 3)
                        {
                            calc = new Geometry.DescriptionCalculators.Interspine.BackSide(inter);
                        }

                        if (!headerAdded)
                        {
                            var ks_ = calc.Keys;
                            row = sheet1.GetRow(index);
                            if (row == null)
                            {
                                row = sheet1.CreateRow(index);
                            }
                            row.CreateCell(0).SetCellValue("Диск");
                            sheet1.AutoSizeColumn(0);
                            int st_ = 1;
                            foreach (var key_col in ks_)
                            {
                                row.CreateCell(st_).SetCellValue(calc.GetParameterDescription(key_col));
                                sheet1.AutoSizeColumn(st_);
                                st_++;
                            }
                            index++;
                            headerAdded = true;
                        }

                        var ks = calc.Keys;
                        row = sheet1.GetRow(index);
                        if (row == null)
                        {
                            row = sheet1.CreateRow(index);
                        }
                        row.CreateCell(0).SetCellValue(SpineConstants.InterSpineNames[keyind]);
                        int st = 1;
                        foreach (var key_col in ks)
                        {
                            var value = calc.GetParameter(key_col);
                            if (calc.IsParameterLinear(key_col))
                            {
                                value /= storage.MarkerLength;
                                value *= storage.MarkerSize;
                            }

                            ICell cell = row.CreateCell(st);
                            cell.SetCellType(CellType.Numeric);
                            cell.SetCellValue(double.Parse(string.Format("{0:#.###}", value), CultureInfo.InvariantCulture));

                            sheet1.AutoSizeColumn(st);
                            st++;
                        }
                        index++;

                    }
                }
            }

            #endregion

            if (storage.SpinousProcessKeys.Count > 0)
            {
                sheet1 = workbook.CreateSheet("Остистые отростки");
                index = 0;
                headerAdded = false;
                #region Process info
                index++;
                headerAdded = false;

                foreach (var key in storage.SpinousProcessKeys)
                {

                    var state = storage.GetSpinousProcessDescription(key);

                    IDescriptionCalculator<SpinousProcessDescription> calc1 = null;

                    if (storage.direction == 0)
                    {
                        calc1 = new Geometry.DescriptionCalculators.SpinousProcess.LeftSide(state);
                    }
                    else if (storage.direction == 1)
                    {
                        calc1 = new Geometry.DescriptionCalculators.SpinousProcess.RightSide(state);
                    }

                    var ks = calc1.Keys;


                    if (!headerAdded)
                    {
                        var ks_ = calc1.Keys;
                        row = sheet1.GetRow(0);
                        if (row == null)
                        {
                            row = sheet1.CreateRow(0);
                        }
                        row.CreateCell(0).SetCellValue("Участок");
                        sheet1.AutoSizeColumn(0);
                        int st_ = 1;
                        foreach (var key_col in ks_)
                        {
                            row = sheet1.GetRow(index);
                            if (row == null)
                            {
                                row = sheet1.CreateRow(index);
                            }
                            row.CreateCell(0).SetCellValue(calc1.GetParameterDescription(key_col));
                            sheet1.AutoSizeColumn(0);
                            st_++;
                        }
                        index++;
                        headerAdded = true;
                    }

                    ks = calc1.Keys;
                    row = sheet1.GetRow(index);
                    if (row == null)
                    {
                        row = sheet1.CreateRow(index);
                    }
                    row.CreateCell(0).SetCellValue(key);
                    sheet1.AutoSizeColumn(0);
                    int st = 1;
                    foreach (var key_col in ks)
                    {
                        row = sheet1.GetRow(index);
                        if (row == null)
                        {
                            row = sheet1.CreateRow(index);
                        }

                        var value = calc1.GetParameter(key_col);
                        if (calc1.IsParameterLinear(key_col))
                        {
                            value /= storage.MarkerLength;
                            value *= storage.MarkerSize;
                        }

                        ICell cell = row.CreateCell(st);
                        cell.SetCellType(CellType.Numeric);
                        cell.SetCellValue(double.Parse(string.Format("{0:#.###}", value), CultureInfo.InvariantCulture));
                        sheet1.AutoSizeColumn(st);
                        st++;
                    }
                    index++;

                }
                #endregion
            }
            sw = File.Create(filename);

            workbook.Write(sw);

            sw.Close();
        }

        public static void ExportToXLSCompare(string filename, SpineStorage storage_lay, SpineStorage storage_stay) {
            FileStream sw;

            IWorkbook workbook = new HSSFWorkbook();

            ISheet sheet1 = workbook.CreateSheet("Позвонки");

            IRow row;

            #region Spines info
            var index = 1;
            bool headerAdded = false;
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
                        calc_first = new Geometry.DescriptionCalculators.Spine.LeftSide(spine_first);
                    }
                    else if (storage_lay.direction == 1)
                    {
                        calc_first = new Geometry.DescriptionCalculators.Spine.RightSide(spine_first);
                    }
                    else if (storage_lay.direction == 2)
                    {
                        calc_first = new Geometry.DescriptionCalculators.Spine.FrontSide(spine_first);
                    }
                    else if (storage_lay.direction == 3)
                    {
                        calc_first = new Geometry.DescriptionCalculators.Spine.BackSide(spine_first);
                    }

                    if (storage_stay.direction == 0)
                    {
                        calc_second = new Geometry.DescriptionCalculators.Spine.LeftSide(spine_second);
                    }
                    else if (storage_stay.direction == 1)
                    {
                        calc_second = new Geometry.DescriptionCalculators.Spine.RightSide(spine_second);
                    }
                    else if (storage_stay.direction == 2)
                    {
                        calc_second = new Geometry.DescriptionCalculators.Spine.FrontSide(spine_second);
                    }
                    else if (storage_stay.direction == 3)
                    {
                        calc_second = new Geometry.DescriptionCalculators.Spine.BackSide(spine_second);
                    }

                    var ks = calc_first.Keys;

                    if (!headerAdded)
                    {
                        row = sheet1.GetRow(0);
                        if (row == null)
                        {
                            row = sheet1.CreateRow(0);
                        }
                        row.CreateCell(0).SetCellValue("Название");
                        sheet1.AutoSizeColumn(0);
                        row.CreateCell(1).SetCellValue("Лежа");
                        sheet1.AutoSizeColumn(1);
                        row.CreateCell(2).SetCellValue("Стоя");
                        sheet1.AutoSizeColumn(2);
                        row.CreateCell(3).SetCellValue("Сравнение");
                        sheet1.AutoSizeColumn(3);
                        headerAdded = true;
                    }


                    row = sheet1.GetRow(index);
                    if (row == null)
                    {
                        row = sheet1.CreateRow(index);
                    }
                    index++;
                    row.CreateCell(1).SetCellValue(key);
                    sheet1.AutoSizeColumn(1);
                    row.CreateCell(2).SetCellValue(key);
                    sheet1.AutoSizeColumn(2);

                    foreach (var key_col in ks)
                    {

                        row = sheet1.GetRow(index);
                        if (row == null)
                        {
                            row = sheet1.CreateRow(index);
                        }

                        var desc = calc_first.GetParameterDescription(key_col);
                        var var_first = calc_first.GetParameter(key_col);
                        if (calc_first.IsParameterLinear(key_col))
                        {
                            var_first /= storage_lay.MarkerLength;
                            var_first *= storage_lay.MarkerSize;
                        }
                        var var_second = calc_second.GetParameter(key_col);
                        if (calc_second.IsParameterLinear(key_col))
                        {
                            var_second /= storage_stay.MarkerLength;
                            var_second *= storage_stay.MarkerSize;
                        }

                        row.CreateCell(0).SetCellValue(desc);
                        sheet1.AutoSizeColumn(0);
                        ICell cell = row.CreateCell(1);
                        cell.SetCellType(CellType.Numeric);
                        cell.SetCellValue(double.Parse(string.Format("{0:#.###}", var_first), CultureInfo.InvariantCulture));
                        sheet1.AutoSizeColumn(1);
                        cell = row.CreateCell(2);
                        cell.SetCellType(CellType.Numeric);
                        cell.SetCellValue(double.Parse(string.Format("{0:#.###}", var_second), CultureInfo.InvariantCulture));
                        sheet1.AutoSizeColumn(2);

                        if (calc_first.IsParameterLinear(key_col) && calc_second.IsParameterLinear(key_col))
                        {
                            var diff = Math.Abs((var_first - var_second) / var_first) * 100;

                            if (var_first < var_second)
                            {
                                row.CreateCell(3).SetCellValue(string.Format("Увеличение на {0}%", string.Format("{0:0.000}", diff)));
                            }
                            else
                            {
                                row.CreateCell(3).SetCellValue(string.Format("Уменьшение на {0}%", string.Format("{0:0.000}", diff)));
                            }
                        }
                        else
                        {
                            if (var_first < var_second)
                            {
                                row.CreateCell(3).SetCellValue(string.Format("Увеличение на {0}", string.Format("{0:0.000}", var_second - var_first)));
                            }
                            else
                            {
                                row.CreateCell(3).SetCellValue(string.Format("Уменьшение на {0}", string.Format("{0:0.000}", var_first - var_second)));
                            }
                        }
                        sheet1.AutoSizeColumn(3);

                        index++;
                    }


                }

            }
            headerAdded = false;
            index++;

            #endregion

            sheet1 = workbook.CreateSheet("Межпозвоночные диски");
            index = 0;
            headerAdded = false;

            #region Interspines info

            headerAdded = false;
            foreach (var key in storage_lay.Keys)
            {
                if (storage_stay.ContainDescription(key))
                {
                    int keyind = SpineConstants.SpineNames.IndexOf(key);
                    if (keyind != SpineConstants.SpineNames.Count - 1)
                    {
                        var next = SpineConstants.SpineNames[(keyind + 1) % SpineConstants.SpineNames.Count];

                        var interspine_key = SpineConstants.InterSpineNames[keyind];

                        if (storage_stay.ContainDescription(key) && storage_stay.ContainDescription(next) &&
                            storage_lay.ContainDescription(key) && storage_lay.ContainDescription(next))

                        {
                            var ispine_first = new InterspineDescription()
                            {
                                UpSpine = key,
                                DownSpine = next,
                                storage = storage_lay
                            };

                            var ispine_second = new InterspineDescription()
                            {
                                UpSpine = key,
                                DownSpine = next,
                                storage = storage_stay
                            };

                            IDescriptionCalculator<InterspineDescription> calc_first = null;
                            IDescriptionCalculator<InterspineDescription> calc_second = null;

                            if (storage_lay.direction == 0)
                            {
                                calc_first = new Geometry.DescriptionCalculators.Interspine.LeftSide(ispine_first);
                            }
                            else if (storage_lay.direction == 1)
                            {
                                calc_first = new Geometry.DescriptionCalculators.Interspine.RightSide(ispine_first);
                            }
                            else if (storage_lay.direction == 2)
                            {
                                calc_first = new Geometry.DescriptionCalculators.Interspine.FrontSide(ispine_first);
                            }
                            else if (storage_lay.direction == 3)
                            {
                                calc_first = new Geometry.DescriptionCalculators.Interspine.BackSide(ispine_first);
                            }

                            if (storage_stay.direction == 0)
                            {
                                calc_second = new Geometry.DescriptionCalculators.Interspine.LeftSide(ispine_second);
                            }
                            else if (storage_stay.direction == 1)
                            {
                                calc_second = new Geometry.DescriptionCalculators.Interspine.RightSide(ispine_second);
                            }
                            else if (storage_stay.direction == 2)
                            {
                                calc_second = new Geometry.DescriptionCalculators.Interspine.FrontSide(ispine_second);
                            }
                            else if (storage_stay.direction == 3)
                            {
                                calc_second = new Geometry.DescriptionCalculators.Interspine.BackSide(ispine_second);
                            }

                            var ks = calc_first.Keys;

                            if (!headerAdded)
                            {
                                row = sheet1.GetRow(index);
                                if (row == null)
                                {
                                    row = sheet1.CreateRow(index);
                                }
                                row.CreateCell(0).SetCellValue("Название");
                                sheet1.AutoSizeColumn(0);
                                row.CreateCell(1).SetCellValue("Лежа");
                                sheet1.AutoSizeColumn(1);
                                row.CreateCell(2).SetCellValue("Стоя");
                                sheet1.AutoSizeColumn(2);
                                row.CreateCell(3).SetCellValue("Сравнение");
                                sheet1.AutoSizeColumn(3);
                                headerAdded = true;
                            }


                            row = sheet1.GetRow(index);
                            if (row == null)
                            {
                                row = sheet1.CreateRow(index);
                            }
                            index++;
                            row.CreateCell(1).SetCellValue(interspine_key);
                            sheet1.AutoSizeColumn(1);
                            row.CreateCell(2).SetCellValue(interspine_key);
                            sheet1.AutoSizeColumn(2);

                            foreach (var key_col in ks)
                            {

                                row = sheet1.GetRow(index);
                                if (row == null)
                                {
                                    row = sheet1.CreateRow(index);
                                }

                                var desc = calc_first.GetParameterDescription(key_col);
                                var var_first = calc_first.GetParameter(key_col);
                                if (calc_first.IsParameterLinear(key_col))
                                {
                                    var_first /= storage_lay.MarkerLength;
                                    var_first *= storage_lay.MarkerSize;
                                }
                                var var_second = calc_second.GetParameter(key_col);
                                if (calc_second.IsParameterLinear(key_col))
                                {
                                    var_second /= storage_stay.MarkerLength;
                                    var_second *= storage_stay.MarkerSize;
                                }

                                row.CreateCell(0).SetCellValue(desc);
                                sheet1.AutoSizeColumn(0);
                                ICell cell = row.CreateCell(1);
                                cell.SetCellType(CellType.Numeric);
                                cell.SetCellValue(double.Parse(string.Format("{0:#.###}", var_first), CultureInfo.InvariantCulture));
                                sheet1.AutoSizeColumn(1);
                                cell = row.CreateCell(2);
                                cell.SetCellType(CellType.Numeric);
                                cell.SetCellValue(double.Parse(string.Format("{0:#.###}", var_second), CultureInfo.InvariantCulture));
                                sheet1.AutoSizeColumn(2);

                                if (calc_first.IsParameterLinear(key_col) && calc_second.IsParameterLinear(key_col))
                                {
                                    var diff = Math.Abs((var_first - var_second) / var_first) * 100;

                                    if (var_first < var_second)
                                    {
                                        row.CreateCell(3).SetCellValue(string.Format("Увеличение на {0}%", string.Format("{0:0.000}", diff)));
                                    }
                                    else
                                    {
                                        row.CreateCell(3).SetCellValue(string.Format("Уменьшение на {0}%", string.Format("{0:0.000}", diff)));
                                    }
                                }
                                else
                                {
                                    if (var_first < var_second)
                                    {
                                        row.CreateCell(3).SetCellValue(string.Format("Увеличение на {0}", string.Format("{0:0.000}", var_second - var_first)));
                                    }
                                    else
                                    {
                                        row.CreateCell(3).SetCellValue(string.Format("Уменьшение на {0}", string.Format("{0:0.000}", var_first - var_second)));
                                    }
                                }
                                sheet1.AutoSizeColumn(3);

                                index++;
                            }
                        }

                    }
                    
                }

            }

            index++;
            headerAdded = false;
            #endregion


            if (storage_lay.SpinousProcessKeys.Count > 0) {
                sheet1 = workbook.CreateSheet("Углы между отростками");
                index = 0;
                headerAdded = false;

                #region Process info
                foreach (var key in storage_lay.SpinousProcessKeys)
                {
                    if (storage_stay.ContainSpinousProcessDescription(key))
                    {
                        var spine_first = storage_lay.GetSpinousProcessDescription(key);
                        var spine_second = storage_stay.GetSpinousProcessDescription(key);

                        IDescriptionCalculator<SpinousProcessDescription> calc_first = null;
                        IDescriptionCalculator<SpinousProcessDescription> calc_second = null;


                        if (storage_lay.direction == 0)
                        {
                            calc_first = new Geometry.DescriptionCalculators.SpinousProcess.LeftSide(spine_first);
                        }
                        else if (storage_lay.direction == 1)
                        {
                            calc_first = new Geometry.DescriptionCalculators.SpinousProcess.RightSide(spine_first);
                        }

                        if (storage_stay.direction == 0)
                        {
                            calc_second = new Geometry.DescriptionCalculators.SpinousProcess.LeftSide(spine_second);
                        }
                        else if (storage_stay.direction == 1)
                        {
                            calc_second = new Geometry.DescriptionCalculators.SpinousProcess.RightSide(spine_second);
                        }

                        var ks = calc_first.Keys;

                        if (!headerAdded)
                        {
                            row = sheet1.GetRow(index);
                            if (row == null)
                            {
                                row = sheet1.CreateRow(index);
                            }
                            row.CreateCell(0).SetCellValue("Название");
                            sheet1.AutoSizeColumn(0);
                            row.CreateCell(1).SetCellValue("Лежа");
                            sheet1.AutoSizeColumn(1);
                            row.CreateCell(2).SetCellValue("Стоя");
                            sheet1.AutoSizeColumn(2);
                            row.CreateCell(3).SetCellValue("Сравнение");
                            sheet1.AutoSizeColumn(3);
                            headerAdded = true;
                        }


                        row = sheet1.GetRow(index);
                        if (row == null)
                        {
                            row = sheet1.CreateRow(index);
                        }
                        index++;
                        row.CreateCell(1).SetCellValue(key);
                        sheet1.AutoSizeColumn(1);
                        row.CreateCell(2).SetCellValue(key);
                        sheet1.AutoSizeColumn(2);

                        foreach (var key_col in ks)
                        {

                            row = sheet1.GetRow(index);
                            if (row == null)
                            {
                                row = sheet1.CreateRow(index);
                            }

                            var desc = calc_first.GetParameterDescription(key_col);
                            var var_first = calc_first.GetParameter(key_col);
                            if (calc_first.IsParameterLinear(key_col))
                            {
                                var_first /= storage_lay.MarkerLength;
                                var_first *= storage_lay.MarkerSize;
                            }
                            var var_second = calc_second.GetParameter(key_col);
                            if (calc_second.IsParameterLinear(key_col))
                            {
                                var_second /= storage_stay.MarkerLength;
                                var_second *= storage_stay.MarkerSize;
                            }

                            row.CreateCell(0).SetCellValue(desc);
                            sheet1.AutoSizeColumn(0);
                            ICell cell = row.CreateCell(1);
                            cell.SetCellType(CellType.Numeric);
                            cell.SetCellValue(double.Parse(string.Format("{0:#.###}", var_first), CultureInfo.InvariantCulture));
                            sheet1.AutoSizeColumn(1);
                            cell = row.CreateCell(2);
                            cell.SetCellType(CellType.Numeric);
                            cell.SetCellValue(double.Parse(string.Format("{0:#.###}", var_second), CultureInfo.InvariantCulture));
                            sheet1.AutoSizeColumn(2);

                            if (calc_first.IsParameterLinear(key_col) && calc_second.IsParameterLinear(key_col))
                            {
                                var diff = Math.Abs((var_first - var_second) / var_first) * 100;

                                if (var_first < var_second)
                                {
                                    row.CreateCell(3).SetCellValue(string.Format("Увеличение на {0}%", string.Format("{0:0.000}", diff)));
                                }
                                else
                                {
                                    row.CreateCell(3).SetCellValue(string.Format("Уменьшение на {0}%", string.Format("{0:0.000}", diff)));
                                }
                            }
                            else
                            {
                                if (var_first < var_second)
                                {
                                    row.CreateCell(3).SetCellValue(string.Format("Увеличение на {0}", string.Format("{0:0.000}", var_second - var_first)));
                                }
                                else
                                {
                                    row.CreateCell(3).SetCellValue(string.Format("Уменьшение на {0}", string.Format("{0:0.000}", var_first - var_second)));
                                }
                            }

                            
                            sheet1.AutoSizeColumn(3);

                            index++;
                        }


                    }

                }
                headerAdded = false;
                index++;
                #endregion
            }


            sw = File.Create(filename);

            workbook.Write(sw);

            sw.Close();
        }

        public static void ExportToDOCCompare(string filename, Patient patient, string date, SpineStorage storage_lay, SpineStorage storage_stay) {

            var document = DocX.Create(filename);

            var headFormat = new Formatting
            {
                FontFamily = new System.Drawing.FontFamily("Times New Roman"),
                Size = 16,
                Bold = true,
                Language = CultureInfo.GetCultureInfoByIetfLanguageTag("ru")
            };


            var finalDiseases = new List<Tuple<string, int>>();

            var p = document.InsertParagraph("Заключение".ToUpper(), false, headFormat);
            p.Alignment = Alignment.center;
            p.AppendLine();

            headFormat.Size = 14;
            p = document.InsertParagraph("Пациент - ", false, headFormat);
            headFormat.Bold = false;
            string name_full = String.Format("{0} {1} {2}", patient.Surname, patient.Name, patient.Patronymic);
            switch (patient.Age % 10) {
                case 0:
                case 5:
                case 6:
                case 7:
                case 8:
                case 9:
                    name_full += string.Format(", {0} лет", patient.Age);
                    break;
                case 1:
                    name_full += string.Format(", {0} год", patient.Age);
                    break;
                default:
                    name_full += string.Format(", {0} года", patient.Age);
                    break;
            }
            p.InsertText(name_full, false, headFormat);
            p.Alignment = Alignment.left;
            p.AppendLine();

            headFormat.Bold = true;
            p = document.InsertParagraph("Дата обследования - ", false, headFormat);
            headFormat.Bold = false;
            p.InsertText(date, false, headFormat);
            p.Alignment = Alignment.left;
            p.AppendLine();

            headFormat.Bold = true;
            headFormat.Size = 16;

            p = document.InsertParagraph("Данные".ToUpper(), false, headFormat);
            p.Alignment = Alignment.center;
            p.AppendLine();


            #region Spines info

            headFormat.Bold = true;
            headFormat.Size = 14;

            p = document.InsertParagraph("Позвонки".ToUpper(), false, headFormat);
            p.Alignment = Alignment.center;
            p.AppendLine();

            headFormat.Bold = false;

            Table table_spines = null;

            var index = 1;
            bool headerAdded = false;
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
                        calc_first = new Geometry.DescriptionCalculators.Spine.LeftSide(spine_first);
                    }
                    else if (storage_lay.direction == 1)
                    {
                        calc_first = new Geometry.DescriptionCalculators.Spine.RightSide(spine_first);
                    }
                    else if (storage_lay.direction == 2)
                    {
                        calc_first = new Geometry.DescriptionCalculators.Spine.FrontSide(spine_first);
                    }
                    else if (storage_lay.direction == 3)
                    {
                        calc_first = new Geometry.DescriptionCalculators.Spine.BackSide(spine_first);
                    }

                    if (storage_stay.direction == 0)
                    {
                        calc_second = new Geometry.DescriptionCalculators.Spine.LeftSide(spine_second);
                    }
                    else if (storage_stay.direction == 1)
                    {
                        calc_second = new Geometry.DescriptionCalculators.Spine.RightSide(spine_second);
                    }
                    else if (storage_stay.direction == 2)
                    {
                        calc_second = new Geometry.DescriptionCalculators.Spine.FrontSide(spine_second);
                    }
                    else if (storage_stay.direction == 3)
                    {
                        calc_second = new Geometry.DescriptionCalculators.Spine.BackSide(spine_second);
                    }

                    var ks = calc_first.Keys;

                    if (!headerAdded)
                    {
                        table_spines = document.AddTable(storage_lay.Keys.Count * (ks.Count + 1) + 1, 4);
                        table_spines.Rows[0].Cells[0].Paragraphs.First().Append("Название");
                        table_spines.Rows[0].Cells[1].Paragraphs.First().Append("Лежа");
                        table_spines.Rows[0].Cells[2].Paragraphs.First().Append("Стоя");
                        table_spines.Rows[0].Cells[3].Paragraphs.First().Append("Сравнение");
                        headerAdded = true;
                    }

                    table_spines.Rows[index].Cells[0].Paragraphs.First().Append(key);
                    //table_spines.Rows[index].MergeCells(0, 3);
                    index++;


                    foreach (var key_col in ks)
                    {

                        var desc = calc_first.GetParameterDescription(key_col);
                        var var_first = calc_first.GetParameter(key_col);
                        if (calc_first.IsParameterLinear(key_col))
                        {
                            var_first /= storage_lay.MarkerLength;
                            var_first *= storage_lay.MarkerSize;
                        }
                        var var_second = calc_second.GetParameter(key_col);
                        if (calc_second.IsParameterLinear(key_col))
                        {
                            var_second /= storage_stay.MarkerLength;
                            var_second *= storage_stay.MarkerSize;
                        }

                        table_spines.Rows[index].Cells[0].Paragraphs.First().Append(desc);
                        table_spines.Rows[index].Cells[1].Paragraphs.First().Append(string.Format("{0:0.000}", var_first));
                        table_spines.Rows[index].Cells[2].Paragraphs.First().Append(string.Format("{0:0.000}", var_second));

                        if (calc_first.IsParameterLinear(key_col) && calc_second.IsParameterLinear(key_col))
                        {
                            var diff = Math.Abs((var_first - var_second) / var_first) * 100;

                            if (var_first < var_second)
                            {
                                table_spines.Rows[index].Cells[3].Paragraphs.First().Append(string.Format("Увеличение на {0}%", string.Format("{0:0.000}", diff)));
                            }
                            else
                            {
                                table_spines.Rows[index].Cells[3].Paragraphs.First().Append(string.Format("Уменьшение на {0}%", string.Format("{0:0.000}", diff)));
                            }
                        }
                        else
                        {
                            if (var_first < var_second)
                            {
                                table_spines.Rows[index].Cells[3].Paragraphs.First().Append(string.Format("Увеличение на {0}", string.Format("{0:0.000}", var_second - var_first)));
                            }
                            else
                            {
                                table_spines.Rows[index].Cells[3].Paragraphs.First().Append(string.Format("Уменьшение на {0}", string.Format("{0:0.000}", var_first - var_second)));
                            }
                        }

                        index++;
                    }


                }

            }
            table_spines.Paragraphs.First().AppendLine();
            document.InsertTable(table_spines);
            document.InsertParagraph().AppendLine();
            #endregion

            #region Interspines info
            headFormat.Bold = true;
            headFormat.Size = 14;

            p = document.InsertParagraph("Межпозвоночные диски".ToUpper(), false, headFormat);
            p.Alignment = Alignment.center;
            p.AppendLine();

            headFormat.Bold = false;

            Table table_interspines = null;

            index = 1;
            headerAdded = false;
            foreach (var key in storage_lay.Keys)
            {
                if (storage_stay.ContainDescription(key))
                {
                    int keyind = SpineConstants.SpineNames.IndexOf(key);
                    if (keyind != SpineConstants.SpineNames.Count - 1)
                    {
                        var next = SpineConstants.SpineNames[(keyind + 1) % SpineConstants.SpineNames.Count];

                        var interspine_key = SpineConstants.InterSpineNames[keyind];

                        if (storage_stay.ContainDescription(key) && storage_stay.ContainDescription(next) &&
                            storage_lay.ContainDescription(key) && storage_lay.ContainDescription(next))

                        {
                            var ispine_first = new InterspineDescription()
                            {
                                UpSpine = key,
                                DownSpine = next,
                                storage = storage_lay
                            };

                            var ispine_second = new InterspineDescription()
                            {
                                UpSpine = key,
                                DownSpine = next,
                                storage = storage_stay
                            };

                            IDescriptionCalculator<InterspineDescription> calc_lay = null;
                            IDescriptionCalculator<InterspineDescription> calc_stay = null;

                            if (storage_lay.direction == 0)
                            {
                                calc_lay = new Geometry.DescriptionCalculators.Interspine.LeftSide(ispine_first);
                            }
                            else if (storage_lay.direction == 1)
                            {
                                calc_lay = new Geometry.DescriptionCalculators.Interspine.RightSide(ispine_first);
                            }
                            else if (storage_lay.direction == 2)
                            {
                                calc_lay = new Geometry.DescriptionCalculators.Interspine.FrontSide(ispine_first);
                            }
                            else if (storage_lay.direction == 3)
                            {
                                calc_lay = new Geometry.DescriptionCalculators.Interspine.BackSide(ispine_first);
                            }

                            if (storage_stay.direction == 0)
                            {
                                calc_stay = new Geometry.DescriptionCalculators.Interspine.LeftSide(ispine_second);
                            }
                            else if (storage_stay.direction == 1)
                            {
                                calc_stay = new Geometry.DescriptionCalculators.Interspine.RightSide(ispine_second);
                            }
                            else if (storage_stay.direction == 2)
                            {
                                calc_stay = new Geometry.DescriptionCalculators.Interspine.FrontSide(ispine_second);
                            }
                            else if (storage_stay.direction == 3)
                            {
                                calc_stay = new Geometry.DescriptionCalculators.Interspine.BackSide(ispine_second);
                            }

                            var ks = calc_lay.Keys;

                            if (!headerAdded)
                            {
                                table_interspines = document.AddTable((storage_lay.Keys.Count - 1) * (ks.Count + 1) + 1, 4);
                                table_interspines.Rows[0].Cells[0].Paragraphs.First().Append("Название");
                                table_interspines.Rows[0].Cells[1].Paragraphs.First().Append("Лежа");
                                table_interspines.Rows[0].Cells[2].Paragraphs.First().Append("Стоя");
                                table_interspines.Rows[0].Cells[3].Paragraphs.First().Append("Сравнение");
                                headerAdded = true;
                            }

                            table_interspines.Rows[index].Cells[0].Paragraphs.First().Append(interspine_key);
                            //table_interspines.Rows[index].MergeCells(0, 3);
                            index++;


                            foreach (var key_col in ks)
                            {
                                var desc = calc_lay.GetParameterDescription(key_col);
                                var var_lay = calc_lay.GetParameter(key_col);
                                if (calc_lay.IsParameterLinear(key_col))
                                {
                                    var_lay /= storage_lay.MarkerLength;
                                    var_lay *= storage_lay.MarkerSize;
                                }
                                var var_stay = calc_stay.GetParameter(key_col);
                                if (calc_stay.IsParameterLinear(key_col))
                                {
                                    var_stay /= storage_stay.MarkerLength;
                                    var_stay *= storage_stay.MarkerSize;
                                }

                                table_interspines.Rows[index].Cells[0].Paragraphs.First().Append(desc);
                                table_interspines.Rows[index].Cells[1].Paragraphs.First().Append(string.Format("{0:0.000}", var_lay));                         
                                table_interspines.Rows[index].Cells[2].Paragraphs.First().Append(string.Format("{0:0.000}", var_stay));

                                if (calc_lay.IsParameterLinear(key_col) && calc_stay.IsParameterLinear(key_col))
                                {
                                    var diff = Math.Abs((var_lay - var_stay) / var_lay) * 100;

                                    if (var_lay < var_stay)
                                    {
                                        table_interspines.Rows[index].Cells[3].Paragraphs.First().Append(string.Format("Увеличение на {0}%", string.Format("{0:0.000}", diff)));
                                        if (key_col.Equals("d2"))
                                        {
                                            finalDiseases.Add(new Tuple<string, int>(interspine_key, 1));
                                        }
                                    }
                                    else
                                    {
                                        table_interspines.Rows[index].Cells[3].Paragraphs.First().Append(string.Format("Уменьшение на {0}%", string.Format("{0:0.000}", diff)));
                                        if (key_col.Equals("d1") || key_col.Equals("alpha_d"))
                                        {
                                            finalDiseases.Add(new Tuple<string, int>(interspine_key, 1));
                                        }
                                    }
                                }
                                else
                                {
                                    if (var_lay < var_stay)
                                    {
                                        table_interspines.Rows[index].Cells[3].Paragraphs.First().Append(string.Format("Увеличение на {0}", string.Format("{0:0.000}", var_stay - var_lay)));
                                    }
                                    else
                                    {
                                        table_interspines.Rows[index].Cells[3].Paragraphs.First().Append(string.Format("Уменьшение на {0}", string.Format("{0:0.000}", var_lay - var_stay)));
                                    }
                                }

                                index++;
                                
                            }
                        }

                    }

                }

            }
            table_interspines.Paragraphs.First().AppendLine();
            document.InsertTable(table_interspines);
            document.InsertParagraph().AppendLine();
            #endregion

            #region Process info
            if (storage_lay.SpinousProcessKeys.Count > 0 && storage_stay.SpinousProcessKeys.Count > 0)
            {
                headFormat.Bold = true;
                headFormat.Size = 14;

                p = document.InsertParagraph("Углы между остистыми отростками".ToUpper(), false, headFormat);
                p.Alignment = Alignment.center;
                p.AppendLine();

                headFormat.Bold = false;

                Table table_angles = null;

                index = 1;
                headerAdded = false;

                foreach (var process_key in storage_lay.SpinousProcessKeys)
                {
                    if (storage_stay.ContainSpinousProcessDescription(process_key))
                    {
                        var spine_first = storage_lay.GetSpinousProcessDescription(process_key);
                        var spine_second = storage_stay.GetSpinousProcessDescription(process_key);

                        IDescriptionCalculator<SpinousProcessDescription> calc_lay = null;
                        IDescriptionCalculator<SpinousProcessDescription> calc_stay = null;


                        if (storage_lay.direction == 0)
                        {
                            calc_lay = new Geometry.DescriptionCalculators.SpinousProcess.LeftSide(spine_first);
                        }
                        else if (storage_lay.direction == 1)
                        {
                            calc_lay = new Geometry.DescriptionCalculators.SpinousProcess.RightSide(spine_first);
                        }

                        if (storage_stay.direction == 0)
                        {
                            calc_stay = new Geometry.DescriptionCalculators.SpinousProcess.LeftSide(spine_second);
                        }
                        else if (storage_stay.direction == 1)
                        {
                            calc_stay = new Geometry.DescriptionCalculators.SpinousProcess.RightSide(spine_second);
                        }

                        var ks = calc_lay.Keys;

                        if (!headerAdded)
                        {
                            table_angles = document.AddTable(storage_lay.SpinousProcessKeys.Count * (ks.Count + 1) + 1, 4);
                            table_angles.Rows[0].Cells[0].Paragraphs.First().Append("Название");
                            table_angles.Rows[0].Cells[1].Paragraphs.First().Append("Лежа");
                            table_angles.Rows[0].Cells[2].Paragraphs.First().Append("Стоя");
                            table_angles.Rows[0].Cells[3].Paragraphs.First().Append("Сравнение");
                            headerAdded = true;
                        }

                        table_angles.Rows[index].Cells[0].Paragraphs.First().Append(process_key);
                        //table_angles.Rows[index].MergeCells(0, 3);
                        index++;


                        foreach (var key_col in ks)
                        {

                            var desc = calc_lay.GetParameterDescription(key_col);
                            var var_lay = calc_lay.GetParameter(key_col);
                            if (calc_lay.IsParameterLinear(key_col))
                            {
                                var_lay /= storage_lay.MarkerLength;
                                var_lay *= storage_lay.MarkerSize;
                            }
                            var var_stay = calc_stay.GetParameter(key_col);
                            if (calc_stay.IsParameterLinear(key_col))
                            {
                                var_stay /= storage_stay.MarkerLength;
                                var_stay *= storage_stay.MarkerSize;
                            }

                            table_angles.Rows[index].Cells[0].Paragraphs.First().Append(desc);
                            table_angles.Rows[index].Cells[1].Paragraphs.First().Append(string.Format("{0:0.000}", var_lay));
                            var par = table_angles.Rows[index].Cells[2].Paragraphs.First();
                            par.Append(string.Format("{0:0.000}", var_stay));
                            par.Bold();


                            if (calc_lay.IsParameterLinear(key_col) && calc_stay.IsParameterLinear(key_col))
                            {
                                var diff = Math.Abs((var_lay - var_stay) / var_lay) * 100;

                                if (var_lay < var_stay)
                                {
                                    table_angles.Rows[index].Cells[3].Paragraphs.First().Append(string.Format("Увеличение на {0}%", string.Format("{0:0.000}", diff)));
                                }
                                else
                                {
                                    table_angles.Rows[index].Cells[3].Paragraphs.First().Append(string.Format("Уменьшение на {0}%", string.Format("{0:0.000}", diff)));
                                }
                            }
                            else
                            {
                                if (var_lay <= var_stay)
                                {
                                    if (key_col.Equals("alpha"))
                                    {
                                        finalDiseases.Add(new Tuple<string, int>(process_key, 2));
                                    }
                                    table_angles.Rows[index].Cells[3].Paragraphs.First().Append(string.Format("Увеличение на {0}", string.Format("{0:0.000}", var_stay - var_lay)));
                                }
                                else
                                {
                                    table_angles.Rows[index].Cells[3].Paragraphs.First().Append(string.Format("Уменьшение на {0}", string.Format("{0:0.000}", var_lay - var_stay)));
                                }
                            }

                            index++;
                        }


                    }

                }
                table_angles.Paragraphs.First().AppendLine();
                document.InsertTable(table_angles);
                headerAdded = false;
                index++;
                document.InsertParagraph().AppendLine();
            }
            #endregion

            if (finalDiseases.Count >= 2) {

                HashSet<Tuple<string, int>> dis_set = new HashSet<Tuple<string, int>>();

                foreach (var fd in finalDiseases)
                {
                    dis_set.Add(fd);
                }

                headFormat.Size = 18;
                headFormat.Bold = true;
                p = document.InsertParagraph("Признаки нарушения биомеханики обнаружены на уровнях:", false, headFormat);
                p.Alignment = Alignment.center;
                p.AppendLine();
                headFormat.Size = 14;
                headFormat.Bold = false;
                foreach (var fd in dis_set)
                {
                    string res = null;
                    switch (fd.Item2)
                    {
                        case 1:
                            res = string.Format("Межпозвоночный диск - {0}", fd.Item1);
                            break;
                        case 2:
                            res = string.Format("Угол между остистыми отростками - {0}", fd.Item1);
                            break;
                        default:
                            break;
                    }
                    p = document.InsertParagraph(res, false, headFormat);
                    p.SetLineSpacing(LineSpacingType.Line, 1);
                    p.Alignment = Alignment.left;
                }
            }

            document.Save();
        }
    }
}
