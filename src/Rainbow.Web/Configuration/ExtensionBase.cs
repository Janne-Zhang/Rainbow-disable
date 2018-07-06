using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Rainbow.Web.Reflection;
using System;
using System.Collections.Generic;

namespace Rainbow.Web.Configuration
{
    internal class ExtensionBase : IBootstrapper, IDisposable
    {
        public string Name { get; protected set; }

        public IList<ReflectObject> Startup { get; protected set; }

        public RunStates State { get; protected set; }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            foreach(ReflectObject itm in this.Startup)
            {
                itm.TryInvoke("Configure", app, env);
            }
        }

        public void ConfigureServices(IServiceCollection services)
        {
            foreach (ReflectObject itm in this.Startup)
            {
                itm.TryInvoke("ConfigureServices", services);
            }
        }

        public void Dispose()
        {
            foreach (ReflectObject itm in this.Startup)
            {
                itm.Dispose();
            }
        }
    }
}
