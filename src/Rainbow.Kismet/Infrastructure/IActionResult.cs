using System.Threading.Tasks;

namespace Rainbow.Kismet.Infrastructure
{
    public interface IActionResult
    {
        Task ExecuteResultAsync(ActionContext context);
    }
}
