using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpineLib.Geometry.ParametersCalculators
{
    public interface IParameterCalculator<T>
    {
        T Description { get; set; }

        double Calculate();

    }
}
