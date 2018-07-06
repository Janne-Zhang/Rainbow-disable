﻿using Microsoft.AspNetCore.WebUtilities;
using Rainbow.Kismet.Infrastructure;
using System;
using System.Buffers;
using System.IO;
using System.Text;

namespace Rainbow.Kismet.Internal
{
    internal class MemoryPoolHttpResponseStreamWriterFactory : IHttpResponseStreamWriterFactory
    {
        public static readonly int DefaultBufferSize = 16 * 1024;
        private readonly ArrayPool<byte> _bytePool;
        private readonly ArrayPool<char> _charPool;

        public MemoryPoolHttpResponseStreamWriterFactory(
            ArrayPool<byte> bytePool,
            ArrayPool<char> charPool)
        {
            _bytePool = bytePool ?? throw new ArgumentNullException(nameof(bytePool));
            _charPool = charPool ?? throw new ArgumentNullException(nameof(charPool));
        }

        public TextWriter CreateWriter(Stream stream, Encoding encoding)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            if (encoding == null)
            {
                throw new ArgumentNullException(nameof(encoding));
            }

            return new HttpResponseStreamWriter(stream, encoding, DefaultBufferSize, _bytePool, _charPool);
        }
    }
}
