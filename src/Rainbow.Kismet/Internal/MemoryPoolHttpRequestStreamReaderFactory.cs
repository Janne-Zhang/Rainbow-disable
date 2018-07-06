using Microsoft.AspNetCore.WebUtilities;
using Rainbow.Kismet.Infrastructure;
using System;
using System.Buffers;
using System.IO;
using System.Text;

namespace Rainbow.Kismet.Internal
{
    internal class MemoryPoolHttpRequestStreamReaderFactory : IHttpRequestStreamReaderFactory
    {
        public static readonly int DefaultBufferSize = 1024;
        private readonly ArrayPool<byte> _bytePool;
        private readonly ArrayPool<char> _charPool;

        public MemoryPoolHttpRequestStreamReaderFactory(
            ArrayPool<byte> bytePool,
            ArrayPool<char> charPool)
        {
            _bytePool = bytePool ?? throw new ArgumentNullException(nameof(bytePool));
            _charPool = charPool ?? throw new ArgumentNullException(nameof(charPool));
        }

        public TextReader CreateReader(Stream stream, Encoding encoding)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            if (encoding == null)
            {
                throw new ArgumentNullException(nameof(encoding));
            }

            return new HttpRequestStreamReader(stream, encoding, DefaultBufferSize, _bytePool, _charPool);
        }
    }
}
