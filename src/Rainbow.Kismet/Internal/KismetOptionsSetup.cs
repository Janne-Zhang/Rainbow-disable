using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rainbow.Kismet.Internal
{
    internal class KismetOptionsSetup : IConfigureOptions<KismetOptions>
    {
        public void Configure(KismetOptions options)
        {
            options.Tests.Add("test");
        }
    }
}
