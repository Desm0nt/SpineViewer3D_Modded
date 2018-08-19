using System;
using SpineLib.Geometry.Descriptions;

namespace SpineLib.Geometry.ParametersCalculators.Spine
{
    public class ClineAngleCalculator : IParameterCalculator<SpineDescription>
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
            return GeometryHelper.AngleBetweenLines(description.UpperLine, description.DownLine);
        }
    }
}
