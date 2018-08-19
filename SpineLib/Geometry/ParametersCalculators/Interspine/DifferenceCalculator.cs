using System;
using System.Drawing;
using SpineLib.Geometry.Descriptions;

namespace SpineLib.Geometry.ParametersCalculators.Interspine
{
    public class DifferenceCalculator : IParameterCalculator<InterspineDescription>
    {

        private InterspineDescription description;

        public IParameterCalculator<InterspineDescription> LeftSide { get; set; }

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

            var up_point = upspine.DownLeft;
            var down_point = downspine.UpLeft;

            var dist = GeometryHelper.Distance(new PointF(up_point.X,
                                                          up_point.Y),
                                               new PointF(down_point.X,
                                                          down_point.Y));

            LeftSide.Description = description;
            var d1 = LeftSide.Calculate();

            return (float)Math.Sqrt(dist * dist + d1 * d1);

        }
    }
}
