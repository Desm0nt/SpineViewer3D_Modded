using System;
using System.Drawing;
using SpineLib.Geometry.Descriptions;

namespace SpineLib.Geometry.ParametersCalculators.Interspine
{
    public class DifferenceAngleCalculator : IParameterCalculator<InterspineDescription>
    {

        private InterspineDescription description;

        public InterspineDescription Description
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
            if (description.UpSpine == null || description.DownSpine == null || description.storage == null)
            {
                throw new ArgumentNullException("Fill all properties");
            }
            if (!description.storage.ContainDescription(description.UpSpine) || !description.storage.ContainDescription(description.DownSpine))
            {
                throw new ArgumentException("Spine not in storage");
            }

            var upspine = description.storage.GetDescription(description.UpSpine);
            var downspine = description.storage.GetDescription(description.DownSpine);

            var up_point = new PointF(upspine.DownLeft.X, upspine.DownLeft.Y);
            var down_point = new PointF(downspine.UpLeft.X, downspine.UpLeft.Y);

            var upline = GeometryHelper.GetLineFromPoints(up_point, down_point);
            var downline = downspine.UpperLine;

            return GeometryHelper.AngleBetweenLines(upline, downline);

        }
    }
}
