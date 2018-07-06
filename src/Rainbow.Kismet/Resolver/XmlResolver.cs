using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Rainbow.Kismet.Resolver
{
    public class XmlResolver : DynamicObject, IEnumerable
    {
        private XToken Token = null;

        private XmlResolver()
        { }

        public XmlResolver(string xml)
        {
            if (string.IsNullOrWhiteSpace(xml))
            {
                throw new ArgumentNullException(nameof(xml));
            }

            this.Token = XToken.Parse(xml);
        }

        public XmlResolver(XmlNode node)
        {
            if (node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }

            this.Token = new XToken(node);
        }

        public XmlResolver(XmlNodeList list)
        {
            if (list == null)
            {
                throw new ArgumentNullException(nameof(list));
            }

            this.Token = new XToken(list);
        }

        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            return base.TryGetIndex(binder, indexes, out result);
        }

        public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value)
        {
            return base.TrySetIndex(binder, indexes, value);
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            XmlNode current = null;
            XmlNodeList nodes = null;
            if(this.Token.Type == XTokenType.Array)
            {
                current = (this.Token.Value as XmlNodeList)[0];
            }
            else
            {
                current = (this.Token.Value as XmlNode);
            }
            nodes = current.ChildNodes;

            foreach(XmlNode item in nodes)
            {
                if(string.Equals(item.LocalName, binder.Name, StringComparison.CurrentCultureIgnoreCase))
                {
                    result = this.GetMember(binder.Name, item);
                    return true;
                }
            }

            result = null;
            return false;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            return base.TrySetMember(binder, value);
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            return base.TryInvokeMember(binder, args, out result);
        }

        private object GetMember(string name, XmlNode node)
        {
            if(node.HasChildNodes)
            {
                return new XmlResolver(node);
            }

            return node.Value;
        }

        public override string ToString()
        {
            return base.ToString();
        }

        public IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public enum XTokenType
        {
            Object = 1,
            Array = 2,
        }

        private class XToken
        {
            public XTokenType Type { get; private set; }

            public object Value { get; private set; }

            public XToken(XmlNode node)
            {
                this.Value = node;
                this.Type = XTokenType.Object;
            }

            public XToken(XmlNodeList list)
            {
                this.Value = list;
                this.Type = XTokenType.Array;
            }

            public static XToken Parse(string xml)
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xml);

                return new XToken(doc.DocumentElement as XmlNode);
            }
        }
    }
}
