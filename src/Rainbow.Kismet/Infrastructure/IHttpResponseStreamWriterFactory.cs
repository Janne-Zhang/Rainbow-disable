using System.IO;
using System.Text;

namespace Rainbow.Kismet.Infrastructure
{
    public interface IHttpResponseStreamWriterFactory
    {
        TextWriter CreateWriter(Stream stream, Encoding encoding);
    }
}
