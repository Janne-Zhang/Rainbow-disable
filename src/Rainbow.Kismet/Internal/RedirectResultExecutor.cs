using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using Rainbow.Kismet.Infrastructure;
using System;
using System.Threading.Tasks;

namespace Rainbow.Kismet.Internal
{
    internal class RedirectResultExecutor : IActionResultExecutor<RedirectResult>
    {

        public Task ExecuteAsync(ActionContext context, RedirectResult result)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            string destinationUrl = result.Url;
            if (UrlHelper.IsLocalUrl(destinationUrl))
            {
                destinationUrl = UrlHelper.Content(context.HttpContext, destinationUrl);
            }

            if (result.PreserveMethod)
            {
                context.HttpContext.Response.StatusCode = result.Permanent ?
                    StatusCodes.Status308PermanentRedirect : StatusCodes.Status307TemporaryRedirect;
                context.HttpContext.Response.Headers[HeaderNames.Location] = destinationUrl;
            }
            else
            {
                context.HttpContext.Response.Redirect(destinationUrl, result.Permanent);
            }

            return Task.CompletedTask;
        }
    }
}
