using SpineLib.Geometry.ParametersCalculators;
using SpineLib.Geometry.ParametersCalculators.SpinousProcess;
using System;
using System.Collections.Generic;
using SpineLib.Geometry.Descriptions;

namespace SpineLib.Geometry.DescriptionCalculators.SpinousProcess
{
    public class RightSide : IDescriptionCalculator<SpinousProcessDescription>
    {

        private SpinousProcessDescription description;
        private ISet<string> keys;

        private Dictionary<string, IParameterCalculator<SpinousProcessDescription>> parameters;
        private Dictionary<string, string> names;

        public RightSide(SpinousProcessDescription description) {
            parameters = new Dictionary<string, IParameterCalculator<SpinousProcessDescription>>();
            names = new Dictionary<string, string>();
            keys = new SortedSet<string>();
            this.description = description;
            this.description.Direction = 0;

            IParameterCalculator<SpinousProcessDescription> param = new AngleCalculator();
            param.Description = description;
            parameters["alpha"] = param;
            names["alpha"] = "Угол между остистыми отростками";
            keys.Add("alpha");


        }

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
                    case "alpha":
                        {
                            return Math.Abs(parameters[key].Calculate());
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
            return false;
        }
    }
}
