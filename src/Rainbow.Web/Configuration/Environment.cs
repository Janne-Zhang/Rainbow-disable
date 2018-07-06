using System.IO;

namespace Rainbow.Web.Configuration
{
    public static class Environment
    {
        public static readonly string root;

        public static readonly string configuration;

        static Environment()
        {
            root = new System.IO.FileInfo(typeof(Environment).Assembly.Location).Directory.FullName;
            configuration = Path.Combine(root, ".config"); // string.Concat(root, ".config");
        }
    }
}
