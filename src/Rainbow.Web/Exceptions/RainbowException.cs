using System;

namespace Rainbow.Web
{
    internal class RainbowException : Exception
    {
        public RainbowException()
            : base()
        { }

        public RainbowException(string message)
            : base(message)
        { }

        public RainbowException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}