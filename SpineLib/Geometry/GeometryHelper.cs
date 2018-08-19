using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace SpineLib.Geometry
{
    public class GeometryHelper
    {
        public static float PointLineDistance(PointF point, Tuple<float, float> line)
        {
            var m = line.Item1;
            var k = line.Item2;
            var x0 = point.X;
            var y0 = point.Y;

            if (float.IsInfinity(m)) {
                return Math.Abs(k - x0);
            }

            return (float)Math.Abs((m*x0-y0+k)/Math.Sqrt(m*m+1));
        }

        /// <summary>
        /// Calculate angle between lines
        /// If first line goes to second counterclockwise (coef increases), angle is positive
        /// If first line goes to second clockwise (coef decreases), angle is negative
        /// </summary>
        /// <param name="first">First line</param>
        /// <param name="second">Second line</param>
        /// <returns></returns>
        public static float AngleBetweenLines(Tuple<float, float> first, Tuple<float, float> second) {
            var k_r = first.Item1;
            var k_l = second.Item1;


            if (float.IsInfinity(k_r))
            {
                return (float)(Math.Atan(1 / k_l) * 180 / Math.PI);
            }
            else if (float.IsInfinity(k_l))
            {
                return (float)(Math.Atan(1 / k_r) * 180 / Math.PI);
            }

            var tn = (float)(Math.Atan((k_l - k_r) / (1 + k_l * k_r)) * 180 / Math.PI);

            if (float.IsNaN(tn))
                return 90f;
            else
                return tn;

        }

        public static Tuple<float, float> GetPerpendicularLine(Tuple<float, float> original) {
            if (float.IsInfinity(original.Item1))
            {
                return new Tuple<float, float>(0, original.Item2);
            }
            else if (original.Item1 == 0.0f)
            {
                return new Tuple<float, float>(float.PositiveInfinity, original.Item2);
            }
            else {
                return new Tuple<float, float>(-1 / original.Item1, original.Item2);
            }
        }

        public static float Distance(Point p1, Point p2)
        {
            var dx = p1.X - p2.X;
            var dy = p1.Y - p2.Y;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }

        public static float Distance(PointF p1, PointF p2)
        {
            var dx = p1.X - p2.X;
            var dy = p1.Y - p2.Y;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }

        public static Tuple<float, float> GetLineFromPoints(Point p1, Point p2)
        {
            var x1 = p1.X * 1.0f;
            var y1 = p1.Y * 1.0f;
            var x2 = p2.X * 1.0f;
            var y2 = p2.Y * 1.0f;

            float k = 0.0f;
            float b = 0.0f;
            if ((x2 - x1) != 0)
            {
                k = (y2 - y1) / (x2 - x1);
                b = (y1 - k * x1);
            }
            else
            {
                k = float.PositiveInfinity;
                b = x1;
            }

            return new Tuple<float, float>(k, b);
        }

        public static Tuple<float, float> GetLineFromPoints(PointF p1, PointF p2)
        {
            var x1 = p1.X * 1.0f;
            var y1 = p1.Y * 1.0f;
            var x2 = p2.X * 1.0f;
            var y2 = p2.Y * 1.0f;

            float k = 0.0f;
            float b = 0.0f;
            if ((x2 - x1) != 0)
            {
                k = (y2 - y1) / (x2 - x1);
                b = (y1 - k * x1);
            }
            else
            {
                k = float.PositiveInfinity;
                b = x1;
            }

            return new Tuple<float, float>(k, b);
        }

        public static Point RotatePoint(Point target, Point origin, double angle) {
            double radians = angle * Math.PI / 180;

            double newX = origin.X + (target.X - origin.X) * Math.Cos(radians) - (target.Y - origin.Y) * Math.Sin(radians);
            double newY = origin.Y + (target.X - origin.X) * Math.Sin(radians) + (target.Y - origin.Y) * Math.Cos(radians);

            return new Point((int)newX, (int)newY);
        }

        public static List<Point> RotatePoints(List<Point> target, Point origin, double angle) {
            return target.Select(x => RotatePoint(x, origin, angle)).ToList();
        }

    }
}
