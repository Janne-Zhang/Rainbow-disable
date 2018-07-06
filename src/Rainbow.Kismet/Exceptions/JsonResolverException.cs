using System;

namespace Rainbow.Kismet.Exceptions
{
    public class JsonResolverException : KismetException
    {
        public JsonResolverException()
            : base()
        { }

        public JsonResolverException(string message)
            : base(message)
        { }

        public JsonResolverException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
