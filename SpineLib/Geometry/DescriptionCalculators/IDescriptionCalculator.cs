using System.Collections.Generic;

namespace SpineLib.Geometry
{
    public interface IDescriptionCalculator<T>
    {
        ISet<string> Keys { get;}

        T Description { get; set; }

        bool IsParameterLinear(string key);

        double GetParameter(string key);

        string GetParameterDescription(string key);
    }
}
