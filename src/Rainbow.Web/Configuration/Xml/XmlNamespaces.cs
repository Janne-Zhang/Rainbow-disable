using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Schema;

namespace Rainbow.Web.Configuration.Xml
{
    /// <summary>
    /// Xml Namespace
    /// </summary>
    internal class XmlNamespaces : IEnumerable<XmlNamespaces.B5F1C47E3281>
    {
        private IList<B5F1C47E3281> Items;

        /// <summary>
        /// 是否包含默认命名空间
        /// </summary>
        public bool HasDefaultNamespace { get; private set; }

        private const string _default_namespace_prefix = "";

        public XmlNamespaces()
        {
            this.Items = new List<B5F1C47E3281>();
        }

        /// <summary>
        /// 增加命名空间
        /// </summary>
        /// <param name="prefix">前缀，如果是默认命名空间，则使用空字符串</param>
        /// <param name="uri">命名空间uri</param>
        public void Add(string prefix, string uri)
        {
            Add(prefix, uri, "", null);
        }

        /// <summary>
        /// 增加命名空间
        /// </summary>
        /// <param name="prefix">前缀，如果是默认命名空间，则使用空字符串</param>
        /// <param name="uri">命名空间uri</param>
        /// <param name="mapping">映射至新的命名空间前缀</param>
        public void Add(string prefix, string uri, string mapping)
        {
            Add(prefix, uri, string.IsNullOrEmpty(mapping) ? "" : mapping, null);
        }

        /// <summary>
        /// 增加命名空间
        /// </summary>
        /// <param name="prefix">前缀，如果是默认命名空间，则使用空字符串</param>
        /// <param name="uri">命名空间uri</param>
        /// <param name="schema">XSD架构缓存对象</param>
        public void Add(string prefix, string uri, XmlSchema schema)
        {
            Add(prefix, uri, "", schema);
        }

        /// <summary>
        /// 增加命名空间
        /// </summary>
        /// <param name="prefix">前缀，如果是默认命名空间，则使用空字符串</param>
        /// <param name="uri">命名空间uri</param>
        /// <param name="mapping">映射至新的命名空间前缀</param>
        /// <param name="schema">XSD架构缓存对象</param>
        public void Add(string prefix, string uri, string mapping, XmlSchema schema)
        {
            if (string.IsNullOrEmpty(prefix))
                prefix = _default_namespace_prefix;

            if(this.Items.Any(p => string.Equals(p.Prefix, prefix, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException($"The namespace named '{prefix}' already exists.");
            }

            if(string.IsNullOrWhiteSpace(uri))
            {
                throw new InvalidOperationException("The URI of the namespace cannot be empty.");
            }

            if (this.Items.Any(p => string.Equals(p.Uri, uri, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException($"The namespace URI '{uri}' already exists.");
            }

            B5F1C47E3281 item = new B5F1C47E3281(prefix, uri, mapping, schema);
            this.Items.Add(item);
            
            if (prefix == _default_namespace_prefix)
            {
                this.HasDefaultNamespace = true;
            }
        }

        /// <summary>
        /// 移除命名空间
        /// </summary>
        /// <param name="prefix">前缀，如果是默认命名空间，则使用空字符串</param>
        public void Remove(string prefix)
        {
            B5F1C47E3281 item = this.Items.FirstOrDefault(p => string.Equals(p.Prefix, prefix, StringComparison.OrdinalIgnoreCase));
            if (item != null)
            {
                this.Items.Remove(item);

                if (item.Prefix == _default_namespace_prefix)
                {
                    this.HasDefaultNamespace = false;
                }
            }
        }

        /// <summary>
        /// 判断命名空间是否存在
        /// </summary>
        /// <param name="prefix">前缀，如果是默认命名空间，则使用空字符串</param>
        /// <returns>是否存在命名空间</returns>
        public bool Exists(string prefix)
        {
            return this.Items.Any(p => string.Equals(p.Prefix, prefix, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// 根据命名空间前缀获取命名空间Uri
        /// </summary>
        /// <param name="prefix">命名空间前缀</param>
        /// <returns>命名空间Uri</returns>
        public string GetUriByPrefix(string prefix)
        {
            B5F1C47E3281 ns = this.Items.FirstOrDefault(p => string.Equals(p.Prefix, prefix, StringComparison.OrdinalIgnoreCase));

            if(ns == null)
            {
                throw new InvalidOperationException($"The namespace prefix '{prefix}' not exists.");
            }

            return ns.Uri;
        }

        /// <summary>
        /// 根据命名空间Uri获取命名空间前缀
        /// </summary>
        /// <param name="uri">命名空间Uri</param>
        /// <returns>命名空间前缀</returns>
        public string GetPrefixByUri(string uri)
        {
            B5F1C47E3281 ns = this.Items.FirstOrDefault(p => string.Equals(p.Uri, uri, StringComparison.OrdinalIgnoreCase));

            if (ns == null)
            {
                throw new InvalidOperationException($"The namespace URI '{uri}' not exists.");
            }

            return ns.Prefix;
        }

        /// <summary>
        /// 根据命名空间前缀获取命名空间映射
        /// </summary>
        /// <param name="prefix">命名空间前缀</param>
        /// <returns>命名空间映射</returns>
        public string GetMappingByPrefix(string prefix)
        {
            B5F1C47E3281 ns = this.Items.FirstOrDefault(p => string.Equals(p.Prefix, prefix, StringComparison.OrdinalIgnoreCase));

            if (ns == null)
            {
                throw new InvalidOperationException($"The namespace prefix '{prefix}' not exists.");
            }

            return ns.Mapping;
        }

        /// <summary>
        /// 根据命名空间前缀获取命名空间Uri，如果不存在则返回 null
        /// </summary>
        /// <param name="prefix">命名空间前缀</param>
        /// <returns>命名空间Uri</returns>
        public string TryGetUriByPrefix(string prefix)
        {
            B5F1C47E3281 ns = this.Items.FirstOrDefault(p => string.Equals(p.Prefix, prefix, StringComparison.OrdinalIgnoreCase));
            return ns == null ? null : ns.Uri;
        }

        /// <summary>
        /// 根据命名空间Uri获取命名空间前缀，如果不存在则返回 null
        /// </summary>
        /// <param name="uri">命名空间Uri</param>
        /// <returns>命名空间前缀</returns>
        public string TryGetPrefixByUri(string uri)
        {
            B5F1C47E3281 ns = this.Items.FirstOrDefault(p => string.Equals(p.Uri, uri, StringComparison.OrdinalIgnoreCase));
            return ns == null ? null : ns.Prefix;
        }

        /// <summary>
        /// 根据命名空间前缀获取命名空间映射，如果不存在则返回 null
        /// </summary>
        /// <param name="prefix">命名空间前缀</param>
        /// <returns>命名空间映射</returns>
        public string TryGetMappingByPrefix(string prefix)
        {
            B5F1C47E3281 ns = this.Items.FirstOrDefault(p => string.Equals(p.Prefix, prefix, StringComparison.OrdinalIgnoreCase));
            return ns == null ? null : ns.Mapping;
        }

        /// <summary>
        /// 根据命名空间前缀和节点名称获取节点类型，如果不存在则返回 null
        /// </summary>
        /// <param name="prefix">命名空间前缀</param>
        /// <param name="name">节点名称</param>
        /// <returns>节点类型</returns>
        public XmlSchemaElement TryGetElementByPrefix(string prefix, string name)
        {
            XmlSchemaElement element = null;
            B5F1C47E3281 ns = this.Items.FirstOrDefault(p => string.Equals(p.Prefix, prefix, StringComparison.OrdinalIgnoreCase));
            if(ns != null)
            {
                if(ns.Schema != null)
                {
                    if(ns.Elements.TryGetValue(name, out element))
                    {
                        return element;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// 清空命名空间
        /// </summary>
        public void Clear()
        {
            this.Items.Clear();
            this.HasDefaultNamespace = false;
        }

        public IEnumerator<B5F1C47E3281> GetEnumerator()
        {
            return this.Items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.Items.GetEnumerator();
        }


        /// <summary>
        /// XML Namespace
        /// </summary>
        public class B5F1C47E3281
        {
            public string Prefix { get; private set; }
            public string Uri { get; private set; }
            public string Mapping { get; private set; }
            public XmlSchema Schema { get; private set; }
            public IDictionary<string, XmlSchemaElement> Elements { get; private set; }

            public B5F1C47E3281(string prefix, string uri, string mapping, XmlSchema schema)
            {
                this.Prefix = prefix;
                this.Uri = uri;
                this.Mapping = mapping;
                this.Schema = schema;
                this.Elements = new Dictionary<string, XmlSchemaElement>();

                if (schema != null)
                {
                    ParseElement(schema.Items as IEnumerable);
                }
            }

            private void ParseElement(IEnumerable items)
            {
                if (items != null)
                {
                    foreach (object item in items)
                    {
                        XmlSchemaElement element = item as XmlSchemaElement;
                        XmlSchemaComplexType type = null;
                        if (element != null)
                        {
                            this.Elements[element.Name] = element;
                            type = element.SchemaType as XmlSchemaComplexType;
                        }
                        else
                        {
                            type = item as XmlSchemaComplexType;
                        }
                        if (type != null)
                        {
                            XmlSchemaSequence sequence = type.Particle as XmlSchemaSequence;
                            ParseElement(sequence.Items as IEnumerable);
                        }
                    }
                }
            }
        }
    }
}
