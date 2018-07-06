using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Rainbow.Kismet.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Rainbow.Kismet.Internal
{
    internal class FileResultExecutorBase
    {
        private const string AcceptRangeHeaderValue = "bytes";

        protected const int BufferSize = 64 * 1024;

        //protected ILogger Logger { get; }

        //public FileResultExecutorBase(ILogger logger)
        //{
        //    Logger = logger;
        //}

        internal enum PreconditionState
        {
            Unspecified,
            NotModified,
            ShouldProcess,
            PreconditionFailed
        }
        
        protected virtual (RangeItemHeaderValue range, long rangeLength, bool serveBody) SetHeadersAndLog(
            ActionContext context,
            FileResult result,
            long? fileLength,
            bool enableRangeProcessing,
            DateTimeOffset? lastModified = null,
            EntityTagHeaderValue etag = null)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            SetContentType(context, result);
            SetContentDispositionHeader(context, result);

            var request = context.HttpContext.Request;
            var httpRequestHeaders = request.GetTypedHeaders();

            if (lastModified.HasValue)
            {
                lastModified = RoundDownToWholeSeconds(lastModified.Value);
            }

            var preconditionState = GetPreconditionState(httpRequestHeaders, lastModified, etag);

            var response = context.HttpContext.Response;
            SetLastModifiedAndEtagHeaders(response, lastModified, etag);
            
            if (preconditionState == PreconditionState.NotModified)
            {
                response.StatusCode = StatusCodes.Status304NotModified;
                return (range: null, rangeLength: 0, serveBody: false);
            }
            else if (preconditionState == PreconditionState.PreconditionFailed)
            {
                response.StatusCode = StatusCodes.Status412PreconditionFailed;
                return (range: null, rangeLength: 0, serveBody: false);
            }

            if (fileLength.HasValue)
            {
                response.ContentLength = fileLength.Value;

                // Handle range request
                if (enableRangeProcessing)
                {
                    SetAcceptRangeHeader(response);

                    if ((HttpMethods.IsHead(request.Method) || HttpMethods.IsGet(request.Method))
                        && (preconditionState == PreconditionState.Unspecified || preconditionState == PreconditionState.ShouldProcess)
                        && (IfRangeValid(httpRequestHeaders, lastModified, etag)))
                    {
                        return SetRangeHeaders(context, httpRequestHeaders, fileLength.Value);
                    }
                }
            }

            return (range: null, rangeLength: 0, serveBody: !HttpMethods.IsHead(request.Method));
        }

        private static void SetContentType(ActionContext context, FileResult result)
        {
            var response = context.HttpContext.Response;
            response.ContentType = result.ContentType;
        }

        private static void SetContentDispositionHeader(ActionContext context, FileResult result)
        {
            if (!string.IsNullOrEmpty(result.FileDownloadName))
            {
                var contentDisposition = new ContentDispositionHeaderValue("attachment");
                contentDisposition.SetHttpFileName(result.FileDownloadName);
                context.HttpContext.Response.Headers[HeaderNames.ContentDisposition] = contentDisposition.ToString();
            }
        }

        private static void SetLastModifiedAndEtagHeaders(HttpResponse response, DateTimeOffset? lastModified, EntityTagHeaderValue etag)
        {
            var httpResponseHeaders = response.GetTypedHeaders();
            if (lastModified.HasValue)
            {
                httpResponseHeaders.LastModified = lastModified;
            }
            if (etag != null)
            {
                httpResponseHeaders.ETag = etag;
            }
        }

        private static void SetAcceptRangeHeader(HttpResponse response)
        {
            response.Headers[HeaderNames.AcceptRanges] = AcceptRangeHeaderValue;
        }

        internal bool IfRangeValid(
            RequestHeaders httpRequestHeaders,
            DateTimeOffset? lastModified = null,
            EntityTagHeaderValue etag = null)
        {
            var ifRange = httpRequestHeaders.IfRange;
            if (ifRange != null)
            {
                if (ifRange.LastModified.HasValue)
                {
                    if (lastModified.HasValue && lastModified > ifRange.LastModified)
                    {
                        return false;
                    }
                }
                else if (etag != null && ifRange.EntityTag != null && !ifRange.EntityTag.Compare(etag, useStrongComparison: true))
                {
                    return false;
                }
            }

            return true;
        }
        
        internal PreconditionState GetPreconditionState(
            RequestHeaders httpRequestHeaders,
            DateTimeOffset? lastModified = null,
            EntityTagHeaderValue etag = null)
        {
            var ifMatchState = PreconditionState.Unspecified;
            var ifNoneMatchState = PreconditionState.Unspecified;
            var ifModifiedSinceState = PreconditionState.Unspecified;
            var ifUnmodifiedSinceState = PreconditionState.Unspecified;
            
            var ifMatch = httpRequestHeaders.IfMatch;
            if (etag != null)
            {
                ifMatchState = GetEtagMatchState(
                    useStrongComparison: true,
                    etagHeader: ifMatch,
                    etag: etag,
                    matchFoundState: PreconditionState.ShouldProcess,
                    matchNotFoundState: PreconditionState.PreconditionFailed);
            }
            
            var ifNoneMatch = httpRequestHeaders.IfNoneMatch;
            if (etag != null)
            {
                ifNoneMatchState = GetEtagMatchState(
                    useStrongComparison: false,
                    etagHeader: ifNoneMatch,
                    etag: etag,
                    matchFoundState: PreconditionState.NotModified,
                    matchNotFoundState: PreconditionState.ShouldProcess);
            }

            var now = RoundDownToWholeSeconds(DateTimeOffset.UtcNow);
            
            var ifModifiedSince = httpRequestHeaders.IfModifiedSince;
            if (lastModified.HasValue && ifModifiedSince.HasValue && ifModifiedSince <= now)
            {
                var modified = ifModifiedSince < lastModified;
                ifModifiedSinceState = modified ? PreconditionState.ShouldProcess : PreconditionState.NotModified;
            }
            
            var ifUnmodifiedSince = httpRequestHeaders.IfUnmodifiedSince;
            if (lastModified.HasValue && ifUnmodifiedSince.HasValue && ifUnmodifiedSince <= now)
            {
                var unmodified = ifUnmodifiedSince >= lastModified;
                ifUnmodifiedSinceState = unmodified ? PreconditionState.ShouldProcess : PreconditionState.PreconditionFailed;
            }

            var state = GetMaxPreconditionState(ifMatchState, ifNoneMatchState, ifModifiedSinceState, ifUnmodifiedSinceState);
            return state;
        }

        private static PreconditionState GetEtagMatchState(
            bool useStrongComparison,
            IList<EntityTagHeaderValue> etagHeader,
            EntityTagHeaderValue etag,
            PreconditionState matchFoundState,
            PreconditionState matchNotFoundState)
        {
            if (etagHeader != null && etagHeader.Any())
            {
                var state = matchNotFoundState;
                foreach (var entityTag in etagHeader)
                {
                    if (entityTag.Equals(EntityTagHeaderValue.Any) || entityTag.Compare(etag, useStrongComparison))
                    {
                        state = matchFoundState;
                        break;
                    }
                }

                return state;
            }

            return PreconditionState.Unspecified;
        }

        private static PreconditionState GetMaxPreconditionState(params PreconditionState[] states)
        {
            var max = PreconditionState.Unspecified;
            for (var i = 0; i < states.Length; i++)
            {
                if (states[i] > max)
                {
                    max = states[i];
                }
            }

            return max;
        }

        private (RangeItemHeaderValue range, long rangeLength, bool serveBody) SetRangeHeaders(
            ActionContext context,
            RequestHeaders httpRequestHeaders,
            long fileLength)
        {
            var response = context.HttpContext.Response;
            var httpResponseHeaders = response.GetTypedHeaders();
            var serveBody = !HttpMethods.IsHead(context.HttpContext.Request.Method);

            var (isRangeRequest, range) = RangeHelper.ParseRange(
                context.HttpContext,
                httpRequestHeaders,
                fileLength);

            if (!isRangeRequest)
            {
                return (range: null, rangeLength: 0, serveBody);
            }
            
            if (range == null)
            {
                response.StatusCode = StatusCodes.Status416RangeNotSatisfiable;
                httpResponseHeaders.ContentRange = new ContentRangeHeaderValue(fileLength);

                return (range: null, rangeLength: 0, serveBody: false);
            }

            response.StatusCode = StatusCodes.Status206PartialContent;
            httpResponseHeaders.ContentRange = new ContentRangeHeaderValue(
                range.From.Value,
                range.To.Value,
                fileLength);
            
            var rangeLength = SetContentLength(response, range);

            return (range, rangeLength, serveBody);
        }

        private static long SetContentLength(HttpResponse response, RangeItemHeaderValue range)
        {
            var start = range.From.Value;
            var end = range.To.Value;
            var length = end - start + 1;
            response.ContentLength = length;
            return length;
        }

        protected static ILogger CreateLogger<T>(ILoggerFactory factory)
        {
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            return factory.CreateLogger<T>();
        }

        protected static async Task WriteFileAsync(HttpContext context, Stream fileStream, RangeItemHeaderValue range, long rangeLength)
        {
            var outputStream = context.Response.Body;
            using (fileStream)
            {
                try
                {
                    if (range == null)
                    {
                        await StreamCopyOperation.CopyToAsync(fileStream, outputStream, count: null, bufferSize: BufferSize, cancel: context.RequestAborted);
                    }
                    else
                    {
                        fileStream.Seek(range.From.Value, SeekOrigin.Begin);
                        await StreamCopyOperation.CopyToAsync(fileStream, outputStream, rangeLength, BufferSize, context.RequestAborted);
                    }
                }
                catch (OperationCanceledException)
                {
                    context.Abort();
                }
            }
        }

        private static DateTimeOffset RoundDownToWholeSeconds(DateTimeOffset dateTimeOffset)
        {
            var ticksToRemove = dateTimeOffset.Ticks % TimeSpan.TicksPerSecond;
            return dateTimeOffset.Subtract(TimeSpan.FromTicks(ticksToRemove));
        }
    }
}
