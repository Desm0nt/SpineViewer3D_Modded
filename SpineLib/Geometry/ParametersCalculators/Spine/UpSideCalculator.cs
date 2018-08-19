using System;
using SpineLib.Geometry.Descriptions;

namespace SpineLib.Geometry.ParametersCalculators.Spine
{
    public class UpSideCalculator : IParameterCalculator<SpineDescription>
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
            var dx = (description.UpLeft.X - description.UpRight.X) * 1.0;
            var dy = (description.UpLeft.Y - description.UpRight.Y) * 1.0;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }
    }
}
