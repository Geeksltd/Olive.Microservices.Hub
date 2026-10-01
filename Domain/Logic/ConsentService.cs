namespace Olive.Microservices.Hub
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Olive;
    using Olive.Entities;

    /// <summary>
    /// Cookie consent for logged in users. The answer is held against the person in ConsentItems,
    /// not in a cookie, so once someone has chosen they are not asked again on another device,
    /// in another browser, or after clearing cookies. Anonymous visitors (the login pages) are
    /// never asked, and Google Consent Mode stays at its denied defaults for them.
    ///
    /// Only functional and analytics cookies are offered. The hub and its microservices serve no
    /// advertising, so there is no marketing choice: ad_storage, ad_user_data and ad_personalization
    /// are always denied. If advertising is ever added, add the category back and bump PolicyVersion
    /// so everyone is asked again.
    /// </summary>
    public static class ConsentService
    {
        /// <summary>Bump to ask everyone again, e.g. after the cookie policy changes.</summary>
        public const int PolicyVersion = 1;

        public static class Sources
        {
            public const string BannerAccept = "banner-accept";
            public const string BannerReject = "banner-reject";
            public const string ModalAccept = "modal-accept";
            public const string ModalReject = "modal-reject";
            public const string ModalSave = "modal-save";

            public static readonly string[] All = [BannerAccept, BannerReject, ModalAccept, ModalReject, ModalSave];
        }

        const string ItemsKey = "Hub.ConsentState";

        static IDatabase Database => Context.Current.Database();

        public class State
        {
            /// <summary>False for anonymous visitors and for anyone without a person id.</summary>
            public bool CanChoose { get; set; }

            /// <summary>The person has an answer for the current policy version.</summary>
            public bool HasChosen { get; set; }

            public bool NeedsPrompt => CanChoose && !HasChosen;

            public bool Functional { get; set; }
            public bool Analytics { get; set; }
        }

        /// <summary>
        /// The current user's consent. Read once per request: the Tag Manager snippet and the
        /// banner both ask for it while rendering the same page.
        /// </summary>
        public static async Task<State> GetCurrent(HttpContext ctx)
        {
            if (ctx is null) return new State();
            if (ctx.Items.TryGetValue(ItemsKey, out var cached) && cached is State state) return state;

            state = await Load(ctx);
            ctx.Items[ItemsKey] = state;
            return state;
        }

        static async Task<State> Load(HttpContext ctx)
        {
            var personId = PersonIdOf(ctx);
            if (personId is null) return new State();

            var latest = await Latest(personId.Value);
            if (latest is null || latest.PolicyVersion < PolicyVersion)
                return new State { CanChoose = true };

            return new State
            {
                CanChoose = true,
                HasChosen = true,
                Functional = latest.Functional,
                Analytics = latest.Analytics
            };
        }

        static async Task<ConsentItem> Latest(Guid personId)
        {
            var items = await Database.GetList<ConsentItem>(c => c.PersonId == personId);
            return items.OrderByDescending(c => c.RegisteredAt).FirstOrDefault();
        }

        /// <summary>Null for anonymous users and for logins whose id is not a person (e.g. shell users).</summary>
        public static Guid? PersonIdOf(HttpContext ctx)
        {
            if (ctx?.User?.Identity?.IsAuthenticated != true) return null;
            return Guid.TryParse(ctx.User.GetId(), out var id) ? id : null;
        }

        public static async Task<State> Record(HttpContext ctx, bool functional, bool analytics, string source, string pageUrl)
        {
            var personId = PersonIdOf(ctx) ?? throw new InvalidOperationException("Consent can only be recorded for a logged in person.");

            await Database.Save(new ConsentItem
            {
                PersonId = personId,
                PolicyVersion = PolicyVersion,
                Necessary = true,
                Functional = functional,
                Analytics = analytics,
                Source = Sources.All.Contains(source) ? source : "unknown",
                IP = Truncate(ctx.Request.GetIPAddress(), 45),
                UserAgent = Truncate(ctx.Request.Headers.UserAgent.ToString(), 500),
                PageUrl = PageOf(ctx, pageUrl)
            });

            var state = new State
            {
                CanChoose = true,
                HasChosen = true,
                Functional = functional,
                Analytics = analytics
            };

            ctx.Items[ItemsKey] = state;
            return state;
        }

        /// <summary>
        /// The page the choice was made on, for the audit row. The client's value is untrusted, so
        /// anything that is not an absolute http(s) url is discarded and the Referer used instead.
        /// </summary>
        static string PageOf(HttpContext ctx, string claimed)
        {
            if (claimed.HasValue() &&
                Uri.TryCreate(claimed, UriKind.Absolute, out var url) &&
                (url.Scheme == Uri.UriSchemeHttp || url.Scheme == Uri.UriSchemeHttps))
                return Truncate(claimed, 2000);

            return Truncate(ctx.Request.Headers.Referer.ToString(), 2000);
        }

        static string Truncate(string s, int max)
        {
            if (s.IsEmpty()) return null;
            return s.Length <= max ? s : s.Substring(0, max);
        }
    }
}
