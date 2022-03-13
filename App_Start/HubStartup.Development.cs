// namespace Olive.Microservices.Hub
// {
//    using System;
//    using System.Threading.Tasks;
//    using Microsoft.AspNetCore.Builder;
//    using Microsoft.AspNetCore.Hosting;
//    using Microsoft.Extensions.Configuration;
//    using Microsoft.Extensions.DependencyInjection;
//    using Microsoft.Extensions.Logging;
//    using Olive;
//    using Olive.Entities.Data;
//    using Olive.Mvc.Testing;
//    using Olive.Security;

//    public class StartupDevelopment : Startup
//    {
//        public StartupDevelopment(IWebHostEnvironment env, IConfiguration config, ILoggerFactory factory) : base(env, config, factory)
//        {
//        }

//        protected override void ConfigureDataProtectionProvider(Microsoft.AspNetCore.Authentication.Google.GoogleOptions config)
//        {
//            var key = Config.Get("Authentication:CookieDataProtectorKey");
//            config.DataProtectionProvider = new SymmetricKeyDataProtector("AuthCookies", key);
//        }

//        public override async Task OnStartUpAsync(IApplicationBuilder app)
//        {
//            await base.OnStartUpAsync(app);
//            await new PeopleService.HubEndPoint(typeof(PeopleService.UserInfo).Assembly).Subscribe();
//        }
//    }
// }