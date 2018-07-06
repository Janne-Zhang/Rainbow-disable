using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.XPath;

namespace Rainbow.Web.Configuration.Xml
{
    internal class XmlConfiguration : ConfigurationBase
    {
        public const string configuration_root = "Rainbow";
        public const string configuration_xmlns = "http://schemas.inetapi.cn/rainbow/XmlSchema";
        public const string configuration_prefix = "rb";
        public const string configuration_suffix = "*.config";

        public static readonly XPathExpression expression_extensions;
        public static readonly XPathExpression expression_extension;

        public string Root { get; private set; }

        static XmlConfiguration()
        {
            NameTable nt = new NameTable();
            XmlNamespaceManager nm = new XmlNamespaceManager(nt);
            nm.AddNamespace(configuration_prefix, configuration_xmlns);

            expression_extensions = XPathExpression.Compile($"//{configuration_prefix}:extensions", nm);
            expression_extension = XPathExpression.Compile($"{configuration_prefix}:extend", nm);
        }

        public XmlConfiguration(string conf)
            : base()
        {
            if (conf == null)
            {
                throw new ArgumentNullException(nameof(conf));
            }

            FileInfo info = new FileInfo(conf);
            this.Root = info.DirectoryName;

            List<string> files = this.GetConfigurationFiles(info);

            if (files == null || files.Count == 0)
            {
                throw new Exception($"Not found configuration files：{conf}");
            }

            XmlCombine combine = new XmlCombine();
            combine.Namespaces.Add(configuration_prefix, configuration_xmlns);

            foreach (string file in files)
            {
                combine.Combine(file);
            }

            using (XmlReader xmlReader = combine.ToXmlReader())
            {
                Configurate(xmlReader);
            }
        }

        public XmlConfiguration(string root, XmlReader xmlReader)
            : base()
        {
            if (root == null)
            {
                throw new ArgumentNullException(nameof(root));
            }

            if (xmlReader == null)
            {
                throw new ArgumentNullException(nameof(xmlReader));
            }

            this.Root = root;
            Configurate(xmlReader);
        }

        private List<string> GetConfigurationFiles(FileInfo file)
        {
            List<string> paths = new List<string>();

            if (file.Exists)
            {
                paths.Add(file.FullName);

                using (XmlReader xmlReader = XmlReader.Create(file.FullName, XmlSchemas.CreateConfigurationReaderSettings()))
                {
                    if(xmlReader.ReadToFollowing("Rainbow"))
                    {
                        string search = xmlReader.GetAttribute("search", "");
                        if (!string.IsNullOrWhiteSpace(search))
                        {
                            string path = Path.Combine(this.Root, search);
                            paths.AddRange(Directory.GetFiles(path, configuration_suffix));
                        }
                    }
                }
            }

            return paths;
        }
        
        private void Configurate(XmlReader xmlReader)
        {
            XPathNavigator nav = new XPathDocument(xmlReader).CreateNavigator();
            Parser(nav);
        }
        
        private void Parser(XPathNavigator navigator)
        {
            ParseExtensions(navigator);
        }

        private void ParseExtensions(XPathNavigator navigator)
        {
            XPathNavigator navi = navigator.SelectSingleNode(expression_extensions);
            if (navi == null)
            {
                return;
            }

            string search = navi.GetAttribute("root", "");
            if (!string.IsNullOrWhiteSpace(search))
            {
                string path = Path.Combine(this.Root, search);
                this.Extensions = new ExtensionCollection(path);
            }

            if (this.Extensions != null)
            {
                XPathNodeIterator xpni = navi.Select(expression_extension);

                while (xpni.MoveNext())
                {
                    this.Extensions.Add(new XmlExtension(this.Extensions.Root, xpni.Current));
                }
            }
        }
        
    }
}
