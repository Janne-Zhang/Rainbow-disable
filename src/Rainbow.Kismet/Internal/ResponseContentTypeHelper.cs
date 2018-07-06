using Rainbow.Kismet.Formatters;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rainbow.Kismet.Internal
{
    public static class ResponseContentTypeHelper
    {
        public static void ResolveContentTypeAndEncoding(
            string actionResultContentType,
            string httpResponseContentType,
            string defaultContentType,
            out string resolvedContentType,
            out Encoding resolvedContentTypeEncoding)
        {
            var defaultContentTypeEncoding = MediaType.GetEncoding(defaultContentType);
            
            if (actionResultContentType != null)
            {
                resolvedContentType = actionResultContentType;
                var actionResultEncoding = MediaType.GetEncoding(actionResultContentType);
                resolvedContentTypeEncoding = actionResultEncoding ?? defaultContentTypeEncoding;
                return;
            }
            
            if (!string.IsNullOrEmpty(httpResponseContentType))
            {
                var mediaTypeEncoding = MediaType.GetEncoding(httpResponseContentType);
                if (mediaTypeEncoding != null)
                {
                    resolvedContentType = httpResponseContentType;
                    resolvedContentTypeEncoding = mediaTypeEncoding;
                }
                else
                {
                    resolvedContentType = httpResponseContentType;
                    resolvedContentTypeEncoding = defaultContentTypeEncoding;
                }

                return;
            }
            
            resolvedContentType = defaultContentType;
            resolvedContentTypeEncoding = defaultContentTypeEncoding;
        }
    }
}
