using System.IO;
using System.Text;

namespace Rainbow.Kismet.Infrastructure
{
    public interface IHttpRequestStreamReaderFactory
    {
        TextReader CreateReader(Stream stream, Encoding encoding);
    }
}
