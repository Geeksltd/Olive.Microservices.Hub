using Olive.Microservices.Hub.Domain.Theme.LoginLoggers;
using PeopleService;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Olive.Microservices.Hub.Domain.Theme.Contracts;

public interface IThemeProvider
{
    Task<Types.Theme> GetCurrentTheme();
    Task<string> GetRootPath(bool withCurrentTheme);
    Task<string> GetPrimaryColor();
    Task<string> GetHomePageUrl();
    Task<string> GetSidebarProfileUrl(string[] userRoles, Dictionary<string, string> parameters);
    Task<string> GetUserImage(UserInfo user);
    Task<string?> GetLoginUrl();
    Task<string> ExtraStylesTag();
    Task LogLoginStatus(string email, LoginLogStatus status, string? message = null);
    Task LogInvalidMagicLink(string? email, string token, DateTime? createOn);
    Task<int> MagicLinkExpirationMinutes();

    string AppResourceVersion { get; }
}