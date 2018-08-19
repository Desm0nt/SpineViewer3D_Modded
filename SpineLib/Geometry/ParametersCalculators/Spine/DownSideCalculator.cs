using System;
using SpineLib.Geometry.Descriptions;

namespace SpineLib.Geometry.ParametersCalculators.Spine
{
    public class DownSideCalculator : IParameterCalculator<SpineDescription>
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
            var dx = (description.DownLeft.X - description.DownRight.X) * 1.0;
            var dy = (description.DownLeft.Y - description.DownRight.Y) * 1.0;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }
    }
}
