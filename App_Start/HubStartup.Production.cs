//namespace Olive.Microservices.Hub
//{
//    using System.Threading.Tasks;
//    
//    using Microsoft.AspNetCore.Authentication.Cookies;
//    using Microsoft.AspNetCore.Authentication.Google;
//    using Microsoft.AspNetCore.Builder;
//    using Microsoft.AspNetCore.Hosting;
//    using Microsoft.Extensions.Configuration;
//    using Microsoft.Extensions.DependencyInjection;
//    using Microsoft.Extensions.Logging;
//    using Olive;
//    using Olive.Aws;
//    using Olive.PassiveBackgroundTasks;

//    public class StartupProduction : Startup
//    {
//        public StartupProduction(IWebHostEnvironment env, IConfiguration config, ILoggerFactory factory) : base(env, config, factory)
//        {

//        }

//        protected override SecretProviderType GetSecretsProvider() => SecretProviderType.SystemsManagerParameter;

//        public override void ConfigureServices(IServiceCollection services)
//        {
//            services.AddAntiforgery(x => x.SuppressXFrameOptionsHeader = true);
//            base.ConfigureServices(services);
//        }

//        protected override bool IsProduction() => true;

//        protected override void ConfigureDataProtectionProvider(GoogleOptions config) =>
//            config.DataProtectionProvider = new Olive.Security.Aws.KmsDataProtectionProvider();

//        protected override void ConfigureAuthCookie(CookieAuthenticationOptions options)
//        {
//            base.ConfigureAuthCookie(options);

//            options.Cookie.Domain = Config.Get("Authentication:Cookie:Domain");
//            options.DataProtectionProvider = new Olive.Security.Aws.KmsDataProtectionProvider();
//            options.SlidingExpiration = true;
//        }

//        protected override void ConfigureMvc(IMvcBuilder mvc)
//        {
//            mvc.SetCompatibilityVersion(Microsoft.AspNetCore.Mvc.CompatibilityVersion.Version_2_1);

//            mvc.AddRazorPagesOptions(options =>
//            {
//                options.Conventions.ConfigureFilter(new Microsoft.AspNetCore.Mvc.IgnoreAntiforgeryTokenAttribute());
//            });

//            base.ConfigureMvc(mvc);
//        }
//    }
//}