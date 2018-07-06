using System;

namespace Rainbow.Kismet.Exceptions
{
    public class XmlResolverException : KismetException
    {
        public XmlResolverException()
            : base()
        { }

        public XmlResolverException(string message)
            : base(message)
        { }

        public XmlResolverException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
