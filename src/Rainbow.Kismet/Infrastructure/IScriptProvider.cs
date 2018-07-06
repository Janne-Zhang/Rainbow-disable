using Microsoft.Scripting.Hosting;

namespace Rainbow.Kismet.Infrastructure
{
    public interface IScriptProvider
    {
        ScriptEngine CreateScriptEngine();
    }
}