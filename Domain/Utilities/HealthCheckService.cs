using Olive.Microservices.Hub.Domain.Theme.Contracts;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Olive.Microservices.Hub.Domain.Utilities
{
    public interface IHealthCheckService
    {
        Task<string> HealthCheckAll();
        Task<string> HealthCheckAlert();
    }

    public class HealthCheckService : IHealthCheckService
    {
        public IHttpClientFactory HttpClientFactory { get; }

        public HealthCheckService(IHttpClientFactory httpClientFactory)
        {
            HttpClientFactory = httpClientFactory;
        }

        public async Task<string> HealthCheckAll()
        {
            var microservices = AllMicroservices.GetServices();

            var result = new ConcurrentDictionary<Microservice, string>(microservices.ToDictionary(
                service => service,
                service => ""
            ));

            await Parallel.ForEachAsync(microservices, async (service, token) =>
            {
                try
                {
                    var url = service.Url("healthcheck");
                    var client = HttpClientFactory.CreateClient();
                    client.Timeout = 5.Seconds();
                    var response = await client.GetStringAsync(url);
                    if (response.HasValue())
                        result[service] = $"<span class='text-success'>{response}</span>";
                    else
                        result[service] = "<span class='text-muted'>No Response</span>";
                }
                catch (Exception ex)
                {
                    result[service] = $"<span class='text-muted'>Error: {ex.Message}</span>";
                }
            });

            var html = @"
                <h1 class='my-5'>Health Check - All Microservices</h1>
                <table class='table table-stripped table-hover'>
                    <tr>
                        <th>#</th>
                        <th>Icon</th>
                        <th>Microservice</th>
                        <th>Status</th>
                        <th>SSO</th>
                        <th>Iframe</th>
                    </tr>" +
                        result.OrderBy(x => x.Key.Name).Select((x, i) =>
                    $@"<tr>
                        <td>{i + 1}</td>
                        <td>{(x.Key.Icon.HasValue() ? $"<i class='fa {x.Key.Icon}'></i>" : "")}</td>
                        <td><a href='{x.Key.Url()}'>{x.Key.Name}</a></td>
                        <td>{x.Value}</td>
                        <td>{(x.Key.Sso ? $"<i class='fa fa-check text-success'></i>" : "<i class='fa fa-times text-muted'></i>")}</td>
                        <td>{(x.Key.Iframe ? $"<i class='fa fa-check text-success'></i>" : "<i class='fa fa-times text-muted'></i>")}</td>
                    </tr>").ToString("") +
                "</table>";

            return string.Format(await HtmlTemplate(), html);
        }

        public async Task<string> HealthCheckAlert()
        {
            var emailAddress = Config.GetOrThrow("HealthCheckAlertRecipients");
            var emails = emailAddress.Split(',', StringSplitOptions.RemoveEmptyEntries).Where(x => x.IsEmailAddress()).ToArray();
            if (emails.Length == 0) throw new Exception("Please define health-check alert recipients");

            var microservices = AllMicroservices.GetServices();

            await Parallel.ForEachAsync(microservices, async (service, token) =>
            {
                try
                {
                    var url = service.Url("healthcheck");
                    var client = HttpClientFactory.CreateClient();
                    client.Timeout = 15.Seconds();
                    var response = await client.GetStringAsync(url);
                }
                catch (Exception ex)
                {
                    await emails.DoAsync(async email =>
                    {
                        await new EmailService.SendEmailCommand
                        {
                            To = email,
                            Html = true,
                            Subject = $"Service is down: {service.Name} @ {LocalTime.Now}",
                            Body = $"Service is down: {service.Name} @ {LocalTime.Now}<br/>{ex.Message}"
                        }.Publish();
                    });
                }
            });

            return "Done";
        }


        async Task<string> HtmlTemplate()
        {
            var themeProvider = Context.Current.GetService<IThemeProvider>();
            var themeRoot = await themeProvider.GetRootPath(true);

            return $@"
                <!DOCTYPE html>
                <html>
                    <head>
                        <meta charset=""utf-8"" />          
                        <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
                        <title>Health Check - All Microservices</title>
                        <link rel=""stylesheet"" href=""{themeRoot}/styles.min.css?v={themeProvider.AppResourceVersion}"" type=""text/css"" />
                    </head>
                    <body>
                        <div class='container-fluid py-5'>
                            <div class='row'>
                                <div class='col'>
                                    {{0}}
                                </div>
                            </div>
                        </div>
                    </body>
                </html>";
        }
    }
}
