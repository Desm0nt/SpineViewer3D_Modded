using System;

namespace Volot.DescriptionOfGeometry.STL
{
    public class StlReadException : Exception
    {
        public StlReadException()
        {}

        public StlReadException(string message)
            : base(message)
        {}

        public StlReadException(string message, Exception innerException)
            : base(message, innerException)
        {}
    }
}