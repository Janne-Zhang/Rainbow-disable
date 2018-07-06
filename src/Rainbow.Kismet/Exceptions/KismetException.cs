using System;

namespace Rainbow.Kismet.Exceptions
{
    public class KismetException : Exception
    {
        public KismetException()
            : base()
        { }

        public KismetException(string message)
            : base(message)
        { }

        public KismetException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
