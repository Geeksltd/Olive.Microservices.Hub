using Microsoft.AspNetCore.Hosting;
using PeopleService;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Olive.Microservices.Hub.Domain.Theme
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Olive;
    using Olive.Microservices.Hub.Domain.Theme.Contracts;
    using Olive.Microservices.Hub.Domain.Theme.LoginLoggers;
    using Olive.Microservices.Hub.Domain.Theme.Types;
    using System;
    using System.Collections.Generic;

    internal class ThemeProvider : IThemeProvider
    {
        private readonly IThemeValidations _themeValidations;
        private readonly IThemeLoginLoggers _loginLoggers;
        private readonly IWebHostEnvironment _environment;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private Theme _currentTheme = new();
        private bool _initialized;

        public ThemeProvider(IThemeValidations themeValidations, IThemeLoginLoggers loginLoggers, IWebHostEnvironment environment,
            IHttpContextAccessor httpContextAccessor)
        {
            _themeValidations = themeValidations;
            _loginLoggers = loginLoggers;
            _environment = environment;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<Theme> GetCurrentTheme()
        {
            if (_initialized) return _currentTheme;

            foreach (var section in Config.GetSection("Themes").GetChildren())
            {
                var theme = ReadTheme(section);

                if (!await _themeValidations.IsValid(theme)) continue;

                _currentTheme = theme;
                break;
            }

            _initialized = true;

            _currentTheme.Copyright = _currentTheme.Copyright?.Replace("%Year%", DateTime.Now.Year.ToString());

            return _currentTheme;
        }

        /// <summary>Binds one Themes entry. The role based settings are read explicitly rather
        /// than bound, for the reason set out in <see cref="RoleBasedReader"/>.</summary>
        static Theme ReadTheme(IConfigurationSection section)
        {
            var theme = section.Get<Theme>() ?? new Theme();

            theme.HomePageUrl = RoleBasedReader.Read<HomePageUrl, string>(section.GetSection(nameof(Theme.HomePageUrl)));
            theme.SidebarProfileUrl = RoleBasedReader.Read<SidebarProfileUrl, string>(section.GetSection(nameof(Theme.SidebarProfileUrl)));
            theme.HideEveryThingMenuItem = RoleBasedReader.Read<HideEveryThingMenuItem, bool?>(section.GetSection(nameof(Theme.HideEveryThingMenuItem)));

            return theme;
        }

        public async Task<string> GetRootPath(bool withCurrentTheme)
        {
            var root = Microservice.Me.Url().TrimEnd("/");
            if (root.Contains("hub.")) root = root.Remove("hub.") + "/hub";

            var theme = await GetCurrentTheme();

            return withCurrentTheme
                ? $"{root}/themes/{theme.Name}"
                : root;
        }

        public async Task<string> GetPrimaryColor()
        {
            var theme = await GetCurrentTheme();
            return theme.PrimaryColor;
        }

        public async Task<string?> GetUserImage(UserInfo? user)
        {
            if (user is null) return null;
            var theme = await GetCurrentTheme();
            return theme.UserImageUrlTemplate.HasValue()
                ? theme.UserImageUrlTemplate?.Replace("%USER_ID%", user.ID.ToString())
                : user.ImageUrl;
        }

        public async Task<string?> GetLoginUrl()
        {
            if (!_initialized) await GetCurrentTheme();
            return _currentTheme.LoginUrl;
        }

        /// <summary>Whether the Everything item is left out of the side menu for the current user.
        /// Unconfigured means it is shown, which is how the setting behaved as a plain bool.</summary>
        public async Task<bool> IsEverythingMenuItemHidden()
        {
            if (!_initialized) await GetCurrentTheme();
            return ResolveByRole(_currentTheme.HideEveryThingMenuItem) ?? false;
        }

        public async Task<string> ExtraStylesTag()
        {
            var root = await GetRootPath(true);
            var extraStylesPath = Path.Combine(_environment.WebRootPath, "themes", _currentTheme.Name, "extra-styles.css");
            var tag = File.Exists(extraStylesPath)
                ? $"<link rel='stylesheet' href='{root}/extra-styles.css?v={AppResourceVersion}' type='text/css' />"
                : "";
            return tag;
        }

        public string AppResourceVersion => Config.Get("App.Resource.Version");

        public async Task<HomePageUrl?> GetHomePage()
        {
            if (!_initialized) await GetCurrentTheme();
            return _currentTheme.HomePageUrl;
        }

        public async Task<string> GetHomePageUrl()
        {
            var home = await GetHomePage();

            return ResolveByRole(home).Or("dashboard/home.aspx");
        }

        public async Task<string> GetSupportEmail()
        {
            if (!_initialized) await GetCurrentTheme();

            var configured = _currentTheme.SupportEmail.OrEmpty();
            if (configured.HasValue()) return configured;

            var domain = Config.Get("Authentication:Cookie:Domain").Or("app.geeks.ltd").RemoveBefore(".").Trim('.');
            return $"support@{domain}";
        }

        public async Task<SidebarProfileUrl?> GetSidebarProfile()
        {
            if (!_initialized) await GetCurrentTheme();
            return _currentTheme.SidebarProfileUrl;
        }

        public async Task<string> GetSidebarProfileUrl(Dictionary<string, string> parameters)
        {
            var profile = await GetSidebarProfile();
            return GetSidebarProfileUrl(profile, parameters);
        }

        private string GetSidebarProfileUrl(SidebarProfileUrl? profile, Dictionary<string, string> parameters)
        {
            return RenderSidebarProfileUrl(ResolveByRole(profile).Or(
                $"https://hub.{Config.Get("Authentication:Cookie:Domain").EnsureEndsWith("/")}person/%EMAIL%"), parameters);
        }

        private string RenderSidebarProfileUrl(string sidebarProfileUrl, Dictionary<string, string> parameters)
        {
            foreach (var key in parameters.Keys)
            {
                sidebarProfileUrl = sidebarProfileUrl.Replace($"%{key}%", parameters[key]);
            }

            return sidebarProfileUrl;
        }

        /// <summary>Returns the value configured for the highest ranking role that the user holds,
        /// or Default when no configured role matched or the matched entry left the value unset.</summary>
        private TValue? ResolveByRole<TValue>(RoleBased<TValue>? setting)
        {
            if (setting is null) return default;

            var byRole = setting.Roles is null ? default : TryGetByRole(setting.Roles);

            return IsSet(byRole) ? byRole : setting.Default;
        }

        /// <summary>Configuration cannot preserve the order the roles were written in - the binder
        /// sorts the keys - so ranking is by Priority, and entries left at the default 0 keep key
        /// order.</summary>
        private TValue? TryGetByRole<TValue>(Dictionary<string, RoleValue<TValue>> roles)
        {
            foreach (var keyValue in roles.OrderBy(x => x.Value?.Priority ?? 0))
                if (IsInRole(keyValue.Key))
                    return keyValue.Value is null ? default : keyValue.Value.Value;

            return default;
        }

        /// <summary>An empty url counts as unset, which is how the url settings behaved before they
        /// were generalised.</summary>
        static bool IsSet<TValue>(TValue? value) => value is string url ? url.HasValue() : value is not null;

        /// <summary>Checks the current user against a role name from configuration. IsInRole()
        /// compares role claims ordinally, so a case insensitive scan backs it up to keep
        /// configuration whose casing differs from the claim working as it did before.</summary>
        private bool IsInRole(string role)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user is null) return false;

            role = role.OrEmpty().Trim();
            if (role.IsEmpty()) return false;

            return user.IsInRole(role) || user.GetRoles().Any(x => x.OrEmpty().Trim().Equals(role, false));
        }

        public async Task LogLoginStatus(string email, LoginLogStatus status, string? message = null)
        {
            if (!_initialized) await GetCurrentTheme();
            await _loginLoggers.Log(_currentTheme, email, status, message);
        }

        public async Task<int> OtpExpirationMinutes()
        {
            if (!_initialized) await GetCurrentTheme();
            return _currentTheme.Otp?.ExpirationMinutes > 0
                ? _currentTheme.Otp.ExpirationMinutes.Value
                : 10;
        }
    }
}
