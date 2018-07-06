using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Rainbow.Kismet;
using Rainbow.Kismet.Infrastructure;
using Rainbow.Web.Configuration;

namespace Rainbow.Web
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddKismet();
            Bootstrapper.Current.ConfigureServices(services);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            Bootstrapper.Current.Configure(app, env);

            //if (env.IsDevelopment())
            //{
            //    app.UseDeveloperExceptionPage();
            //}
            app.Run(async (context) =>
            {
                //ActionContext ctx = new ActionContext();
                //ctx.HttpContext = context;

                //System.IO.FileStream stream = new System.IO.FileStream("/app/bin/Debug/netcoreapp2.0/test.py", System.IO.FileMode.Open);
                //byte[] buffer = new byte[stream.Length];
                //stream.Read(buffer, 0, buffer.Length);

                //FileResult rst = new FileResult(buffer, "text/plain")
                //{
                //    FileDownloadName = "test.py"
                //};

                //ActionContext ctx = new ActionContext();
                //ctx.HttpContext = context;

                //System.IO.FileStream stream = new System.IO.FileStream("/app/bin/Debug/netcoreapp2.0/test.py", System.IO.FileMode.Open);

                //FileResult rst = new FileResult(stream, "text/plain")
                //{
                //    FileDownloadName = "test.py"
                //};

                //ActionContext ctx = new ActionContext();
                //ctx.HttpContext = context;
                //FileResult rst = new FileResult("/app/bin/Debug/netcoreapp2.0/test.py");

                if (context.Request.Path.Value== "/redirect")
                {
                    ActionContext ctx = new ActionContext();
                    ctx.HttpContext = context;
                    RedirectResult rst = new RedirectResult("redirect2");
                    await rst.ExecuteResultAsync(ctx);
                }

                await context.Response.WriteAsync(context.Request.Path.Value);
            });
        }
    }
}
