using System;
using System.Drawing;

namespace SpineLib.Geometry.Descriptions
{
    public class SpineDescription
    {
        public Point UpLeft { get; set; }
        public Point DownLeft { get; set; }
        public Point DownRight { get; set; }
        public Point UpRight { get; set; }

        public static PointF GetLineMiddle(PointF p1, PointF p2) {
            var x = (p1.X + p2.X) * 1.0f / 2;
            var y = (p1.Y + p2.Y) * 1.0f / 2;
            return new PointF(x, y);
        }

        public Tuple<float, float> UpperLine
        {
            get
            {
                return GeometryHelper.GetLineFromPoints(UpLeft, UpRight);
            }
        }
        public Tuple<float, float> DownLine
        {
            get
            {
                return GeometryHelper.GetLineFromPoints(DownLeft, DownRight);
            }
        }
        public Tuple<float, float> RightLine
        {
            get
            {
                return GeometryHelper.GetLineFromPoints(UpRight, DownRight);
            }
        }
        public Tuple<float, float> LeftLine
        {
            get
            {
                return GeometryHelper.GetLineFromPoints(UpLeft, DownLeft);
            }
        }

    }

}
