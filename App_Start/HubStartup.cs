namespace Olive.Microservices.Hub
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.Google;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Olive;
    using Olive.Entities.Data;
    using Olive.Microservices.Hub;
    using Olive.PassiveBackgroundTasks;

    public abstract class HubStartup<TTaskManager> : FS.Shared.Website.Startup<ReferenceData, BackgroundTask, TTaskManager> where TTaskManager : BackgroundJobsPlan, new()
    {
        protected HubStartup(IWebHostEnvironment env, IConfiguration config, ILoggerFactory factory) : base(env, config, factory)
        {
            Subdomains = config["HubSubdomain"]?.Split(",") ?? new string[0];

            if (env.IsProduction()) Features.SetRepository(new S3FeatureRepository());
            else Features.SetRepository(new IOFeatureRepository());
        }

        protected virtual bool IsProduction() => false;
        string[] Subdomains;

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddResponseCompression();
            if (Subdomains.Any())
                services.AddCors(c => c.AddPolicy("AllowSubdomains", builder =>
                {
                    var domainProtocol = $"http{"s".OnlyWhen(IsProduction())}://*.";

                    var domains = from d in Subdomains
                                  let trimmed = d.TrimStart("*").TrimStart(".")
                                  select domainProtocol.WithSuffix(d);

                    builder.WithOrigins(domains.ToArray())
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials()
                .SetIsOriginAllowedToAllowWildcardSubdomains();
                }));

            base.ConfigureServices(services);
        }

        public override void Configure(IApplicationBuilder app)
        {
            AppContentService.HubApi.DefaultConfig(config => config.Cache(CachePolicy.CacheOrFreshOrNull));
            app.UseResponseCompression();

            if (Subdomains.Any())
                app.UseCors("AllowSubdomains");

            base.Configure(app);

            Console.Title = Microservice.Me.Name;

            Feature.DataProvider.Register();
            Service.DataProvider.Register();
            Board.DataProvider.Register();

        }

        protected override void ConfigureMiddlewares(IApplicationBuilder app)
        {
            app.UseGlobalSearch<GlobalSearchSource>();
            //app.Use(RedirectSmartPhone);
            base.ConfigureMiddlewares(app);
        }

        protected override void ConfigureRequestHandlers(IApplicationBuilder app)
        {
            StructureDeserializer.Load();
            base.ConfigureRequestHandlers(app);
        }
        protected abstract void ConfigureDataProtectionProvider(GoogleOptions config);

        static async Task RedirectSmartPhone(Microsoft.AspNetCore.Http.HttpContext context, Func<Task> next)
        {
            if (context.Request.Path.Value == "/" && context.Request.IsSmartPhone())
                context.Response.Redirect("/root");
            else await next();
        }
    }

    public class ReferenceData : IReferenceData
    {
        public async Task Create()
        {
            await Context.Current.Database().Save(new PeopleService.UserInfo
            {
                Email = Config.Get("Authentication:SimulateLogin:Email"),
                DisplayName = Config.Get("Authentication:SimulateLogin:DisplayName"),
                IsActive = true,
                ID = Config.Get("Authentication:SimulateLogin:Id").To<Guid>(),
                Roles = Config.Get("Authentication:SimulateLogin:Roles")
            });
        }
    }
}