using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Rainbow.Kismet.Infrastructure;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Rainbow.Kismet.Internal
{
    internal class FileResultExecutor : FileResultExecutorBase, IActionResultExecutor<FileResult>
    {
        //public FileContentResultExecutor(ILoggerFactory loggerFactory)
        //    : base(CreateLogger<FileContentResultExecutor>(loggerFactory))
        //{ }


        public virtual Task ExecuteAsync(ActionContext context, FileResult result)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }
            
            var (range, rangeLength, serveBody) = SetHeadersAndLog(
                context,
                result,
                result.FileStream.Length,
                result.EnableRangeProcessing,
                result.LastModified,
                result.EntityTag);

            if (!serveBody)
            {
                return Task.CompletedTask;
            }

            return WriteFileAsync(context, result, range, rangeLength);
        }

        protected virtual Task WriteFileAsync(ActionContext context, FileResult result, RangeItemHeaderValue range, long rangeLength)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            if (range != null && rangeLength == 0)
            {
                return Task.CompletedTask;
            }
            
            return WriteFileAsync(context.HttpContext, result.FileStream, range, rangeLength);
        }
    }
}
