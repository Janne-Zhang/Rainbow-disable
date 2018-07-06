using Microsoft.Extensions.Logging;
using Rainbow.Kismet.Infrastructure;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Rainbow.Kismet.Internal
{
    public class ContentResultExecutor : IActionResultExecutor<ContentResult>
    {
        private const string DefaultContentType = "text/plain; charset=utf-8";
        //private readonly ILogger<ContentResultExecutor> _logger;
        private readonly IHttpResponseStreamWriterFactory _httpResponseStreamWriterFactory;


        //public ContentResultExecutor(ILogger<ContentResultExecutor> logger, IHttpResponseStreamWriterFactory httpResponseStreamWriterFactory)
        //{
        //    _logger = logger;
        //    _httpResponseStreamWriterFactory = httpResponseStreamWriterFactory;
        //}

        public ContentResultExecutor(IHttpResponseStreamWriterFactory httpResponseStreamWriterFactory)
        {
            _httpResponseStreamWriterFactory = httpResponseStreamWriterFactory;
        }

        public virtual async Task ExecuteAsync(ActionContext context, ContentResult result)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            var response = context.HttpContext.Response;

            string resolvedContentType;
            Encoding resolvedContentTypeEncoding;
            ResponseContentTypeHelper.ResolveContentTypeAndEncoding(
                result.ContentType,
                response.ContentType,
                DefaultContentType,
                out resolvedContentType,
                out resolvedContentTypeEncoding);

            response.ContentType = resolvedContentType;

            if (result.StatusCode != null)
            {
                response.StatusCode = result.StatusCode.Value;
            }

            if (result.Content != null)
            {
                response.ContentLength = resolvedContentTypeEncoding.GetByteCount(result.Content);

                using (var textWriter = _httpResponseStreamWriterFactory.CreateWriter(response.Body, resolvedContentTypeEncoding))
                {
                    await textWriter.WriteAsync(result.Content);
                    await textWriter.FlushAsync();
                }
            }
        }
    }
}
