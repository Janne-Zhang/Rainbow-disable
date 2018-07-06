using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Rainbow.Web.Reflection
{
    public class ReflectContainer
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string Path { get; set; }

        public Assembly Assembly { get; private set; }

        public ReflectContainer(Assembly assembly)
        {
            this.Assembly = assembly;
        }

        public ReflectContainer(string component)
        {
            if(component.Contains(System.IO.Path.DirectorySeparatorChar))
            {
                if (File.Exists(this.Path))
                {
                    this.Assembly = Assembly.LoadFile(this.Path);
                    return;
                }
            }
            this.Assembly = Assembly.Load(component);
        }

        /// <summary>
        /// 返回指定路径下所有的 *.dll 文件
        /// </summary>
        public static string[] GetComponents(string path)
        {
            return Directory.GetFiles(path, "*.dll", SearchOption.TopDirectoryOnly);
        }

        /// <summary>
        /// 返回指定路径下所有名称满足search规则的文件
        /// </summary>
        public static string[] GetComponents(string path, string search)
        {
            return Directory.GetFiles(path, search, SearchOption.TopDirectoryOnly);
        }

        /// <summary>
        /// 返回继承于 T 的所有类型
        /// </summary>
        public Type[] GetInherits<T>()
        {
            Type source = typeof(T);
            return this.Assembly.GetTypes().Where(p => p.IsSubclassOf(source)).ToArray();
        }

        /// <summary>
        /// 查找类型名称为 typeName 的 T 类型，并使用 args 进行初始化的实例
        /// </summary>
        public T GetInstance<T>(string typeName, params object[] args)
        {
            Type type = this.Assembly.GetType(typeName);
            ReflectObject reflect = new ReflectObject(type).Constructor(args);
            return (T)reflect.Instance;
        }

        /// <summary>
        /// 查找名称为 typeName 的类型，使用 args 进行初始化并返回 ReflectObject 对象
        /// </summary>
        public ReflectObject GetReflectObject(string typeName, params object[] args)
        {
            Type type = this.Assembly.GetType(typeName);
            return new ReflectObject(type);
        }

        /// <summary>
        /// 查找名称为 typeName 的类型，使用 args 进行初始化并返回透明代理
        /// </summary>
        public T GetReflectProxy<T>(string typeName, params object[] args) where T : IReflectProxy
        {
            Type type = this.Assembly.GetType(typeName);
            ReflectObject reflect = new ReflectObject(type).Constructor(args);
            return ReflectProxy<T>.Creator(reflect);
        }

        /// <summary>
        /// 使用 args 参数实例化 type 类型
        /// </summary>
        public static T GetInstance<T>(Type type, params object[] args)
        {
            ReflectObject reflect = new ReflectObject(type).Constructor(args);
            return (T)reflect.Instance;
        }

        /// <summary>
        /// 使用 args 参数实例化 type 类型，并返回 ReflectObject
        /// </summary>
        public static ReflectObject GetReflectObject(Type type, params object[] args)
        {
            return new ReflectObject(type);
        }

        /// <summary>
        /// 使用 args 参数实例化 type 类型，并返回透明代理
        /// </summary>
        public static T GetReflectProxy<T>(Type type, params object[] args) where T : IReflectProxy
        {
            ReflectObject reflect = new ReflectObject(type).Constructor(args);
            return ReflectProxy<T>.Creator(reflect);
        }
    }
}
