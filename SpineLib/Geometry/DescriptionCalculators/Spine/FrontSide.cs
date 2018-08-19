using SpineLib.Geometry.ParametersCalculators;
using SpineLib.Geometry.ParametersCalculators.Spine;
using System;
using System.Collections.Generic;
using SpineLib.Geometry.Descriptions;

namespace SpineLib.Geometry.DescriptionCalculators.Spine
{
    public class FrontSide : IDescriptionCalculator<SpineDescription>
    {

        private SpineDescription description;
        private ISet<string> keys;

        private Dictionary<string, IParameterCalculator<SpineDescription>> parameters;
        private Dictionary<string, string> names;

        public FrontSide(SpineDescription description) {
            parameters = new Dictionary<string, IParameterCalculator<SpineDescription>>();
            names = new Dictionary<string, string>();
            keys = new SortedSet<string>();
            this.description = description;

            IParameterCalculator<SpineDescription> param = new LeftSideCalculator();
            param.Description = description;
            parameters["h_a"] = param;
            names["h_a"] = "Высота левого контура";
            keys.Add("h_a");

            param = new RightSideCalculator();
            param.Description = description;
            parameters["h_b"] = param;
            names["h_b"] = "Высота правого контура";
            keys.Add("h_b");

            param = new UpSideCalculator();
            param.Description = description;
            parameters["l_a"] = param;
            names["l_a"] = "Длина покровной замыкательной пластинки";
            keys.Add("l_a");

            param = new DownSideCalculator();
            param.Description = description;
            parameters["l_b"] = param;
            names["l_b"] = "Длина базальной замыкательной пластинки";
            keys.Add("l_b");

            param = new TrapezeAngleCalculator();
            param.Description = description;
            parameters["alpha_t"] = param;
            names["alpha_t"] = "Угол трапецевидности";
            keys.Add("alpha_t");

            param = new ClineAngleCalculator();
            param.Description = description;
            parameters["alpha_p"] = param;
            names["alpha_p"] = "Угол клиновидности";
            keys.Add("alpha_p");

            param = new VerticalAngleCalculator();
            param.Description = description;
            parameters["alpha_v"] = param;
            names["alpha_v"] = "Угол наклона к вертикали";
            keys.Add("alpha_v");

            param = new HorizontalAngleCalculator();
            param.Description = description;
            parameters["alpha_h"] = param;
            names["alpha_h"] = "Угол наклона к горизонтали";
            keys.Add("alpha_h");
        }

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

        public ISet<string> Keys
        {
            get
            {
                return keys;
            }
        }

        public double GetParameter(string key)
        {
            if (parameters.ContainsKey(key))
            {
                switch (key)
                {
                    case "alpha_t":
                        {
                            var downside = GeometryHelper.Distance(description.DownLeft, description.DownRight);
                            var upside = GeometryHelper.Distance(description.UpLeft, description.UpRight);
                            if (downside < upside)
                            {
                                return -1 * Math.Abs(parameters[key].Calculate());
                            }
                            else {
                                return Math.Abs(parameters[key].Calculate());
                            }
                        }
                    case "alpha_p":
                        {
                            var leftside = GeometryHelper.Distance(description.DownLeft, description.UpLeft);
                            var rightside = GeometryHelper.Distance(description.DownRight, description.UpRight);
                            if (leftside < rightside)
                            {
                                return -1 * Math.Abs(parameters[key].Calculate());
                            }
                            else
                            {
                                return Math.Abs(parameters[key].Calculate());
                            }
                        }
                    default:
                        return parameters[key].Calculate();
                }
                
            }
            else {
                throw new ArgumentException("Wrong parameter");
            }
        }

        public string GetParameterDescription(string key)
        {
            if (parameters.ContainsKey(key))
            {
                return names[key];
            }
            else
            {
                throw new ArgumentException("Wrong parameter");
            }
        }

        public bool IsParameterLinear(string key)
        {
            switch (key)
            {
                case "l_a":
                case "l_b":
                case "h_a":
                case "h_b":
                    return true;
                default:
                    return false;
            }
        }
    }
}
