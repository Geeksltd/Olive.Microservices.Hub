// namespace Olive.Microservices.Hub
// {
//    using Microsoft.AspNetCore.Authentication;
//    using Microsoft.AspNetCore.Hosting;
//    using Microsoft.Extensions.Configuration;
//    using Microsoft.Extensions.DependencyInjection;
//    using Microsoft.Extensions.Logging;
//    using Olive;

//    public class StartupUAT : StartupProduction
//    {
//        public StartupUAT(IWebHostEnvironment env, IConfiguration config, ILoggerFactory factory) : base(env, config, factory)
//        {
//            var domain = config["Authentication:Cookie:Domain"] ?? throw new System.Exception("Could not find the domain in Authentication:Cookie:Domain");
//            var section = config.GetSection("Microservice");

//            foreach (var service in section.GetChildren())
//            {
//                var key = section.Key + ":" + service.Key + ":Url";
//                config[key] = config[key]?.Replace("%DOMAIN%", domain);
//            }
//        }

//        protected override void AddGoogle(AuthenticationBuilder auth)
//        {
//            auth.AddGoogleOpenIdConnect(config =>
//            {
//                config.ClientId = Config.Get("Authentication:Google:ClientId");
//                config.ClientSecret = Config.Get("Authentication:Google:ClientSecret");
//            });
//        }
//    }
// }