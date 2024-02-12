using System.Collections.Generic;
using System.Threading.Tasks;
using PeopleService;

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
    string AppResourceVersion { get; }
}