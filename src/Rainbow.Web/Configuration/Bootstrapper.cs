using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Rainbow.Web.Configuration.Xml;

namespace Rainbow.Web.Configuration
{
    internal class Bootstrapper
    {
        private static Bootstrapper _Current;
        private static ConfigurationBase _Configuration;

        public static Bootstrapper Current
        {
            get
            {
                if(_Current == null)
                {
                    _Current = new Bootstrapper();
                }
                return _Current;
            }
        }

        private Bootstrapper()
        {
            if (_Configuration == null)
            {
                _Configuration = new XmlConfiguration(Environment.configuration);
            }
        }

        public void ConfigureServices(IServiceCollection services)
        {
            if (_Configuration.Extensions != null)
            {
                _Configuration.Extensions.ConfigureServices(services);
            }
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (_Configuration.Extensions != null)
            {
                _Configuration.Extensions.Configure(app, env);
            }
        }
    }
}
