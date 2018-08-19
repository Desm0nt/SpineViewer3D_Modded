using SpineLib.Geometry.DescriptionCalculators.Spine;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Xml.Linq;
using SpineLib.Geometry.Descriptions;
using System.Globalization;
using NLog;

namespace SpineLib.Geometry
{
    public class SpineReader
    {

        private static Logger logger = LogManager.GetLogger("StorageLog");

        public static SpineStorage ReadFromFile(string filename) {
            var storage = new SpineStorage();

            string text = null;
            StreamReader file = null;
            try
            {
                file = new StreamReader(filename);
                text = file.ReadToEnd();
                logger.Info("Read storage file - {0}", filename);
            }
            catch (FileNotFoundException e)
            {
                text = "";
                logger.Error(e, "File not found - {0}", filename);
            }
            finally {
                if (file != null)
                    file.Close();
            }

            XDocument doc = new XDocument();

            try
            {
                doc = XDocument.Parse(text);

                var root = doc.Element("storage");


                #region Window Parameters

                var window_params = ParseWindowParameters(root.Element("window"));
                storage.windowCenter = window_params.Item1;
                storage.windowWidth = window_params.Item2;


                #endregion

                #region Direction Parameters

                storage.direction = ParseDirectionParameters(root.Element("direction"));

                storage.imageDirection = ParseImageDirectionParameters(root.Element("imageDirection"));

                var angle = ParseAngleParameters(root.Element("angle"));
                storage.SetRotatingAngle(angle);

                #endregion

                #region Marker Points

                var markers = ParseMarkers(root.Element("markers"));

                for (int i = 0; i < markers.Count; i++)
                {
                    var point = new Point(markers[i].Item1, markers[i].Item2);
                    storage.SetMarkerPoint(i, point);
                }

                #endregion

                #region Marker Line

                var markerl = ParseMarkerLine(root.Element("markerline"));

                storage.MarkerLine = markerl.Item1;
                storage.MarkerLength = markerl.Item2;
                storage.MarkerSize = markerl.Item3;

                #endregion


                logger.Info("Read pixel parameters for file - {0}", filename);

                foreach (var element in root.Descendants("spine"))
                {
                    
                   var key = element.Attribute("key").Value;
                   var points = new List<Point>();

                   var points_xml = element.Element("points");

                   var points_x = points_xml.Descendants("point");

                    foreach (var point in points_x)
                    {
                        var x = int.Parse(point.Attribute("x").Value, CultureInfo.InvariantCulture);
                        var y = int.Parse(point.Attribute("y").Value, CultureInfo.InvariantCulture);
                        points.Add(new Point(x, y));
                    }



                    storage.AddDescription(key, new SpineDescription() {
                        UpLeft = points[0],
                        DownLeft = points[1],
                        DownRight = points[2],
                        UpRight = points[3],
                    });


                    logger.Info("Read spine description {0} for file - {1}", key, filename);

                }

                foreach (var element in root.Descendants("process"))
                {

                    var key = element.Attribute("key").Value;
                    var points = new List<Point>();

                    var points_xml = element.Element("points");

                    var points_x = points_xml.Descendants("point");

                    foreach (var point in points_x)
                    {
                        var x = int.Parse(point.Attribute("x").Value, CultureInfo.InvariantCulture);
                        var y = int.Parse(point.Attribute("y").Value, CultureInfo.InvariantCulture);
                        points.Add(new Point(x, y));
                    }

                    var i = new SpinousProcessDescription()
                    {
                        UpPoint = points[0],
                        VertexPoint = points[1],
                        DownPoint = points[2],
                    };

                    logger.Info("Read process description {0} for file - {1}", key, filename);

                    storage.AddSpinousProcessDescription(key, i);

                }

            }
            catch (Exception e)
            {
                logger.Error(e, "Error parsing storage XML");
                return storage;
            }



            logger.Info("Storage has been read from file - {0}", filename);
            return storage;
        }

        public static void WriteToFile(SpineStorage storage, string filename) {
            var keys = storage.Keys;
            var sp_keys = storage.SpinousProcessKeys;

            logger.Info("Create XML for storage file - {0}", filename);

            XDocument doc = new XDocument();

            doc.Add(new XElement("storage"));

            var root = doc.Element("storage");


            #region Window Parameters

            var window = new XElement("window");

            window.Add(new XAttribute("center", storage.windowCenter));
            window.Add(new XAttribute("width", storage.windowWidth));

            root.Add(window);

            logger.Info("Add window parameters for file - {0}", filename);

            #endregion

            #region Direction Parameters

            var direction = new XElement("direction");

            direction.Add(new XAttribute("value", storage.direction));

            root.Add(direction);
            logger.Info("Add direction parameter for file - {0}", filename);

            var imageDirection = new XElement("imageDirection");

            imageDirection.Add(new XAttribute("value", storage.imageDirection));

            root.Add(imageDirection);
            logger.Info("Add image direction parameter for file - {0}", filename);

            #endregion

            #region Angle Parameters

            var angle = new XElement("angle");

            angle.Add(new XAttribute("value", storage.GetRotatingAngle()));

            root.Add(angle);
            logger.Info("Add angle parameter for file - {0}", filename);
            #endregion

            #region Marker Points
            var markers = new XElement("markers");

            if (storage.GetMarkersCount() != 0)
            {

                for (int i = 0; i < 4; i++)
                {
                    var point = storage.GetMarkerPoint(i);
                    var p = new XElement("point");
                    p.Add(new XAttribute("x", point.X));
                    p.Add(new XAttribute("y", point.Y));
                    markers.Add(p);
                }
            }

            root.Add(markers);
            logger.Info("Add markers parameter for file - {0}", filename);
            #endregion

            #region Marker Line
     
            var markerLine = storage.MarkerLine;

            if (markerLine != null)
            {
                XElement marker = new XElement("markerline");
                marker.Add(new XAttribute("size", storage.MarkerSize));
                marker.Add(new XAttribute("length", storage.MarkerLength));

                XElement point_first = new XElement("point_first");
                XElement point_second = new XElement("point_second");
                point_first.Add(new XAttribute("first", markerLine.Item1.Item1));
                point_first.Add(new XAttribute("second", markerLine.Item1.Item2));
                point_first.Add(new XAttribute("third", markerLine.Item1.Item3));

                marker.Add(point_first);

                point_second.Add(new XAttribute("first", markerLine.Item2.Item1));
                point_second.Add(new XAttribute("second", markerLine.Item2.Item2));
                point_second.Add(new XAttribute("third", markerLine.Item2.Item3));

                marker.Add(point_second);

                root.Add(marker);
            }


            #endregion

            logger.Info("Add pixel parameters for file - {0}", filename);

            foreach (var key in keys)
            {
                var state = storage.GetDescription(key);

                var element = new XElement("spine");
                var attr = new XAttribute("key", key);
                element.Add(attr);

                var points_element = new XElement("points");

                var upleft_el = new XElement("point");
                upleft_el.SetAttributeValue("x", state.UpLeft.X);
                upleft_el.SetAttributeValue("y", state.UpLeft.Y);
                var downleft_el = new XElement("point");
                downleft_el.SetAttributeValue("x", state.DownLeft.X);
                downleft_el.SetAttributeValue("y", state.DownLeft.Y);
                var downright_el = new XElement("point");
                downright_el.SetAttributeValue("x", state.DownRight.X);
                downright_el.SetAttributeValue("y", state.DownRight.Y);
                var upright_el = new XElement("point");
                upright_el.SetAttributeValue("x", state.UpRight.X);
                upright_el.SetAttributeValue("y", state.UpRight.Y);

                points_element.Add(upleft_el);
                points_element.Add(downleft_el);
                points_element.Add(downright_el);
                points_element.Add(upright_el);

                element.Add(points_element);


                var geom_element = new XElement("geometry");

                IDescriptionCalculator<SpineDescription> calc = null;

                if (storage.direction == 0)
                {
                    calc = new RightSide(state);
                }
                else if (storage.direction == 1)
                {
                    calc = new LeftSide(state);
                }
                else if (storage.direction == 2)
                {
                    calc = new FrontSide(state);
                }
                else if (storage.direction == 3)
                {
                    calc = new BackSide(state);
                }

                var ks = calc.Keys;

                foreach (var key_param in ks)
                {
                    var geom_node = new XElement("param");
                    attr = new XAttribute("key", key_param);
                    geom_node.Add(attr);
                    attr = new XAttribute("value", calc.GetParameter(key_param));
                    geom_node.Add(attr);
                    geom_element.Add(geom_node);
                }
              

                element.Add(geom_element);
                root.Add(element);
                logger.Info("Add spine {0} parameters for file - {1}", key, filename);
            }


            foreach (var key in sp_keys)
            {
                var state = storage.GetSpinousProcessDescription(key);

                var element = new XElement("process");
                var attr = new XAttribute("key", key);
                element.Add(attr);

                var points_element = new XElement("points");

                var uptp_el = new XElement("point");
                uptp_el.SetAttributeValue("x", state.UpPoint.X);
                uptp_el.SetAttributeValue("y", state.UpPoint.Y);
                var vertp_el = new XElement("point");
                vertp_el.SetAttributeValue("x", state.VertexPoint.X);
                vertp_el.SetAttributeValue("y", state.VertexPoint.Y);
                var downtp_el = new XElement("point");
                downtp_el.SetAttributeValue("x", state.DownPoint.X);
                downtp_el.SetAttributeValue("y", state.DownPoint.Y);


                points_element.Add(uptp_el);
                points_element.Add(vertp_el);
                points_element.Add(downtp_el);

                element.Add(points_element);

                var geom_element = new XElement("geometry");

                IDescriptionCalculator<SpinousProcessDescription> calc1 = null;

                if (storage.direction == 0)
                {
                    calc1 = new DescriptionCalculators.SpinousProcess.RightSide(state);
                }
                else if (storage.direction == 1)
                {
                    calc1 = new DescriptionCalculators.SpinousProcess.LeftSide(state);
                }

                var ks = calc1.Keys;

                foreach (var key_param in ks)
                {
                    var geom_node = new XElement("param");
                    attr = new XAttribute("key", key_param);
                    geom_node.Add(attr);
                    attr = new XAttribute("value", calc1.GetParameter(key_param));
                    geom_node.Add(attr);
                    geom_element.Add(geom_node);
                }


                element.Add(geom_element);
                root.Add(element);
                logger.Info("Add process {0} parameters for file - {1}", key, filename);
            }

            doc.Save(filename);

            logger.Info("Storage file has been saved - {0}", filename);
        }

        private static Tuple<short, short> ParseWindowParameters(XElement element) {
            if (element == null)
            {
                logger.Info("No window parameters in file");
                return new Tuple<short, short>(0, 0);
            }
            else
            {
                short windowCenter = 0, windowWidth = 0;
                try
                {
                    windowCenter = short.Parse(element.Attribute("center").Value, CultureInfo.InvariantCulture);
                    windowWidth = short.Parse(element.Attribute("width").Value, CultureInfo.InvariantCulture);
                    logger.Info("Read window parameters successful");
                    return new Tuple<short, short>(windowCenter, windowWidth);
                }
                catch (Exception e)
                {

                    logger.Error(e, "Read window parameters error");
                }

                return new Tuple<short, short>(0, 0);
            }

        }

        private static byte ParseDirectionParameters(XElement element)
        {
            if (element == null)
            {
                logger.Info("No direction parameters in file");
                return 0;
            }
            else
            {
                byte direction = 0;
                try
                {
                    direction = byte.Parse(element.Attribute("value").Value, CultureInfo.InvariantCulture);
                    logger.Info("Read direction parameter successful");
                    return direction;
                }
                catch (Exception e)
                {
                    logger.Error(e, "Read direction parameter error");
                }

                return direction;
            }

        }

        private static byte ParseImageDirectionParameters(XElement element)
        {
            if (element == null)
            {
                logger.Info("No image direction parameters in file");
                return 0;
            }
            else
            {
                byte direction = 0;
                try
                {
                    direction = byte.Parse(element.Attribute("value").Value, CultureInfo.InvariantCulture);
                    logger.Info("Read image direction parameter successful");
                    return direction;
                }
                catch (Exception e)
                {
                    logger.Error(e, "Read image direction parameter error");
                }

                return direction;
            }

        }

        private static int ParseAngleParameters(XElement element)
        {
            if (element == null)
            {
                logger.Info("No angle parameters in file");
                return 0;
            }
            else
            {
                int angle = 0;
                try
                {
                    angle = int.Parse(element.Attribute("value").Value, CultureInfo.InvariantCulture);
                    logger.Info("Read angle parameter successful");
                    return angle;
                }
                catch (Exception e)
                {
                    logger.Error(e, "Read angle parameter error");
                }

                return angle;
            }

        }

        private static List<Tuple<int, int>> ParseMarkers(XElement element)
        {
            Console.WriteLine("HEllo, markers");
            if (element == null)
            {
                
                logger.Info("No markers in file");
                return new List<Tuple<int, int>>();
            }
            else
            {
                var list = new List<Tuple<int, int>>();
                try
                {

                    var markers = element.Elements("point");

                    if (markers != null)
                    {
                        foreach (var marker in markers)
                        {
                            var x = int.Parse(marker.Attribute("x").Value, CultureInfo.InvariantCulture);
                            var y = int.Parse(marker.Attribute("y").Value, CultureInfo.InvariantCulture);
                            list.Add(new Tuple<int, int>(x, y));
                        }
                        logger.Info("Read markers successful");
                        return list;
                    }
                    else {
                        logger.Info("No points among markers");
                        return list;
                    }
                }
                catch (Exception e)
                {
                    logger.Error(e, "Read markers error");
                }

                return list;
            }

        }

        private static Tuple<Tuple<Tuple<int, int, int>, Tuple<int, int, int>>, double, double> ParseMarkerLine(XElement element)
        {
            Tuple<Tuple<int, int, int>, Tuple<int, int, int>> line = null;
            double markerLength = 1;
            double markerPhysSize = 1;
            
            if (element == null)
            {
                logger.Info("No marker line in file");
                return new Tuple<Tuple<Tuple<int, int, int>, Tuple<int, int, int>>, double, double>(line, markerLength, markerPhysSize);
            }
            else
            {
                try
                {
                    var point_first = element.Element("point_first");
                    var point_second = element.Element("point_second");
                    var size = double.Parse(element.Attribute("size").Value, CultureInfo.InvariantCulture);
                    var length = double.Parse(element.Attribute("length").Value, CultureInfo.InvariantCulture);

                    int f1 = int.Parse(point_first.Attribute("first").Value, CultureInfo.InvariantCulture);
                    int f2 = int.Parse(point_first.Attribute("second").Value, CultureInfo.InvariantCulture);
                    int f3 = int.Parse(point_first.Attribute("third").Value, CultureInfo.InvariantCulture);

                    int s1 = int.Parse(point_second.Attribute("first").Value, CultureInfo.InvariantCulture);
                    int s2 = int.Parse(point_second.Attribute("second").Value, CultureInfo.InvariantCulture);
                    int s3 = int.Parse(point_second.Attribute("third").Value, CultureInfo.InvariantCulture);

                    var fst = new Tuple<int, int, int>(f1, f2, f3);
                    var snd = new Tuple<int, int, int>(s1, s2, s3);

                    logger.Info("Read markers line successful");

                    return new Tuple<Tuple<Tuple<int, int, int>, Tuple<int, int, int>>, double, double>(
                        new Tuple<Tuple<int, int, int>, Tuple<int, int, int>>(fst, snd), length, size);
                }
                catch (Exception e)
                {
                    logger.Error(e, "Read markers line error");
                    return new Tuple<Tuple<Tuple<int, int, int>, Tuple<int, int, int>>, double, double>(line, markerLength, markerPhysSize);
                }
            }

        }
    }
}
