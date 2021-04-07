using System.Linq;
using System.Security.Claims;
using Olive;
using Olive.Entities;

namespace Domain
{
    public static class FeatureSecurityFilter
    {
        public static AuthroziedFeatureInfo[] GetAuthorizedFeatures(ClaimsPrincipal user, Feature parent = null)
        {
            var features = parent?.Children ?? Feature.All.Where(f => f.Parent is null);

            features = features.Except(x => x.Title == "WIDGETS");

            return features.Select(f => GetAuthorizationInfo(user, f)).ExceptNull().ToArray();
        }

        static AuthroziedFeatureInfo GetAuthorizationInfo(ClaimsPrincipal user, Feature feature)
        {
            if (user.CanSee(feature))
                return new AuthroziedFeatureInfo { Feature = feature };

            var hasPermittedChildNodes = feature.GetAllChildren().Cast<Feature>().Any(c => user.CanSee(c));

            if (hasPermittedChildNodes)
                return new AuthroziedFeatureInfo { Feature = feature, IsDisabled = true };

            return null;
        }
    }
}