using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Olive.Microservices.Hub.Domain.Theme
{
    using Olive.Microservices.Hub.Domain.Theme.Contracts;
    using Olive.Microservices.Hub.Domain.Theme.Types;
    using Microsoft.Extensions.Configuration;
    using Olive;
    using System;
    using System.Collections.Generic;

    internal class ThemeProvider : IThemeProvider
    {
        private readonly IThemeValidations _themeValidations;
        private readonly IWebHostEnvironment _environment;
        private Theme _currentTheme = new();
        private bool _initialized;

        public ThemeProvider(IThemeValidations themeValidations, IWebHostEnvironment environment)
        {
            _themeValidations = themeValidations;
            _environment = environment;
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

        public async Task<string?> GetLoginUrl()
        {
            if (!_initialized) await GetCurrentTheme();
            return _currentTheme.LoginUrl.Or(Config.Get<string>("LoginUrl"));
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

        public async Task<string> GetHomePageUrl()
        {
            if (!_initialized) await GetCurrentTheme();
            return (_currentTheme.HomePageUrl).Or(Config.Get<string>("HomePageUrl", "dashboard/home.aspx"));
        }

        public async Task<SidebarProfileUrl?> GetSidebarProfile()
        {
            if (!_initialized) await GetCurrentTheme();
            if (_currentTheme.SidebarProfileUrl != null) return _currentTheme.SidebarProfileUrl;
            return GetConfig<SidebarProfileUrl>(nameof(SidebarProfileUrl));
        }

        public async Task<string> GetSidebarProfileUrl(string[] userRoles, Dictionary<string, string> parameters)
        {
            var profile = await GetSidebarProfile();
            return GetSidebarProfileUrl(profile, userRoles, parameters);
        }

        private string GetSidebarProfileUrl(SidebarProfileUrl? profile, string[] userRoles, Dictionary<string, string> parameters)
        {
            string? sidebarProfileUrl = "";

            if (profile?.Roles != null)
                sidebarProfileUrl = TryGetSidebarProfileUrlByRole(profile.Roles, userRoles);

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

        private string? TryGetSidebarProfileUrlByRole(Dictionary<string, string> roles, string[] userRoles)
        {
            foreach (var keyValue in roles)
                if (userRoles.Any(a => a.Equals(keyValue.Key, false)))
                    return keyValue.Value;

            return null;
        }
    }
}