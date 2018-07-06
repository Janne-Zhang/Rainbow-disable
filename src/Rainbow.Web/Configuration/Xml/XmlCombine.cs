using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Schema;

namespace Rainbow.Web.Configuration.Xml
{
    /// <summary>
    /// 根据XML架构文档XmlSchema合并xml文件
    /// </summary>
    internal class XmlCombine
    {
        private static readonly string XMLNS_NAME = "xmlns";
        public XmlNamespaces Namespaces { get; set; }
        private C0323E7167F4 Document = null;

        public XmlCombine()
        {
            this.Namespaces = new XmlNamespaces();
        }

        public void Combine(string uri)
        {
            this.Parse(XmlReader.Create(uri));
        }

        public void Combine(Stream input)
        {
            this.Parse(XmlReader.Create(input));
        }

        private void Parse(XmlReader current)
        {
            using (F4F9AF6F3B04 parse = new F4F9AF6F3B04(this, current))
            {
                parse.Parse();
            }
        }

        /// <summary>
        /// 输出格式化的xml
        /// </summary>
        /// <returns>格式化的xml文档</returns>
        public override string ToString()
        {
            return ToFormatString("\t", System.Environment.NewLine);
        }

        /// <summary>
        /// 输出格式化的xml
        /// </summary>
        /// <param name="identChar">缩进符</param>
        /// <param name="newlineChar">换行符</param>
        /// <returns>格式化的xml文档</returns>
        public string ToFormatString(string identChar, string newlineChar)
        {
            XmlWriterSettings setting = new XmlWriterSettings();
            setting.Indent = true;
            setting.Encoding = new UTF8Encoding(false);
            setting.IndentChars = identChar;
            setting.NewLineChars = newlineChar;

            return ToFormatString(setting);
        }

        /// <summary>
        /// 输出格式化的xml
        /// </summary>
        /// <param name="setting">输出xml细节</param>
        /// <returns>格式化的xml文档</returns>
        public string ToFormatString(XmlWriterSettings setting)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (XmlWriter xml = XmlWriter.Create(stream, setting))
                {
                    xml.WriteStartDocument(false);
                    ToFormatString(xml, this.Document);
                }

                return Encoding.UTF8.GetString(stream.ToArray());
            }
        }

        /// <summary>
        /// 输出合并后的XML并返回XmlReader对象
        /// </summary>
        public XmlReader ToXmlReader()
        {
            XmlReader reader = null;
            string xml = ToString();

            if (!string.IsNullOrWhiteSpace(xml))
            {
                XmlSchemaSet schemaSet = new XmlSchemaSet();
                foreach(XmlNamespaces.B5F1C47E3281 ns in this.Namespaces)
                {
                    if (ns.Schema != null)
                    {
                        schemaSet.Add(ns.Schema);
                    }
                }
                schemaSet.Compile();

                XmlReaderSettings settings = new XmlReaderSettings { ValidationType = ValidationType.Schema, Schemas = schemaSet };
                settings.ValidationEventHandler += (sender, e) =>
                {
                    throw new RainbowException($"An exception occurred parsing xml :{e.Message}", e.Exception);
                };
                settings.IgnoreComments = true;

                StringReader input = new StringReader(xml);
                reader = XmlReader.Create(input, settings);
            }

            return reader;
        }

        private void ToFormatString(XmlWriter xml, C0323E7167F4 node)
        {
            if (node != null)
            {
                xml.WriteStartElement(node.Prefix, node.Name, this.Namespaces.GetUriByPrefix(node.Prefix));

                if (node.Attributes.Count > 0)
                {
                    foreach (DCA6AE8014E9 attr in node.Attributes)
                    {
                        if (string.Equals(attr.Prefix, node.Prefix, StringComparison.OrdinalIgnoreCase)
                            || string.IsNullOrWhiteSpace(node.Prefix))
                        {
                            xml.WriteAttributeString(attr.Name, attr.Value);
                        }
                        else
                        {
                            xml.WriteAttributeString(attr.Prefix, attr.Name, this.Namespaces.GetUriByPrefix(attr.Prefix), attr.Value);
                        }
                    }
                }

                if (!string.IsNullOrWhiteSpace(node.Value))
                {
                    xml.WriteString(node.Value);
                }

                if (node.Childs.Count > 0)
                {
                    foreach (C0323E7167F4 current in node.Childs)
                    {
                        ToFormatString(xml, current);
                    }
                }

                xml.WriteEndElement();
            }
        }

        /// <summary>
        /// XML Node
        /// </summary>
        public class C0323E7167F4
        {
            public string Prefix { get; set; }
            public string Name { get; set; }
            public string Value { get; set; }
            public IList<DCA6AE8014E9> Attributes { get; set; }
            public IList<C0323E7167F4> Childs { get; set; }
            public C0323E7167F4 Parent { get; set; }

            public C0323E7167F4()
            {
                this.Attributes = new List<DCA6AE8014E9>();
                this.Childs = new List<C0323E7167F4>();
            }
        }

        /// <summary>
        /// XML Attribute
        /// </summary>
        public class DCA6AE8014E9
        {
            public string Prefix { get; set; }
            public string Name { get; set; }
            public string Value { get; set; }
        }

        /// <summary>
        /// XML Parser
        /// </summary>
        public class F4F9AF6F3B04 : IDisposable
        {
            private XmlNamespaces Namespaces;
            private XmlReader Current;
            private XmlCombine Combine;

            public F4F9AF6F3B04(XmlCombine combine, XmlReader reader)
            {
                this.Namespaces = new XmlNamespaces();
                this.Combine = combine;
                this.Current = reader;
            }

            public void Parse()
            {
                C0323E7167F4 root = null;

                while (this.Current.Read())
                {
                    switch (this.Current.NodeType)
                    {
                        case XmlNodeType.Element:
                            bool isEmpty = this.Current.IsEmptyElement;
                            root = this.Current.Depth == 0 ? ParseDocument(this.Current) : ParseElement(this.Current, root);
                            if (isEmpty) root = root.Parent;
                            break;

                        case XmlNodeType.EndElement:
                            root = root.Parent;
                            break;
                    }
                }
            }

            private C0323E7167F4 ParseDocument(XmlReader current)
            {
                C0323E7167F4 node = new C0323E7167F4()
                {
                    Name = current.LocalName,
                    Value = current.Value,
                    Parent = null,
                    Prefix = current.Prefix,
                };

                ParseAttributes(current, node);
                ParseNamespace(node);

                if (this.Combine.Document != null)
                {
                    if (!(string.Equals(this.Combine.Document.Prefix, node.Prefix, StringComparison.CurrentCultureIgnoreCase)
                        && string.Equals(this.Combine.Document.Name, node.Name, StringComparison.CurrentCultureIgnoreCase)))
                    {
                        throw new RainbowException($"The XML root node is not consistent and must be '{this.Combine.Document.Prefix}:{this.Combine.Document.Name}'");
                    }

                    foreach (DCA6AE8014E9 attr in node.Attributes)
                    {
                        this.AddAttribute(this.Combine.Document, attr);
                    }
                }
                else
                {
                    this.Combine.Document = node;
                }

                return this.Combine.Document;
            }

            private C0323E7167F4 ParseElement(XmlReader current, C0323E7167F4 parent)
            {
                C0323E7167F4 node = new C0323E7167F4()
                {
                    Name = current.LocalName,
                    Value = current.Value,
                    Parent = parent,
                    Prefix = current.Prefix,
                };

                ParseAttributes(current, node);
                ParseNamespace(node);

                return AddElement(parent, node);
            }

            private void ParseAttributes(XmlReader current, C0323E7167F4 node)
            {
                if (current.HasAttributes)
                {
                    List<DCA6AE8014E9> attrs = new List<DCA6AE8014E9>();

                    while (current.MoveToNextAttribute())
                    {
                        if (string.Equals(XMLNS_NAME, current.Prefix, StringComparison.OrdinalIgnoreCase))
                        {
                            this.AddNamespace(current.LocalName, current.Value);
                        }
                        else if (string.Equals(XMLNS_NAME, current.LocalName, StringComparison.OrdinalIgnoreCase))
                        {
                            this.AddNamespace("", current.Value);
                        }
                        else
                        {
                            DCA6AE8014E9 attr = new DCA6AE8014E9()
                            {
                                Name = current.LocalName,
                                Value = current.Value,
                                Prefix = current.Prefix
                            };
                            attrs.Add(attr);
                        }
                    }

                    foreach (DCA6AE8014E9 attr in attrs)
                    {
                        attr.Prefix = string.IsNullOrWhiteSpace(attr.Prefix) ?
                            "" : this.Namespaces.GetMappingByPrefix(attr.Prefix);
                        this.AddAttribute(node, attr);
                    }
                }
            }

            private void ParseNamespace(C0323E7167F4 node)
            {
                node.Prefix = this.Namespaces.GetMappingByPrefix(node.Prefix);
                foreach (DCA6AE8014E9 attr in node.Attributes)
                {
                    if (string.IsNullOrWhiteSpace(attr.Prefix))
                    {
                        attr.Prefix = node.Prefix;
                    }
                }
            }

            private C0323E7167F4 AddElement(C0323E7167F4 parent, C0323E7167F4 node)
            {
                XmlSchemaElement element = this.Combine.Namespaces.TryGetElementByPrefix(node.Prefix, node.Name);
                if(element != null && element.MaxOccurs == 1)
                {
                    C0323E7167F4 ins = parent.Childs.FirstOrDefault(p => 
                        string.Equals(p.Name, node.Name, StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(p.Prefix, node.Prefix, StringComparison.OrdinalIgnoreCase));

                    if(ins != null)
                    {
                        ins.Value = node.Value;

                        foreach(DCA6AE8014E9 attr in node.Attributes)
                        {
                            AddAttribute(ins, attr);
                        }
                        return ins;
                    }
                }

                parent.Childs.Add(node);
                return node;
            }

            private DCA6AE8014E9 AddAttribute(C0323E7167F4 node, DCA6AE8014E9 attr)
            {
                DCA6AE8014E9 current = node.Attributes.FirstOrDefault(p => string.Equals(p.Prefix, attr.Prefix, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(p.Name, attr.Name, StringComparison.OrdinalIgnoreCase));

                if (current != null)
                {
                    current.Value = attr.Value;
                    return current;
                }
                else
                {
                    node.Attributes.Add(attr);
                    return attr;
                }
            }

            private void AddNamespace(string prefix, string uri)
            {
                if (!this.Namespaces.Exists(prefix))
                {
                    this.Namespaces.Add(prefix, uri, this.Combine.Namespaces.GetPrefixByUri(uri));
                }
            }

            public void Dispose()
            {
                this.Current.Dispose();
            }
        }
    }
}