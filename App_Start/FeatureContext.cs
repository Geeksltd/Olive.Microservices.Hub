
using Olive;
using Olive.Microservices.Hub;

namespace Olive.Microservices.Hub
{
    public static class FeatureContext
    {
        public static Feature ViewingFeature
        {
            get => Context.Current.Http().Items["ViewingFeature"] as Feature;
            set => Context.Current.Http().Items["ViewingFeature"] = value;
        }
    }
}