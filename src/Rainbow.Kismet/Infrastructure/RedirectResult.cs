using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Rainbow.Kismet.Infrastructure
{
    /// <summary>
    /// 返回查找(302)、永久移动(301)、临时重定向(307)或永久重定向(308)的响应，其中位置头指向所提供的URL。
    /// </summary>
    public class RedirectResult : ActionResult
    {
        public string Url { get; private set; }

        /// <summary>
        /// 获取或设置一个值，该值指定重定向应该是永久的(如果为true)或临时的(如果为false)。
        /// </summary>
        public bool Permanent { get; set; }

        /// <summary>
        /// 获取或设置重定向是否保留初始请求方法。
        /// </summary>
        public bool PreserveMethod { get; set; }

        public RedirectResult(string url)
            : this(url, permanent: false)
        { }

        public RedirectResult(string url, bool permanent)
            : this(url, permanent, preserveMethod: false)
        { }

        public RedirectResult(string url, bool permanent, bool preserveMethod)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentNullException(nameof(url));
            }

            this.Permanent = permanent;
            this.PreserveMethod = preserveMethod;
            this.Url = url;
        }

        public override Task ExecuteResultAsync(ActionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var executor = context.HttpContext.RequestServices.GetRequiredService<IActionResultExecutor<RedirectResult>>();
            return executor.ExecuteAsync(context, this);
        }
    }
}
