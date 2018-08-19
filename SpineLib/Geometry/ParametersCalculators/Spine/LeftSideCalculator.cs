using System;
using SpineLib.Geometry.Descriptions;

namespace SpineLib.Geometry.ParametersCalculators.Spine
{
    public class LeftSideCalculator : IParameterCalculator<SpineDescription>
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
            var dx = (description.UpLeft.X - description.DownLeft.X) * 1.0f;
            var dy = (description.UpLeft.Y - description.DownLeft.Y) * 1.0f;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }

        public static implicit operator LeftSideCalculator(RightSideCalculator v)
        {
            throw new NotImplementedException();
        }
    }
}
