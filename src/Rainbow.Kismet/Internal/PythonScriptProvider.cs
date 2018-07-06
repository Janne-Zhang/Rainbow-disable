using IronPython.Hosting;
using Microsoft.Scripting.Hosting;
using Rainbow.Kismet.Infrastructure;

namespace Rainbow.Kismet.Internal
{
    internal class PythonScriptProvider : IScriptProvider
    {
        public ScriptEngine CreateScriptEngine()
        {
            ScriptRuntime runt = Python.CreateRuntime();
            return Python.GetEngine(runt);
        }
    }
}
