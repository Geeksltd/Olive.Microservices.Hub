namespace Olive.Microservices.Hub
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using AppStart;
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

    public abstract class HubStartup : FS.Shared.Website.Startup<ReferenceData, BackgroundTask, TaskManager>
    {
        protected HubStartup(IWebHostEnvironment env, IConfiguration config, ILoggerFactory factory) : base(env, config, factory)
        {
            Subdomains = config["GeeksSubdomain"]?.Split(",") ?? new string[0];
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
            app.UseResponseCompression();

            if (Subdomains.Any())
                app.UseCors("AllowSubdomains");

            base.Configure(app);

            Console.Title = Microservice.Me.Name;

            Feature.DataProvider.Register();
            Service.DataProvider.Register();
            Board.DataProvider.Register();

            AppContentService.HubApi.DefaultConfig(config => config.Cache(CachePolicy.CacheOrFreshOrNull));
        }

        protected override void ConfigureMiddlewares(IApplicationBuilder app)
        {
            base.ConfigureMiddlewares(app);
            app.UseGlobalSearch<GlobalSearchSource>();
        }

        protected override void ConfigureRequestHandlers(IApplicationBuilder app)
        {
            StructureDeserializer.Load();

            base.ConfigureRequestHandlers(app);
            app.Use(RedirectSmartPhone);
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
                Email = "jack.smith@geeks.ltd",
                DisplayName = "Jack Smith",
                IsActive = true,
                ID = "5C832BDB-145D-4DD6-8A28-379CC504B5EA".To<Guid>(),
                Roles = "Employee,Dev,JuniorDev,SeniorDev,LeadDev,HeadDev,QA,JuniorQA,SeniorQA,LeadQA,HeadQA,BA,JuniorBA,SeniorBA,LeadBA,HeadBA,PM,JuniorPM,SeniorPM,LeadPM,HeadPM,AM,JuniorAM,SeniorAM,LeadAM,HeadAM,Director,JuniorDirector,SeniorDirector,LeadDirector,HeadDirector,Designer,JuniorDesigner,SeniorDesigner,LeadDesigner,HeadDesigner,IT,JuniorIT,SeniorIT,LeadIT,HeadIT,Reception,JuniorReception,SeniorReception,LeadReception,HeadReception,PA,JuniorPA,SeniorPA,LeadPA,HeadPA,Sales,JuniorSales,SeniorSales,LeadSales,HeadSales,DevOps,JuniorDevOps,SeniorDevOps,LeadDevOps,HeadDevOps"
            });
        }
    }
}