namespace Olive.Microservices.Hub
{
    using Microsoft.AspNetCore;
    using Microsoft.AspNetCore.Hosting;
    using Olive.Logging;
    using Olive.Mvc;

    public class Program
    {
        public static void Main(string[] args) => CreateWebHostBuilder(args).Build().Run();

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                .ConfigureLogging(p => p.AddEventBus())
                .UseSetting("detailedErrors", "true")
                .CaptureStartupErrors(true)
                .UseStartup(typeof(HubStartup<ITaskManager>).Assembly.FullName);
        }

        // static void ConfigureLogging(WebHostBuilderContext context, ILoggingBuilder logging) { }
    }
}