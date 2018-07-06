using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using Rainbow.Kismet.Infrastructure;
using Rainbow.Kismet.Internal;
using Microsoft.Extensions.Options;
using Rainbow.Kismet;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class KismetServiceCollectionExtensions
    {
        public static void AddKismet(this IServiceCollection services)
        {
            services.TryAddEnumerable(
                ServiceDescriptor.Transient<IConfigureOptions<KismetOptions>, KismetOptionsSetup>());


            services.TryAddSingleton<IScriptProvider, PythonScriptProvider>();
            services.TryAddSingleton<IRouteParser, DefaultRouteParser>();

            services.TryAddSingleton(ArrayPool<byte>.Shared);
            services.TryAddSingleton(ArrayPool<char>.Shared);

            services.TryAddSingleton<IContentTypeProvider, FileExtensionContentTypeProvider>();

            services.TryAddSingleton<IHttpRequestStreamReaderFactory, MemoryPoolHttpRequestStreamReaderFactory>();
            services.TryAddSingleton<IHttpResponseStreamWriterFactory, MemoryPoolHttpResponseStreamWriterFactory>();

            services.TryAddSingleton<IActionResultExecutor<ContentResult>, ContentResultExecutor>();
            services.TryAddSingleton<IActionResultExecutor<FileResult>, FileResultExecutor>();
            services.TryAddSingleton<IActionResultExecutor<RedirectResult>, RedirectResultExecutor>();




            //marker
            services.TryAddSingleton<KismetMarkerService, KismetMarkerService>();
        }
    }
}
