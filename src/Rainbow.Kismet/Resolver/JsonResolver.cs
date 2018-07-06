/*
using Newtonsoft.Json.Linq;
using Rainbow.Kismet.Exceptions;
using System;
using System.Collections;
using System.Dynamic;

namespace Rainbow.Kismet.Resolver
{
    public class JsonResolver : DynamicObject, IEnumerable
    {
        private JToken Token = null;

        private JsonResolver()
        { }

        public JsonResolver(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                throw new ArgumentNullException(nameof(json));
            }

            this.Token = JToken.Parse(json);
        }

        public JsonResolver(JObject jObject)
        {
            if (jObject == null)
            {
                throw new ArgumentNullException(nameof(jObject));
            }

            this.Token = jObject;
        }

        public JsonResolver(JArray jArray)
        {
            if (jArray == null)
            {
                throw new ArgumentNullException(nameof(jArray));
            }

            this.Token = jArray;
        }

        private int Count()
        {
            if (this.Token.Type == JTokenType.Array)
            {
                return (this.Token as JArray).Count;
            }
            else if (this.Token.Type == JTokenType.Object)
            {
                return 1;
            }
            return 0;
        }

        private bool Add(object[] args)
        {
            if(args.Length == 1)
            {
                return this.Add(args[0]);
            }
            else
            {
                return this.Add(args[0].ToString(), args[1]);
            }
        }

        private bool Add(string name, object value)
        {
            if (this.Token.Type != JTokenType.Object)
            {
                throw new JsonResolverException($"Type does not support this operation: name={name}, type={this.Token.Type}");
            }

            if(value is JsonResolver)
            {
                value = (value as JsonResolver).Token;
            }

            JToken token = this.Token[name];
            if (token != null)
            {
                throw new JsonResolverException($"Property already exists: name={name}");
            }
            try
            {
                (this.Token as JObject).Add(new JProperty(name, value));

                return true;
            }
            catch(Exception exp)
            {
                throw new JsonResolverException(exp.Message, exp);
            }
        }

        private bool Add(object value)
        {
            if(this.Token.Type != JTokenType.Array)
            {
                throw new JsonResolverException($"Type does not support this operation: type={this.Token.Type}");
            }

            if (value is JsonResolver)
            {
                value = (value as JsonResolver).Token;
            }

            try
            {
                (this.Token as JArray).Add(value);

                return true;
            }
            catch (Exception exp)
            {
                throw new JsonResolverException(exp.Message, exp);
            }
        }

        private bool Remove(object[] args)
        {
            if(args.Length == 0)
            {
                return this.Remove();
            }
            else
            {
                return this.Remove(args[0].ToString());
            }
        }

        private bool Remove()
        {
            if (this.Token.Type != JTokenType.Array && this.Token.Type != JTokenType.Object)
            {
                throw new JsonResolverException($"Type does not support this operation: type={this.Token.Type}");
            }

            try
            {
                this.Token.Remove();
                this.Token = null;

                return true;
            }
            catch(Exception exp)
            {
                throw new JsonResolverException(exp.Message, exp);
            }
        }

        private bool Remove(string name)
        {
            if(this.Token.Type != JTokenType.Object)
            {
                throw new JsonResolverException($"Type does not support this operation: type={this.Token.Type}");
            }

            try
            {
                if (this.Token[name] != null)
                {
                    return (this.Token as JObject).Remove(name);
                }
                return false;
            }
            catch(Exception exp)
            {
                throw new JsonResolverException(exp.Message, exp);
            }
        }

        private bool Clear()
        {
            if(this.Token.Type == JTokenType.Array)
            {
                try
                {
                    (this.Token as JArray).Clear();
                }
                catch (Exception exp)
                {
                    throw new JsonResolverException(exp.Message, exp);
                }
            }
            return true;
        }

        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            string name = string.Empty;
            JToken token = this.Token;

            this.GetTokenByIndexs(indexes, ref name, ref token);
            result = GetMember(name, token);

            return true;
        }

        public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value)
        {
            string name = string.Empty;
            JToken token = this.Token;

            this.GetTokenByIndexs(indexes, ref name, ref token);
            this.SetMember(name, value, token);

            return true;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (this.Token.Type != JTokenType.Object)
            {
                throw new JsonResolverException($"Type mismatch: name={binder.Name}, type={this.Token.Type}");
            }

            JToken token = this.Token[binder.Name];
            if (token == null)
            {
                throw new JsonResolverException($"property does not exist: name={binder.Name}");
            }
            result = GetMember(binder.Name, token);
            
            return true;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            result = this.InvokeMember(binder.Name, args);
            return true;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            if (this.Token.Type != JTokenType.Object)
            {
                throw new JsonResolverException($"Type mismatch: name={binder.Name}, type={this.Token.Type}");
            }

            JToken token = this.Token[binder.Name];
            this.SetMember(binder.Name, value, token);

            return true;
        }

        private object GetMember(string name, JToken token)
        {
            switch (token.Type)
            {
                case JTokenType.Array:
                    return new JsonResolver(token as JArray);
                case JTokenType.Object:
                    return new JsonResolver(token as JObject);
                case JTokenType.String:
                    return token.Value<string>();
                case JTokenType.Integer:
                    return token.Value<int>();
                case JTokenType.Boolean:
                    return token.Value<bool>();
                case JTokenType.Float:
                    return token.Value<float>();
                default:
                    throw new JsonResolverException($"Unsupported type: name={name}, type={token.Type}");
            }
        }

        private void SetMember(string name, object value, JToken token)
        {
            try
            {
                if(value is JsonResolver)
                {
                    value = (value as JsonResolver).Token;
                }

                if (!(token is JValue))
                {
                    throw new JsonResolverException($"Type does not support modification: name={name}, type={token.Type}");
                }
                (token as JValue).Value = value;
            }
            catch(JsonResolverException exp)
            {
                throw exp;
            }
            catch(Exception exp)
            {
                throw new JsonResolverException(exp.Message, exp);
            }
        }

        private object InvokeMember(string name, object[] args)
        {
            switch(name.ToUpper())
            {
                case "COUNT":
                    return this.Count();
                case "ADD":
                    return this.Add(args);
                case "REMOVE":
                    return this.Remove(args);
                case "CLEAR":
                    return this.Clear();
                default:
                    throw new JsonResolverException($"Unsupported method: name={name}");
            }
        }

        private void GetTokenByIndexs(object[] indexes, ref string name, ref JToken token)
        {
            if (token == null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            foreach (object item in indexes)
            {
                int index;
                name = item.ToString();

                if (int.TryParse(item.ToString(), out index))
                {
                    if (token.Type != JTokenType.Array)
                    {
                        throw new JsonResolverException($"Index operation not supported: index={index}, type={this.Token.Type}");
                    }
                    token = token[index];
                }
                else
                {
                    if (token.Type != JTokenType.Object)
                    {
                        throw new JsonResolverException($"Index operation not supported: index={item}, type={this.Token.Type}");
                    }
                    token = token[item.ToString()];
                }

                if (token == null)
                {
                    throw new JsonResolverException($"index does not exist: index={item}");
                }
            }
        }

        public override string ToString()
        {
            return this.Token.ToString();
        }

        public IEnumerator GetEnumerator()
        {
            return new JsonResolverEnumerator(this);
        }

        private class JsonResolverEnumerator : IEnumerator
        {
            private JsonResolver Resolver;
            private JToken Token;

            private int Index;
            public int Count { get; private set; }

            public JsonResolverEnumerator(JsonResolver resolver)
            {
                this.Resolver = resolver;
                this.Token = resolver.Token;
                this.Reset();
            }

            object IEnumerator.Current
            {
                get
                {
                    JToken token = null;
                    if (this.Token.Type == JTokenType.Array)
                    {
                        token = (this.Token as JArray)[this.Index];
                    }
                    else if (this.Token.Type == JTokenType.Object)
                    {
                        token = this.Token;
                    }

                    return this.Resolver.GetMember(this.Index.ToString(), token);
                }
            }

            public bool MoveNext()
            {
                this.Index++;
                return this.Index < this.Count;
            }

            public void Reset()
            {
                this.Index = -1;
                this.Count = this.Resolver.Count();
            }
        }
    }
}
*/