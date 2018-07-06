using Microsoft.Extensions.Options;
using Rainbow.Analytical.Infrastructure;
using System;

namespace Microsoft.AspNetCore.Builder
{
    public static class AnalyseExtensions
    {
		public static IApplicationBuilder UseAnalytical(this IApplicationBuilder app)
        {
            if (app == null)
            {
                throw new ArgumentNullException("app");
            }
            return UseMiddlewareExtensions.UseMiddleware<AnalyseMiddleware>(app, new object[]
            {
                Options.Create<AnalyseOptions>(AnalyseOptions.Default)
            });
        }

        public static IApplicationBuilder UseDefaultFiles(this IApplicationBuilder app, AnalyseOptions options)
        {
            if (app == null)
            {
                throw new ArgumentNullException("app");
            }
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }
            return UseMiddlewareExtensions.UseMiddleware<AnalyseMiddleware>(app, new object[]
            {
                Options.Create<AnalyseOptions>(options)
            });
        }
    }
}
