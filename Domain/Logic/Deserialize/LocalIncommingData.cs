using System;
using System.Collections.Generic;
using System.Text;

namespace Olive.Microservices.Hub
{
    internal class LocalIncommingData
    {

        internal Service Service { set; get; }
        internal FeatureDefinition[] Features { set; get; }
        internal string[] BoardSources { set; get; }
        internal bool GlobalySearchable { set; get; }

    }
}
