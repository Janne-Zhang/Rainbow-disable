using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Rainbow.Web.Reflection;

namespace Rainbow.Web.Configuration
{
    internal interface IBootstrapper
    {
        void ConfigureServices(IServiceCollection services);

        void Configure(IApplicationBuilder app, IHostingEnvironment env);
    }
}
