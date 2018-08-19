using System;
using System.Drawing;
using SpineLib.Geometry.Descriptions;

namespace SpineLib.Geometry.ParametersCalculators.Interspine
{
    public class RightSideCalculator : IParameterCalculator<InterspineDescription>
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

            var point = downspine.UpRight;
            var line = upspine.DownLine;

            var fpoint = new PointF(point.X, point.Y);

            return GeometryHelper.PointLineDistance(fpoint, line);

        }
    }
}
