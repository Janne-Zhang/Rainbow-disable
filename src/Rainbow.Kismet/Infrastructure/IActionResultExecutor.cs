using System.Threading.Tasks;

namespace Rainbow.Kismet.Infrastructure
{
    public interface IActionResultExecutor<in TResult> where TResult : IActionResult
    {
        Task ExecuteAsync(ActionContext context, TResult result);
    }
}
