using SpineLib.Geometry.Descriptions;

namespace SpineLib.Geometry.ParametersCalculators.SpinousProcess
{
    public class AngleCalculator : IParameterCalculator<SpinousProcessDescription>
    {

        private SpinousProcessDescription description;

        public SpinousProcessDescription Description
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
