namespace Olive.Microservices.Hub.Utilities.UrlExtensions
{
    public static class Extensions
    {
        public static string AppendUrlPath(this string @this, string path)
            => @this.WithSuffix("/" + path.TrimStart("/"));
    }
}