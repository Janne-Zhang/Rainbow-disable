using System;
using System.Reflection;

namespace Rainbow.Web.Reflection
{
    public class ReflectProxy<T> : DispatchProxy where T : IReflectProxy
    {
        private const string ReflectName = "Reflect";
        private ReflectObject Reflect = null;

        public static T Creator(ReflectObject reflect)
        {
            T instance = DispatchProxy.Create<T, ReflectProxy<T>>();
            instance.Reflect = reflect;
            return instance;
        }

        protected override object Invoke(MethodInfo targetMethod, object[] args)
        {
            if (targetMethod.Name.StartsWith("set_"))
            {
                string property = targetMethod.Name.Substring(4, targetMethod.Name.Length - 4);
                object value = args[0];
                return this.InvokeSetter(targetMethod, property, value);
            }
            else if (targetMethod.Name.StartsWith("get_"))
            {
                string property = targetMethod.Name.Substring(4, targetMethod.Name.Length - 4);
                return this.InvokeGetter(targetMethod, property);
            }
            else if (targetMethod.Name == "FieldSetter" && targetMethod.DeclaringType.Name == "Object")
            {
                string property = args[1].ToString();
                object value = args[2];
                return this.InvokeSetter(targetMethod, property, value);
            }
            else if (targetMethod.Name == "FieldGetter" && targetMethod.DeclaringType.Name == "Object")
            {
                string property = args[1].ToString();
                return this.InvokeGetter(targetMethod, property);
            }
            else
            {
                string method = targetMethod.Name;
                return this.InvokeMethod(targetMethod, method, args);
            }
        }

        protected virtual object InvokeMethod(MethodInfo msg, string methodName, params object[] args)
        {
            object rst = this.Reflect.Invoke(methodName, args);
            return rst;
        }

        protected virtual object InvokeGetter(MethodInfo msg, string propertyName)
        {
            object rst = this.Reflect.Getter(propertyName);
            return rst;
        }

        protected virtual object InvokeSetter(MethodInfo msg, string propertyName, object value)
        {
            if (propertyName == ReflectName)
            {
                this.Reflect = value as ReflectObject;
            }
            else
            {
                this.Reflect.Setter(propertyName, value);
            }
            return null;
        }
    }
}
