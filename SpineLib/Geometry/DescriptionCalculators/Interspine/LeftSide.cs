using SpineLib.Geometry.ParametersCalculators;
using SpineLib.Geometry.ParametersCalculators.Interspine;
using System;
using System.Collections.Generic;
using SpineLib.Geometry.Descriptions;

namespace SpineLib.Geometry.DescriptionCalculators.Interspine
{
    public class LeftSide : IDescriptionCalculator<InterspineDescription>
    {

        private InterspineDescription description;
        private ISet<string> keys;

        private Dictionary<string, IParameterCalculator<InterspineDescription>> parameters;
        private Dictionary<string, string> names;

        public LeftSide(InterspineDescription description) {
            parameters = new Dictionary<string, IParameterCalculator<InterspineDescription>>();
            names = new Dictionary<string, string>();
            keys = new SortedSet<string>();
            this.description = description;

            IParameterCalculator<InterspineDescription> param = new LeftSideCalculator();
            param.Description = description;
            parameters["d_1"] = param;
            names["d_1"] = "Высота вентрального контура";
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
            names["d_2"] = "Высота дорсального контура";
            keys.Add("d_2");

            param = new DifferenceAngleCalculator();        
            param.Description = description;
            parameters["alpha_s"] = param;
            names["alpha_s"] = "Угол смещения позвонка";
            keys.Add("alpha_s");

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
                                return -1 * Math.Abs(parameters[key].Calculate());
                            }
                            else {
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
