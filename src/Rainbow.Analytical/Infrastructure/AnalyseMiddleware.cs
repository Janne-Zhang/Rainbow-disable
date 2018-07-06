using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace Rainbow.Analytical.Infrastructure
{
    internal class AnalyseMiddleware
    {
        private readonly AnalyseOptions _options;
        private readonly RequestDelegate _next;
        private readonly IHostingEnvironment _environment;

        public AnalyseMiddleware(RequestDelegate next, IHostingEnvironment hostingEnv, IOptions<AnalyseOptions> options)
        {
            if (next == null)
            {
                throw new ArgumentNullException("next");
            }
            if (hostingEnv == null)
            {
                throw new ArgumentNullException("hostingEnv");
            }
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }
            this._next = next;
            this._options = options.Value;
            this._environment = hostingEnv;
        }
        
        public Task Invoke(HttpContext context)
        {
            if (context.Request.Query["ana"] == "1")
            {
                return context.Response.WriteAsync("analyse");
            }
            else
            {
                return this._next.Invoke(context);
            }
        }
    }
}
