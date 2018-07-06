using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.IO;

namespace Rainbow.Web.Configuration
{
    internal class ExtensionCollection : IBootstrapper
    {
        public string Root { get; private set; }

        //public RunStates EnvironmentState { get; private set; }

        private List<ExtensionBase> Extensions { get; set; }

        public ExtensionCollection(string root)
        {
            this.Root = root;
            this.Extensions = new List<ExtensionBase>();
        }

        public void Add(ExtensionBase ext)
        {
            this.Extensions.Add(ext);
        }

        public void ConfigureServices(IServiceCollection services)
        {
            if(this.Extensions != null && this.Extensions.Count > 0)
            {
                this.Extensions.ForEach((p) => p.ConfigureServices(services));
            }
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (this.Extensions != null && this.Extensions.Count > 0)
            {
                this.Extensions.ForEach((p) => p.Configure(app, env));
            }
        }
    }
}
