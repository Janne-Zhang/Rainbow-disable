using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Rainbow.Kismet.Infrastructure
{
    public class ContentResult : ActionResult 
    {
        public string Content { get; set; }

        public string ContentType { get; set; }

        public int? StatusCode { get; set; }

        public override Task ExecuteResultAsync(ActionContext context)
        {
            if(context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var executor = context.HttpContext.RequestServices.GetRequiredService<IActionResultExecutor<ContentResult>>();
            return executor.ExecuteAsync(context, this);
        }
    }
}
