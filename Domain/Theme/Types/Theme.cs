﻿namespace Olive.Microservices.Hub.Domain.Theme.Types
{
	public class Theme
	{
		public string Name { get; set; } = "default";
		public string PrimaryColor { get; set; } = "#42AAA9";
		public string ValidationFunction { get; set; } = "ForceTrue";
		public string? HomePageUrl { get; set; } = "dashboard/home.aspx";
		public SidebarProfileUrl? SidebarProfileUrl { get; set; } = new()
		{
			Default = "/person/%EMAIL%"
		};

		public string? LoginUrl { get; set; } = "/login";


		public override string ToString()
		{
			return $"{Name} : {ValidationFunction}";
		}
	}
}