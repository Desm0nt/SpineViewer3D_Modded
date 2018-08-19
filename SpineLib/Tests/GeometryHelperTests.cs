using NUnit.Framework;
using SpineLib.Geometry;
using System;
using System.Drawing;

namespace SpineLib.Tests
{
    [TestFixture]
    public class GeometryHelperTests
    {
        [Test]
        public void TestPointLineDistanceVerticalLinePos()
        {
            var point = new System.Drawing.PointF(0, 0);
            var vert_line = new Tuple<float, float>(float.PositiveInfinity, 10);
            var dist = GeometryHelper.PointLineDistance(point, vert_line);
            Assert.AreEqual(10.0f, dist, 0.001);
        }

        [Test]
        public void TestPointLineDistanceVerticalLineNeg()
        {
            var point = new System.Drawing.PointF(0, 0);
            var vert_line = new Tuple<float, float>(float.NegativeInfinity, 10);
            var dist = GeometryHelper.PointLineDistance(point, vert_line);
            Assert.AreEqual(10.0f, dist, 0.001);
        }

        [Test]
        public void TestPointLineDistanceHorizontalLineUp()
        {
            var point = new System.Drawing.PointF(0, 0);
            var vert_line = new Tuple<float, float>(0, 10);
            var dist = GeometryHelper.PointLineDistance(point, vert_line);
            Assert.AreEqual(10.0f, dist, 0.001);
        }

        [Test]
        public void TestPointLineDistanceHorizontalLineDown()
        {
            var point = new System.Drawing.PointF(0, 0);
            var vert_line = new Tuple<float, float>(0, -10);
            var dist = GeometryHelper.PointLineDistance(point, vert_line);
            Assert.AreEqual(10.0f, dist, 0.001);
        }

        [Test]
        public void TestPointLineDistanceSimple()
        {
            var point = new System.Drawing.PointF(0, 0);
            var vert_line = new Tuple<float, float>(-1, 10);
            var dist = GeometryHelper.PointLineDistance(point, vert_line);
            Assert.AreEqual(10.0f * (float)Math.Sqrt(2) / 2.0f, dist, 0.001);
        }

        [Test]
        public void TestAngleBetweenVerticalLinesPosPos()
        {

            var vert_line1 = new Tuple<float, float>(float.PositiveInfinity, 10);

            var vert_line2 = new Tuple<float, float>(float.PositiveInfinity, -10);

            var angle = GeometryHelper.AngleBetweenLines(vert_line1, vert_line2);

            Assert.AreEqual(0.0f, angle, 0.001);

        }

        [Test]
        public void TestAngleBetweenVerticalLinesPosNeg()
        {

            var vert_line1 = new Tuple<float, float>(float.PositiveInfinity, 10);

            var vert_line2 = new Tuple<float, float>(float.NegativeInfinity, -10);

            var angle = GeometryHelper.AngleBetweenLines(vert_line1, vert_line2);

            Assert.AreEqual(0.0f, angle, 0.001);

        }

        [Test]
        public void TestAngleBetweenVerticalLinesNegNeg()
        {

            var vert_line1 = new Tuple<float, float>(float.NegativeInfinity, 10);

            var vert_line2 = new Tuple<float, float>(float.NegativeInfinity, -10);

            var angle = GeometryHelper.AngleBetweenLines(vert_line1, vert_line2);

            Assert.AreEqual(0.0f, angle, 0.001);

        }

        [Test]
        public void TestAngleBetweenVerticalLinesNegPos()
        {

            var vert_line1 = new Tuple<float, float>(float.PositiveInfinity, 10);

            var vert_line2 = new Tuple<float, float>(float.NegativeInfinity, -10);

            var angle = GeometryHelper.AngleBetweenLines(vert_line1, vert_line2);

            Assert.AreEqual(0.0f, angle, 0.001);

        }

        [Test]
        public void TestAngleBetweenVerticalLineHorizontalLine()
        {

            var vert_line1 = new Tuple<float, float>(float.PositiveInfinity, 10);

            var vert_line2 = new Tuple<float, float>(0, -10);

            var angle = GeometryHelper.AngleBetweenLines(vert_line1, vert_line2);

            Assert.AreEqual(90.0f, angle, 0.001);

        }

        [Test]
        public void TestAngleBetweenVerticalLineSlopeLine()
        {

            var vert_line1 = new Tuple<float, float>(float.PositiveInfinity, 10);

            var vert_line2 = new Tuple<float, float>(1, -10);

            var angle = GeometryHelper.AngleBetweenLines(vert_line1, vert_line2);

            Assert.AreEqual(45.0f, angle, 0.001);
        }

        [Test]
        public void TestAngleBetweenHorizontalLineSlopeLine()
        {

            var vert_line1 = new Tuple<float, float>(0, 10);

            var vert_line2 = new Tuple<float, float>(1, -10);

            var angle = GeometryHelper.AngleBetweenLines(vert_line1, vert_line2);

            Assert.AreEqual(45.0f, angle, 0.001);
        }

        [Test]
        public void TestAngleBetweenSlopeLines1()
        {

            var vert_line1 = new Tuple<float, float>(1, 10);

            var vert_line2 = new Tuple<float, float>(6, -10);

            var angle = GeometryHelper.AngleBetweenLines(vert_line1, vert_line2);

            Assert.AreEqual(35.5376f, angle, 0.0001);
        }

        [Test]
        public void TestAngleBetweenSlopeLines2()
        {

            var vert_line1 = new Tuple<float, float>(-1, 10);

            var vert_line2 = new Tuple<float, float>(1, -10);

            var angle = GeometryHelper.AngleBetweenLines(vert_line1, vert_line2);

            Assert.AreEqual(90f, angle, 0.0001);
        }

        [Test]
        public void TestAngleBetweenSlopeLines2Inv()
        {

            var vert_line1 = new Tuple<float, float>(1, 10);

            var vert_line2 = new Tuple<float, float>(-1, -10);

            var angle = GeometryHelper.AngleBetweenLines(vert_line1, vert_line2);

            Assert.AreEqual(-90f, angle, 0.0001);
        }

        [Test]
        public void TestAngleBetweenSlopeLines3()
        {

            var vert_line1 = new Tuple<float, float>(-5, 10);

            var vert_line2 = new Tuple<float, float>(-1, -10);

            var angle = GeometryHelper.AngleBetweenLines(vert_line1, vert_line2);

            Assert.AreEqual(33.6900f, angle, 0.0001);
        }

        [Test]
        public void TestAngleBetweenSlopeLines3Inv()
        {

            var vert_line1 = new Tuple<float, float>(-1, 10);

            var vert_line2 = new Tuple<float, float>(-5, -10);

            var angle = GeometryHelper.AngleBetweenLines(vert_line1, vert_line2);

            Assert.AreEqual(-33.6900f, angle, 0.0001);
        }

        [Test]
        public void TestAngleBetweenSlopeLines4()
        {

            var vert_line1 = new Tuple<float, float>(-8, 10);

            var vert_line2 = new Tuple<float, float>(-12, -10);

            var angle = GeometryHelper.AngleBetweenLines(vert_line1, vert_line2);

            Assert.AreEqual(-2.3613f, angle, 0.0001);
        }

        [Test]
        public void TestAngleBetweenSlopeLines4Inv()
        {

            var vert_line1 = new Tuple<float, float>(-12, 10);

            var vert_line2 = new Tuple<float, float>(-8, -10);

            var angle = GeometryHelper.AngleBetweenLines(vert_line1, vert_line2);

            Assert.AreEqual(2.3613f, angle, 0.0001);
        }

        [Test]
        public void TestPerpendicularToVertical()
        {

            var orig = new Tuple<float, float>(float.PositiveInfinity, 10);

            var perpendicular_calc = GeometryHelper.GetPerpendicularLine(orig);
            var perpendicular_expected = new Tuple<float, float>(0, 10);
            
            Assert.AreEqual(perpendicular_expected, perpendicular_calc);
        }

        [Test]
        public void TestPerpendicularToHorizontal()
        {

            var orig = new Tuple<float, float>(0, 10);

            var perpendicular_calc = GeometryHelper.GetPerpendicularLine(orig);
            var perpendicular_expected = new Tuple<float, float>(float.PositiveInfinity, 10);

            Assert.AreEqual(perpendicular_expected, perpendicular_calc);
        }

        [Test]
        public void TestPerpendicularToPositive1()
        {

            var orig = new Tuple<float, float>(10, 10);

            var perpendicular_calc = GeometryHelper.GetPerpendicularLine(orig);
            var perpendicular_expected = new Tuple<float, float>(-0.1f, 10);

            Assert.AreEqual(perpendicular_expected, perpendicular_calc);
        }

        [Test]
        public void TestPerpendicularToPositive2()
        {

            var orig = new Tuple<float, float>(1, 10);

            var perpendicular_calc = GeometryHelper.GetPerpendicularLine(orig);
            var perpendicular_expected = new Tuple<float, float>(-1f, 10);

            Assert.AreEqual(perpendicular_expected, perpendicular_calc);
        }

        [Test]
        public void TestPerpendicularToPositive3()
        {

            var orig = new Tuple<float, float>(2, 10);

            var perpendicular_calc = GeometryHelper.GetPerpendicularLine(orig);
            var perpendicular_expected = new Tuple<float, float>(-0.5f, 10);

            Assert.AreEqual(perpendicular_expected, perpendicular_calc);
        }

        [Test]
        public void TestPerpendicularToNegative1()
        {

            var orig = new Tuple<float, float>(-1, 10);

            var perpendicular_calc = GeometryHelper.GetPerpendicularLine(orig);
            var perpendicular_expected = new Tuple<float, float>(1, 10);

            Assert.AreEqual(perpendicular_expected, perpendicular_calc);
        }

        [Test]
        public void TestPerpendicularToNegative2()
        {

            var orig = new Tuple<float, float>(-2, 10);

            var perpendicular_calc = GeometryHelper.GetPerpendicularLine(orig);
            var perpendicular_expected = new Tuple<float, float>(0.5f, 10);

            Assert.AreEqual(perpendicular_expected, perpendicular_calc);
        }

        [Test]
        public void TestPerpendicularToNegative3()
        {

            var orig = new Tuple<float, float>(-0.25f, 10);

            var perpendicular_calc = GeometryHelper.GetPerpendicularLine(orig);
            var perpendicular_expected = new Tuple<float, float>(4, 10);

            Assert.AreEqual(perpendicular_expected, perpendicular_calc);
        }

        [Test]
        public void TestDistanceSamePoint() {
            var point1 = new Point(0, 0);
            var point2 = new Point(0, 0);

            Assert.AreEqual(0, GeometryHelper.Distance(point1, point2), 0.0001f);
        }

        [Test]
        public void TestDistanceHorizontal()
        {
            var point1 = new Point(1, 0);
            var point2 = new Point(10, 0);

            Assert.AreEqual(9, GeometryHelper.Distance(point1, point2), 0.0001f);
        }

        [Test]
        public void TestDistanceVertical()
        {
            var point1 = new Point(0, -10);
            var point2 = new Point(0, 10);

            Assert.AreEqual(20, GeometryHelper.Distance(point1, point2), 0.0001f);
        }

        [Test]
        public void TestDistanceGeneric1()
        {
            var point1 = new Point(0, 0);
            var point2 = new Point(2, 2);

            Assert.AreEqual((float)Math.Sqrt(2) * 2, GeometryHelper.Distance(point1, point2), 0.0001f);
        }

        [Test]
        public void TestDistanceGeneric2()
        {
            var point1 = new Point(-2, -2);
            var point2 = new Point(2, 2);

            Assert.AreEqual((float)Math.Sqrt(2) * 4, GeometryHelper.Distance(point1, point2), 0.0001f);
        }

        [Test]
        public void TestDistanceFloatSamePoint()
        {
            var point1 = new PointF(0, 0);
            var point2 = new PointF(0, 0);

            Assert.AreEqual(0, GeometryHelper.Distance(point1, point2), 0.0001f);
        }

        [Test]
        public void TestDistanceFloatHorizontal()
        {
            var point1 = new PointF(1, 0);
            var point2 = new PointF(10, 0);

            Assert.AreEqual(9, GeometryHelper.Distance(point1, point2), 0.0001f);
        }

        [Test]
        public void TestDistanceFloatVertical()
        {
            var point1 = new PointF(0, -10);
            var point2 = new PointF(0, 10);

            Assert.AreEqual(20, GeometryHelper.Distance(point1, point2), 0.0001f);
        }

        [Test]
        public void TestDistanceFloatGeneric1()
        {
            var point1 = new PointF(0, 0);
            var point2 = new PointF(2, 2);

            Assert.AreEqual((float)Math.Sqrt(2) * 2, GeometryHelper.Distance(point1, point2), 0.0001f);
        }

        [Test]
        public void TestDistanceFloatGeneric2()
        {
            var point1 = new PointF(-2, -2);
            var point2 = new PointF(2, 2);

            Assert.AreEqual((float)Math.Sqrt(2) * 4, GeometryHelper.Distance(point1, point2), 0.0001f);
        }

        [Test]
        public void TestDistanceFloatGeneric3()
        {
            var point1 = new PointF(1, 1);
            var point2 = new PointF(2.5f, 6.5f);

            Assert.AreEqual(5.7008f, GeometryHelper.Distance(point1, point2), 0.0001f);
        }

        [Test]
        public void TestLineCreatingVertical1()
        {
            var point1 = new Point(0, 0);
            var point2 = new Point(0, 10);

            var line = new Tuple<float, float>(float.PositiveInfinity, 0);

            Assert.AreEqual(line, GeometryHelper.GetLineFromPoints(point1, point2));
        }

        [Test]
        public void TestLineCreatingVertical2()
        {
            var point1 = new Point(10, 0);
            var point2 = new Point(10, 10);

            var line = new Tuple<float, float>(float.PositiveInfinity, 10);

            Assert.AreEqual(line, GeometryHelper.GetLineFromPoints(point1, point2));
        }

        [Test]
        public void TestLineCreatingVertical3()
        {
            var point1 = new Point(-10, 0);
            var point2 = new Point(-10, 10);

            var line = new Tuple<float, float>(float.PositiveInfinity, -10);

            Assert.AreEqual(line, GeometryHelper.GetLineFromPoints(point1, point2));
        }

        public void TestLineCreatingHorizontal1()
        {
            var point1 = new Point(0, 0);
            var point2 = new Point(10, 0);

            var line = new Tuple<float, float>(0, 0);

            Assert.AreEqual(line, GeometryHelper.GetLineFromPoints(point1, point2));
        }

        [Test]
        public void TestLineCreatingHorizontal2()
        {
            var point1 = new Point(0, 10);
            var point2 = new Point(-10, 10);

            var line = new Tuple<float, float>(0, 10);

            Assert.AreEqual(line, GeometryHelper.GetLineFromPoints(point1, point2));
        }

        [Test]
        public void TestLineCreatingGeneric1()
        {
            var point1 = new Point(0, 0);
            var point2 = new Point(1, 1);

            var line = new Tuple<float, float>(1, 0);

            Assert.AreEqual(line, GeometryHelper.GetLineFromPoints(point1, point2));
        }

        [Test]
        public void TestLineCreatingGeneric2()
        {
            var point1 = new Point(0, 0);
            var point2 = new Point(1, -1);

            var line = new Tuple<float, float>(-1, 0);

            Assert.AreEqual(line, GeometryHelper.GetLineFromPoints(point1, point2));
        }

        [Test]
        public void TestLineCreatingGeneric3()
        {
            var point1 = new Point(0, 1);
            var point2 = new Point(1, 2);

            var line = new Tuple<float, float>(1, 1);

            Assert.AreEqual(line, GeometryHelper.GetLineFromPoints(point1, point2));
        }


        [Test]
        public void TestLineFloatCreatingVertical1()
        {
            var point1 = new PointF(0, 0);
            var point2 = new PointF(0, 10);

            var line = new Tuple<float, float>(float.PositiveInfinity, 0);

            Assert.AreEqual(line, GeometryHelper.GetLineFromPoints(point1, point2));
        }

        [Test]
        public void TestLineFloatCreatingVertical2()
        {
            var point1 = new PointF(10, 0);
            var point2 = new PointF(10, 10);

            var line = new Tuple<float, float>(float.PositiveInfinity, 2.5f);

            Assert.AreEqual(line, GeometryHelper.GetLineFromPoints(point1, point2));
        }

        [Test]
        public void TestLineFloatCreatingVertical3()
        {
            var point1 = new PointF(-10, 0);
            var point2 = new PointF(-10, 10);

            var line = new Tuple<float, float>(float.PositiveInfinity, -2.5f);

            Assert.AreEqual(line, GeometryHelper.GetLineFromPoints(point1, point2));
        }

        public void TestLineFloatCreatingHorizontal1()
        {
            var point1 = new PointF(0, 0);
            var point2 = new PointF(10, 0);

            var line = new Tuple<float, float>(0, 0);

            Assert.AreEqual(line, GeometryHelper.GetLineFromPoints(point1, point2));
        }

        [Test]
        public void TestLineFloatCreatingHorizontal2()
        {
            var point1 = new PointF(0, 10);
            var point2 = new PointF(-10, 10);

            var line = new Tuple<float, float>(0, 2.5f);

            Assert.AreEqual(line, GeometryHelper.GetLineFromPoints(point1, point2));
        }

        [Test]
        public void TestLineFloatCreatingGeneric1()
        {
            var point1 = new PointF(0, 0);
            var point2 = new PointF(1, 1);

            var line = new Tuple<float, float>(1, 0);

            Assert.AreEqual(line, GeometryHelper.GetLineFromPoints(point1, point2));
        }

        [Test]
        public void TestLineFloatCreatingGeneric2()
        {
            var point1 = new PointF(0, 0);
            var point2 = new PointF(1, -1);

            var line = new Tuple<float, float>(-1, 0);

            Assert.AreEqual(line, GeometryHelper.GetLineFromPoints(point1, point2));
        }

        [Test]
        public void TestLineFloatCreatingGeneric3()
        {
            var point1 = new PointF(0, 1);
            var point2 = new PointF(1, 2);

            var line = new Tuple<float, float>(1, 0.25f);

            Assert.AreEqual(line, GeometryHelper.GetLineFromPoints(point1, point2));
        }
    }
}
