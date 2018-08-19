using System;
using System.Drawing;

namespace SpineLib.Geometry.Descriptions
{
    public class SpinousProcessDescription
    {

        private int direction = 0;

        public Point VertexPoint { get; set; }
        public Point UpPoint { get; set; }
        public Point DownPoint { get; set; }

        public int Direction {
            get {
                return direction;
            }
            set
            {
                direction = value;
            }
        }

        public Tuple<float, float> UpperLine
        {
            get
            {
                return GeometryHelper.GetLineFromPoints(VertexPoint, UpPoint);
            }
        }

        public Tuple<float, float> DownLine
        {
            get
            {
                return GeometryHelper.GetLineFromPoints(VertexPoint, DownPoint);
            }
        }
    }
}
