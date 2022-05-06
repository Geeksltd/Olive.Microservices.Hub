using System;
using System.Collections.Generic;
using System.Text;

namespace Olive.Microservices.Hub
{
    public class LocalIncommingData
    {
        public Service Service { set; get; }
        public FeatureDefinition[] Features { set; get; }
        public string[] BoardSources { set; get; }
        public bool GlobalySearchable { set; get; }

    }
}
