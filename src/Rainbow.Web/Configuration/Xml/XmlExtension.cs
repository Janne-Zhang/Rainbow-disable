using Rainbow.Web.Reflection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.XPath;

namespace Rainbow.Web.Configuration.Xml
{
    internal class XmlExtension : ExtensionBase
    {
        private static readonly string library_suffix = "*.dll";
        private static readonly string library_startup = "Bootstrapper";

        public string Root { get; private set; }

        public IList<Assembly> Libraries { get; private set; }

        public XmlExtension(string root, XPathNavigator navi)
            : base()
        {
            this.Startup = new List<ReflectObject>();
            this.Libraries = new List<Assembly>();

            Parser(root, navi);
            Finder();
            Search();
        }

        private void Parser(string root, XPathNavigator navi)
        {
            if(navi == null)
            {
                throw new ArgumentNullException(nameof(navi));
            }

            string name = navi.GetAttribute("name", "");
            string state = navi.GetAttribute("state", "");

            if(string.IsNullOrWhiteSpace(name))
            {
                throw new RainbowException("Configuration file 'extend/name' attribute node is not set.");
            }

            this.Name = name;
            this.State = string.IsNullOrWhiteSpace(state) ? RunStates.Enable : Enum.Parse<RunStates>(state, true);

            this.Root = Path.Combine(root, name);
        }

        private void Finder()
        {
            DirectoryInfo dir = new DirectoryInfo(this.Root);
            if(dir.Exists)
            {
                Assembly[] assembies = AppDomain.CurrentDomain.GetAssemblies();

                foreach (FileInfo lib in dir.GetFiles(library_suffix, SearchOption.TopDirectoryOnly))
                {
                    Assembly assembly = assembies.FirstOrDefault(p => string.Equals(p.ManifestModule.Name, lib.Name, StringComparison.OrdinalIgnoreCase));
                    if (assembly == null)
                    {
                        assembly = Assembly.LoadFrom(lib.FullName);
                    }
                    this.Libraries.Add(assembly);
                }
            }
        }

        private void Search()
        {
            foreach(Assembly assembly in this.Libraries)
            {
                string name = this.Libraries[0].FullName;
                name = name.Substring(0, name.IndexOf(','));
                name = string.Concat(name, ".", library_startup);

                Type start = assembly.GetType(name, false, true);
                if(start != null)
                {
                    ReflectObject reflect = new ReflectObject(start);
                    reflect.Constructor();

                    this.Startup.Add(reflect);
                }
            }
        }
    }
}
