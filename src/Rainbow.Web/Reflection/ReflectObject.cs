using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Rainbow.Web.Reflection
{
    public class ReflectObject : IDisposable
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private Type Type { get; set; }

        public Object Instance { get; set; }

        public ReflectObject(Type type)
        {
            this.Type = type;
        }

        public ReflectObject(Type type, object instance)
        {
            this.Type = type;
            this.Instance = instance;
        }

        public bool TryConstructor(params object[] args)
        {
            return _Constructor(false, args);
        }

        public ReflectObject Constructor(params object[] args)
        {
            _Constructor(true, args);
            return this;
        }

        private bool _Constructor(bool throwOnError, params object[] args)
        {
            this.Dispose();

            if (this.Type.IsValueType)
            {
                this.Instance = this.Type.Assembly.CreateInstance(this.Type.FullName);
                return true;
            }
            else
            {
                ConstructorInfo[] constructors = this.Type.GetConstructors();
                if (constructors == null || constructors.Length == 0)
                {
                    if (!throwOnError)
                        return false;
                    throw new ReflectException("constructor not found!");
                }

                foreach (ConstructorInfo constructor in constructors)
                {
                    if (this.InvokeMethod(constructor, ref args))
                    {
                        this.Instance = constructor.Invoke(args);
                        return true;
                    }
                }
            }

            if (!throwOnError)
                return false;

            throw new ReflectException("constructor not found!");
        }

        public void Setter(string propertyName, object value)
        {
            _Setter(true, propertyName, value);
        }

        public bool TrySetter(string propertyName, object value)
        {
            return _Setter(false, propertyName, value);
        }

        private bool _Setter(bool throwOnError, string propertyName, object value)
        {
            FieldInfo field = this.Type.GetField(propertyName);
            if (field != null)
            {
                if (!field.IsStatic && this.Instance == null)
                {
                    if (!throwOnError)
                        return false;

                    throw new ReflectException("Object has not been initialized!");
                }

                field.SetValue(this.Instance, value);
                return true;
            }

            PropertyInfo property = this.Type.GetProperty(propertyName);
            if (property == null)
            {
                if (!throwOnError)
                    return false;

                throw new ReflectException($"property '{propertyName}' not found!");
            }
            if (!property.GetSetMethod().IsStatic && this.Instance == null)
            {
                if (!throwOnError)
                    return false;

                throw new ReflectException("Object has not been initialized!");
            }
            if (!property.CanWrite)
            {
                if (!throwOnError)
                    return false;

                throw new ReflectException($"property '{propertyName}' cann't write!");
            }

            property.SetValue(this.Instance, value, null);
            return true;
        }

        public bool TryGetter(string propertyName, ref object value)
        {
            return _Getter(false, propertyName, ref value);
        }

        public object Getter(string propertyName)
        {
            object value = null;
            _Getter(true, propertyName, ref value);
            return value;
        }

        private bool _Getter(bool throwOnError, string propertyName, ref object value)
        {
            FieldInfo field = this.Type.GetField(propertyName);
            if (field != null)
            {
                if (!field.IsStatic && this.Instance == null)
                {
                    if (!throwOnError)
                        return false;

                    throw new ReflectException("Object has not been initialized!");
                }

                value = field.GetValue(this.Instance);
                return true;
            }

            PropertyInfo property = this.Type.GetProperty(propertyName);
            if (property == null)
            {
                if (!throwOnError)
                    return false;

                throw new ReflectException($"property '{propertyName}' not found!");
            }
            if (!property.GetGetMethod().IsStatic && this.Instance == null)
            {
                if (!throwOnError)
                    return false;

                throw new ReflectException("Object has not been initialized!");
            }
            if (!property.CanRead)
            {
                if (!throwOnError)
                    return false;

                throw new ReflectException($"property '{propertyName}' cann't read!");
            }

            value = property.GetValue(this.Instance, null);
            return true;
        }

        public T Getter<T>(string propertyName)
        {
            return (T)this.Getter(propertyName);
        }

        public ReflectObject InvokeReflect(string methodName, params object[] args)
        {
            object value = this.Invoke(methodName, args);
            Type type = value.GetType();
            ReflectObject rst = new ReflectObject(type);
            rst.Instance = value;
            return rst;
        }

        public bool TryInvoke(string methodName, params object[] args)
        {
            object value = null;
            return _Invoke(false, methodName, ref value, args);
        }

        public bool TryInvoke(ref object value, string methodName, params object[] args)
        {
            return _Invoke(true, methodName, ref value, args);
        }

        public object Invoke(string methodName, params object[] args)
        {
            object value = null;
            _Invoke(true, methodName, ref value, args);
            return value;
        }

        private bool _Invoke(bool throwOnError, string methodName, ref object value, params object[] args)
        {
            MethodInfo[] methods = this.Type.GetMethods().Where(p => p.Name == methodName).ToArray();
            if (methods == null || methods.Length == 0)
            {
                if (!throwOnError)
                    return false;

                throw new ReflectException($"method '{methodName}' not found!");
            }

            foreach (MethodInfo method in methods)
            {
                if (this.InvokeMethod(method, ref args))
                {
                    if (!method.IsStatic && this.Instance == null)
                    {
                        if (!throwOnError)
                            return false;

                        throw new ReflectException("Object has not been initialized!");
                    }
                    value = method.Invoke(this.Instance, args);
                    return true;
                }
            }

            if (!throwOnError)
                return false;

            throw new ReflectException($"method '{methodName}' not found!");
        }

        private bool InvokeMethod(MethodBase method, ref object[] args)
        {
            ParameterInfo[] parameters = method.GetParameters();
            int argsCount = args == null ? 0 : args.Length;

            if (argsCount <= parameters.Length)
            {
                int index = 0;
                for (; index < argsCount; index++)
                {
                    object newValue = null;
                    if (this.Convert(parameters[index].ParameterType, args[index], ref newValue))
                    {
                        args[index] = newValue;
                    }
                    else
                    {
                        return false;
                    }
                }

                object[] newArgs = null;

                for (; index < parameters.Length; index++)
                {
                    if (!parameters[index].IsOptional)
                    {
                        return false;
                    }
                    if (newArgs == null)
                    {
                        newArgs = new object[parameters.Length];
                    }
                    newArgs[index] = parameters[index].DefaultValue;
                }

                if (newArgs != null)
                {
                    Array.Copy(args, 0, newArgs, 0, args.Length);
                    args = newArgs;
                    return true;
                }
                return true;
            }

            return false;
        }

        public T Invoke<T>(string methodName, params object[] args)
        {
            object value = this.Invoke(methodName, args);
            object newValue = null;

            if (this.Convert(typeof(T), value, ref newValue))
            {
                return (T)newValue;
            }
            return (T)value;
        }

        private bool Convert(Type source, object value, ref object newValue)
        {
            if (source.IsInstanceOfType(value) || value == null)
            {
                newValue = value;
                return true;
            }
            else if (source.IsValueType || value.GetType().IsValueType)
            {
                ReflectObject reflect = new ReflectObject(source.IsValueType ? source : value.GetType());
                MethodInfo[] methods = reflect.Type.GetMethods().Where(p => p.Name == "op_Implicit").ToArray();
                foreach (MethodInfo method in methods)
                {
                    ParameterInfo[] parameters = method.GetParameters();
                    if (parameters.Length == 1
                        && parameters[0].ParameterType.IsInstanceOfType(value)
                        && method.ReturnType == source)
                    {
                        reflect.Constructor();
                        newValue = method.Invoke(reflect.Instance, new object[] { value });
                        return true;
                    }
                }
            }

            return false;
        }

        public void Dispose()
        {
            if (this.Instance != null)
            {
                IDisposable res = this.Instance as IDisposable;
                if (res != null)
                {
                    res.Dispose();
                }
            }
        }
    }
}
