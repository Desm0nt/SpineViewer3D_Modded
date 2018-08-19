using SpineLib.Geometry.ParametersCalculators;
using SpineLib.Geometry.ParametersCalculators.Interspine;
using SpineLib.Geometry.Descriptions;
using System;
using System.Collections.Generic;

namespace SpineLib.Geometry.DescriptionCalculators.Interspine
{
    public class BackSide : IDescriptionCalculator<InterspineDescription>
    {

        private InterspineDescription description;
        private ISet<string> keys;

        private Dictionary<string, IParameterCalculator<InterspineDescription>> parameters;
        private Dictionary<string, string> names;

        public BackSide(InterspineDescription description) {
            parameters = new Dictionary<string, IParameterCalculator<InterspineDescription>>();
            names = new Dictionary<string, string>();
            keys = new SortedSet<string>();
            this.description = description;

            IParameterCalculator<InterspineDescription> param = new LeftSideCalculator();
            param.Description = description;
            parameters["d_1"] = param;
            names["d_1"] = "Высота правого контура";
            keys.Add("d_1");

            var param1 = new DifferenceCalculator();
            param1.LeftSide = param;
            param.Description = description;
            parameters["s"] = param;
            names["s"] = "Линейное смещение тела позвонка";
            keys.Add("s");

            param = new RightSideCalculator();
            param.Description = description;
            parameters["d_2"] = param;
            names["d_2"] = "Высота левого контура";
            keys.Add("d_2");

            param = new ClineAngleCalculator();
            param.Description = description;
            parameters["alpha_d"] = param;
            names["alpha_d"] = "Угол клиновидности диска";
            keys.Add("alpha_d");

            param = new SpinesAngleCalculator();
            param.Description = description;
            parameters["alpha_m"] = param;
            names["alpha_m"] = "Угол между телами позвонков";
            keys.Add("alpha_m");
        }

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
                    case "alpha_d":
                    case "alpha_m":
                    case "s":
                        {
                            var leftside = parameters["d_1"].Calculate();
                            var rightside = parameters["d_2"].Calculate();
                            if (leftside < rightside)
                            {
                                return Math.Abs(parameters[key].Calculate());
                            }
                            else {
                                return -1 * Math.Abs(parameters[key].Calculate());
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
                case "d_1":
                case "d_2":
                case "s":
                    return true;
                default:
                    return false;
            }
        }
    }
}
