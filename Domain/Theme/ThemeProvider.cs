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

        public static T? GetConfig<T>(string sectionName)
        {
            var section = Config.GetSection(sectionName);
            var data = section.Get<T>();
            return data;

        }

        public async Task<Theme> GetCurrentTheme()
        {
            if (_initialized) return _currentTheme;

            var themes = GetConfig<Theme[]>("Themes");

            if (themes != null)
            {
                foreach (var item in themes)
                {
                    if (!await _themeValidations.IsValid(item)) continue;

                    _currentTheme = item;
                    break;
                }
            }

            _initialized = true;

            _currentTheme.Copyright = _currentTheme.Copyright?.Replace("%Year%", DateTime.Now.Year.ToString());

            return _currentTheme;
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

            string? homePageUrl = "";

            if (home?.Roles != null)
                homePageUrl = TryGetUrlByRole(home.Roles);

            if (homePageUrl.IsEmpty())
                homePageUrl = home?.Default;

            return homePageUrl.Or("dashboard/home.aspx");
        }

        public async Task<string> GetSupportEmail()
        {
            if (!_initialized) await GetCurrentTheme();

            var configured = (_currentTheme.SupportEmail).Or(Config.Get<string>("SupportEmail", ""));
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
            string? sidebarProfileUrl = "";

            if (profile?.Roles != null)
                sidebarProfileUrl = TryGetUrlByRole(profile.Roles);

            if (sidebarProfileUrl.IsEmpty())
                sidebarProfileUrl = profile?.Default;

            return RenderSidebarProfileUrl(sidebarProfileUrl.Or(
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

        private string? TryGetUrlByRole(Dictionary<string, string> roles)
        {
            foreach (var keyValue in roles)
                if (IsInRole(keyValue.Key))
                    return keyValue.Value;

            return null;
        }

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
