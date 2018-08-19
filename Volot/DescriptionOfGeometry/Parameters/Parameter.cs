using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace Volot.DescriptionOfGeometry.Parameters
{
    public class Parameter
    {
        public double PixelWidth { get; private set; }
        public double PixelHeight { get; private set; }

        public int WindowCenter { get; private set; }
        public int WindowWidth { get; private set; }
        public int Direction { get; private set; }
        public int ImageDirection { get; private set; }
        public double Angle { get; private set; }

        public int ClipHeight { get; private set; }
        public List<Vertex> Clip { get; private set; }
        public List<Vertex> Markers { get; private set; }
        public List<Spine> Spines { get; private set; }
        public List<Spine> SpinesCut { get; private set; }
        public List<Spine> ProcessSpines { get; private set; }

        public Parameter(XDocument xml)
        {
            if (xml.Root != null)
            {
                PixelWidth = Convert.ToDouble(xml.Root.Attribute("pixelWidth")?.Value, CultureInfo.InvariantCulture);
                PixelHeight = Convert.ToDouble(xml.Root.Attribute("pixelHeight")?.Value, CultureInfo.InvariantCulture);

                WindowCenter = Convert.ToInt32(xml.Root.Element("window")?.Attribute("center")?.Value, CultureInfo.InvariantCulture);
                WindowWidth =  Convert.ToInt32(xml.Root.Element("window")?.Attribute("width")?.Value, CultureInfo.InvariantCulture);
                Direction =  Convert.ToInt32(xml.Root.Element("direction")?.Attribute("value")?.Value, CultureInfo.InvariantCulture);
                ImageDirection = Convert.ToInt32(xml.Root.Element("imageDirection")?.Attribute("value")?.Value, CultureInfo.InvariantCulture);
                Angle =  Convert.ToDouble(xml.Root.Element("angle")?.Attribute("value")?.Value, CultureInfo.InvariantCulture);
                ClipHeight = Convert.ToInt32(xml.Root.Element("clip")?.Attribute("height")?.Value, CultureInfo.InvariantCulture);

                Clip = GetPoints(xml.Root.Elements("clip"));
                Markers = GetPoints(xml.Root.Elements("markers"));
                Spines = GetSpines(xml.Root.Elements("spine"));
                SpinesCut = GetSpinesCut(xml.Root.Elements("spine"));
                ProcessSpines = GetSpines(xml.Root.Elements("process"));

                //int k = 0;
            }
        }

        private List<Spine> GetSpines(IEnumerable<XElement> xSpines) => xSpines.Select(xs =>
            new Spine(xs.Attribute("key")?.Value, GetPoints(xs.Elements("points")),
                GetGeometry(xs.Elements("geometry")),Direction)).ToList();

        private List<Spine> GetSpinesCut(IEnumerable<XElement> xSpines) => xSpines.Select(xs =>
new Spine(xs.Attribute("key")?.Value, GetPoints(xs.Elements("points")),
GetGeometry(xs.Elements("geometry")), Direction, true)).ToList();


        private List<Geometry> GetGeometry(IEnumerable<XElement> xs) => xs.Elements("param")
            .Select(t => new Geometry(t.Attribute("key")?.Value, Convert.ToDouble(t.Attribute("value")?.Value, CultureInfo.InvariantCulture)))
            .ToList();

        private List<Vertex> GetPoints(IEnumerable<XElement> xs) => xs.Elements("point")
            .Select(t =>
                new Vertex(Convert.ToDouble(t.Attribute("x")?.Value), Convert.ToDouble(t.Attribute("y")?.Value, CultureInfo.InvariantCulture)))
            .ToList();
    }
}