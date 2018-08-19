using System;
using System.Drawing;
using SpineLib.Geometry.Descriptions;

namespace SpineLib.Geometry.ParametersCalculators.Spine
{
    public class VerticalAngleCalculator : IParameterCalculator<SpineDescription>
    {

        private SpineDescription description;

        public SpineDescription Description
        {
            get
            {
                return description;
            }

            set
            {
                description = value;
            }
        }

        public double Calculate()
        {
            var upMiddle = SpineDescription.GetLineMiddle(description.UpLeft, description.UpRight);
            var downMiddle = SpineDescription.GetLineMiddle(description.DownLeft, description.DownRight);

            var vertline = GeometryHelper.GetLineFromPoints(description.UpLeft, new PointF(description.UpLeft.X, description.UpLeft.Y + 20));
            var middleLine = GeometryHelper.GetLineFromPoints(upMiddle, downMiddle);

            return GeometryHelper.AngleBetweenLines(vertline, middleLine);
        }
    }
}
