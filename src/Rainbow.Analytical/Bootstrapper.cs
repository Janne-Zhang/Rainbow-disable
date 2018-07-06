using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Rainbow.Analytical
{
    internal class Bootstrapper
    {
        public void ConfigureServices(IServiceCollection services)
        {
            //
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseAnalytical();
        }
    }
}
