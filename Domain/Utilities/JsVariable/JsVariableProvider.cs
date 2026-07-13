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

		public JsVariableProvider(IThemeProvider themeProvider) => ThemeProvider = themeProvider;

		public async Task<string> Render()
		{
			var boardsAssemblyName =
				AppDomain.CurrentDomain
					.GetBaseDirectory()
					.GetFiles()
					.SingleOrDefault(f => f.Name.Equals("website.dll", false))?
					.FullName;

			var boards = boardsAssemblyName.IsEmpty()
				? Array.Empty<string>()
				: Assembly.LoadFrom(boardsAssemblyName!)
					.GetTypes()
					.Where(a => typeof(IBoardController).IsAssignableFrom(a))
					.Select(a => a.Name.Replace("BoardController", "").ToLower())
					.ToArray();

			var items = new Dictionary<string, object>
			{
				{ "services", Service.GetAllForJsVariables() },
				{ "boards", boards },
				{ "isEmployee", Context.Current.User()?.IsInRole("Employee") == true },
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
