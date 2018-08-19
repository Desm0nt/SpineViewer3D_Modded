using System.Drawing;
using SpineLib.Geometry.Descriptions;

namespace SpineLib.Geometry.ParametersCalculators.Spine
{
    public class HorizontalAngleCalculator : IParameterCalculator<SpineDescription>
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
            var leftMiddle = SpineDescription.GetLineMiddle(description.DownRight, description.UpRight);
            var rightMiddle = SpineDescription.GetLineMiddle(description.DownLeft, description.UpLeft);

            var horizLine = GeometryHelper.GetLineFromPoints(description.UpLeft, new PointF(description.UpLeft.X + 20, description.UpLeft.Y));
            var middleLine = GeometryHelper.GetLineFromPoints(leftMiddle, rightMiddle);

            return GeometryHelper.AngleBetweenLines(horizLine, middleLine);
        }
    }
}
