using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Volot
{
    /// <summary>
    /// Цвет вершины.
    /// </summary>
    public class RColor
    {
        public float R { get; set; }
        public float G { get; set; }
        public float B { get; set; }

        public RColor(float r, float g, float b)
        {
            R = r;
            G = g;
            B = b;
        }

        public RColor()
        {
            R = Convert.ToSingle(0.752941f, CultureInfo.InvariantCulture);
            G = Convert.ToSingle(0.752941f, CultureInfo.InvariantCulture);
            B = Convert.ToSingle(0.752941f, CultureInfo.InvariantCulture);
        }
    }
}
