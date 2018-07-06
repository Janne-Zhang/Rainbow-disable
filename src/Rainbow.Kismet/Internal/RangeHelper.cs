using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using System;
using System.Linq;

namespace Rainbow.Kismet.Internal
{
    internal static class RangeHelper
    {
        public static (bool isRangeRequest, RangeItemHeaderValue range) ParseRange(
            HttpContext context,
            RequestHeaders requestHeaders,
            long length)
        {
            var rawRangeHeader = context.Request.Headers[HeaderNames.Range];
            if (StringValues.IsNullOrEmpty(rawRangeHeader))
            {
                return (false, null);
            }
            
            if (rawRangeHeader.Count > 1 || rawRangeHeader[0].IndexOf(',') >= 0)
            {
                return (false, null);
            }

            var rangeHeader = requestHeaders.Range;
            if (rangeHeader == null)
            {
                return (false, null);
            }


            var ranges = rangeHeader.Ranges;
            if (ranges == null)
            {
                return (false, null);
            }

            if (ranges.Count == 0)
            {
                return (true, null);
            }

            if (length == 0)
            {
                return (true, null);
            }
            
            var range = NormalizeRange(ranges.SingleOrDefault(), length);
            
            return (true, range);
        }
        
        internal static RangeItemHeaderValue NormalizeRange(RangeItemHeaderValue range, long length)
        {
            var start = range.From;
            var end = range.To;
            
            if (start.HasValue)
            {
                if (start.Value >= length)
                {
                    return null;
                }
                if (!end.HasValue || end.Value >= length)
                {
                    end = length - 1;
                }
            }
            else
            {
                if (end.Value == 0)
                {
                    return null;
                }

                var bytes = Math.Min(end.Value, length);
                start = length - bytes;
                end = start + bytes - 1;
            }

            return new RangeItemHeaderValue(start, end);
        }
    }
}
