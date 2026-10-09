using Newtonsoft.Json;
using Olive.Microservices.Hub.Domain.Theme.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Olive.Microservices.Hub.Domain.Utilities.JsVariable
{
    public class JsVariableProvider : IJsVariableProvider
    {
        readonly IThemeProvider ThemeProvider;

        // The boards are the website's compiled types, so they cannot change while it runs. Scanning them
        // used to happen on every page render.
        static readonly Lazy<string[]> Boards = new(FindBoards);

        public JsVariableProvider(IThemeProvider themeProvider) => ThemeProvider = themeProvider;

        static string[] FindBoards()
        {
            var boardsAssemblyName =
                AppDomain.CurrentDomain
                    .GetBaseDirectory()
                    .GetFiles()
                    .SingleOrDefault(f => f.Name.Equals("website.dll", false))?
                    .FullName;

            return boardsAssemblyName.IsEmpty()
                ? Array.Empty<string>()
                : Assembly.LoadFrom(boardsAssemblyName!)
                    .GetTypes()
                    .Where(a => typeof(IBoardController).IsAssignableFrom(a))
                    .Select(a => a.Name.Replace("BoardController", "").ToLower())
                    .ToArray();
        }

        public async Task<string> Render()
        {
            var boards = Boards.Value;

            var user = Context.Current.User();

            var items = new Dictionary<string, object>
            {
                { "services", Service.GetAllForJsVariables() },
                { "boards", boards },
                { "isEmployee", user?.IsInRole("Employee") == true },
				// Named on the access denied view, where being signed in as the wrong one of two accounts
				// is the usual cause. Empty for an anonymous request, and the view drops the sentence.
				{ "userEmail", user?.GetEmail().OrEmpty() },
                { "supportEmail", await ThemeProvider.GetSupportEmail() },
            };

            var builder = new StringBuilder();
            foreach (var item in items)
            {
                builder.AppendLine($"window[\"{item.Key}\"] = {JsonConvert.SerializeObject(item.Value)};");
            }

            return builder.ToString();
        }
    }
}
