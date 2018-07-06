using Microsoft.AspNetCore.Http;
using System;

namespace Rainbow.Kismet
{
    public class ActionContext
    {
        public HttpContext HttpContext
        {
            get;
            set;
        }

        public ActionContext()
        { }

        public ActionContext(HttpContext httpContext)
        {
            this.HttpContext = httpContext ?? throw new ArgumentNullException(nameof(httpContext));
        }
    }
}
