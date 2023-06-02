namespace Olive.Microservices.Hub.Domain.Utilities
{
	using System;
	using System.Collections.Generic;

	public class ThemeProvider
	{
		public static string GetRootPath(bool withCurrentTheme)
		{
			var root = Microservice.Me.Url().TrimEnd("/");
			if (root.Contains("hub.")) root = root.Remove("hub.") + "/hub";

			return withCurrentTheme 
				? $"{root}/themes/{Config.Get("Theme:Name", "default")}" 
				: root;
		}
		public static string GetPrimaryColor()
		{
			return Config.Get("Theme:PrimaryColor", "#42AAA9");
		}
	}
}