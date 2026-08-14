namespace Olive.Microservices.Hub.Domain.Theme.Types
{
    public class Theme
    {
        public string Name { get; set; } = "default";
        public string PrimaryColor { get; set; } = "#42AAA9";
        public string ValidationFunction { get; set; } = "ForceTrue";
        public LogUserLogin? LogUserLogins { get; set; }
        public OtpConfigs? Otp { get; set; }
        public HomePageUrl? HomePageUrl { get; set; }
        public SidebarProfileUrl? SidebarProfileUrl { get; set; }

        public string? SupportEmail { get; set; }
        public string? LoginUrl { get; set; } = "/login";
        public string? LoginTitle { get; set; }
        public string? Copyright { get; set; }
        public bool HideEveryThingMenuItem { get; set; }
        public string? UserImageUrlTemplate { get; set; }


        public override string ToString()
        {
            return $"{Name} : {ValidationFunction}";
        }
    }
}