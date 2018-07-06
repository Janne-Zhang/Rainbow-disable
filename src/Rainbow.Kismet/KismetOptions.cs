using System;
using System.Collections.Generic;
using System.Text;

namespace Rainbow.Kismet
{
    public class KismetOptions
    {
        public List<string> Tests { get; set; }

        public KismetOptions()
        {
            this.Tests = new List<string>();
        }
    }
}
