namespace Olive.Microservices.Hub
{
    using System.Linq;
    using System.Security.Claims;
    using System.Xml.Linq;
    using Olive;
    using Olive.Entities;

    /// <summary>
    /// Provides the business logic for Extensions class.
    /// </summary>
    public static class Extensions
    {
        public static string GetFullPath(this IHierarchy @this, string separator = ">") => @this.WithAllParents().Select(i => i.ToString()).ToString($" {separator} ");

        public static bool CanSee(this ClaimsPrincipal @this, Feature feature)
        {
            foreach (var notPermission in feature.NotPermissions)
                if (@this.IsInRole(notPermission)) return false;

            if (feature.Permissions.None())
            {
                foreach (var child in feature.Children)
                    if (CanSee(@this, child) && child.ImplementationUrl.HasValue()) return true;


                return false;
            }

            return feature.Permissions.Any(p => @this.IsInRole(p));
        }

        internal static string GetCleanName(this XElement @this)
        {
            return @this.Name.LocalName
                  .Replace("_STAR_", "*")
                  .Replace("_AND_", " & ")
                  .Replace("_SLASH_", "/")
                  .Replace("_DASH_", "-")
                  .Replace("_QUESTION_", "?")
                  .Replace("_", " ");
        }

        internal static string InjectId(this string @this, string id)
        {
            var urlParts = @this.Replace("{id}", id).Split('&', '?');
            return urlParts.FirstOrDefault() + urlParts.ExceptFirst().ToString("&").WithPrefix("?");
        }
        public static string Between(this string str, string firstString, string lastString)
        {
            var firest = str.IndexOf(firstString);
            var secend = str.IndexOf(lastString);
            if (firest == -1 || secend == -1)
                return null;

            return str.Substring(firest + firstString.Length, secend - (firest + firstString.Length));

        }
    }
}