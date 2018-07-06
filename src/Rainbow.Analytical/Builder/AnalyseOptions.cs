using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Microsoft.AspNetCore.Builder
{
    public class AnalyseOptions
    {
        private static AnalyseOptions _Default = null;

        public static AnalyseOptions Default
        {
            get
            {
                if(_Default == null)
                {
                    _Default = new AnalyseOptions()
                    {

                    };
                }
                return _Default;
            }
        }

        public AnalyseOptions()
        {

        }

        public AnalyseOptions(string xmlConfigPath)
        {

        }

        public AnalyseOptions(XmlReader xmlConfig)
        {

        }
    }
}
