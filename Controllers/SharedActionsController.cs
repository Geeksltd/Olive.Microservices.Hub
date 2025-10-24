namespace Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;
    using Olive;
    using Olive.Entities;
    using Olive.Microservices.Hub;
    using Olive.Microservices.Hub.Domain.Theme.Contracts;
    using Olive.Mvc;
    using System;
    using System.Collections.Concurrent;
    using System.ComponentModel;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;

    public class SharedActionsController : BaseController
    {
        readonly IFileRequestService FileRequestService;
        readonly IFileAccessorFactory FileAccessorFactory;

        public SharedActionsController(
            IFileRequestService fileRequestService,
            IFileAccessorFactory fileAccessorFactory
        )
        {
            FileRequestService = fileRequestService;
            FileAccessorFactory = fileAccessorFactory;
        }

        const int MaxFileLength = 260;

        [Route("healthcheck")]
        public async Task<ActionResult> HealthCheck()
        {
            return Content($"Health check @ {LocalTime.Now.ToLongTimeString()}, " +
                $" version = {Config.Get("App.Resource.Version")} in env:{Context.Current.Environment().EnvironmentName}({Config.Get("Runtime")})," +
                Environment.NewLine + $" email: {User.GetEmail()} roles: {User.GetRoles().ToString("|")} {Config.Get("Authentication:Cookie:Domain")}");
        }

        [Route("healthcheck/all")]
        [Authorize(Roles = "DevOps")]
        public async Task<ActionResult> HealthCheckAll()
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
                    var client = new HttpClient() { Timeout = 5.Seconds() };
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

            var themeProvider = Context.Current.GetService<IThemeProvider>();
            var themeRoot = await themeProvider.GetRootPath(true);
            var template = $@"
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

            return Content(string.Format(template, html), "text/html");
        }

        [Route("healthcheck/alert")]
        public async Task<ActionResult> HealthCheckAlert()
        {
            var emailAddress = Config.Get("HealthCheckAlertRecipients");
            if (emailAddress.Length == 0) return Content("Please define health-check alert recipients");
            var emails = emailAddress.Split(',', StringSplitOptions.RemoveEmptyEntries).Where(x => x.IsEmailAddress()).ToArray();
            if (emails.Length == 0) return Content("Please define health-check alert recipients");

            var microservices = AllMicroservices.GetServices();

            await Parallel.ForEachAsync(microservices, async (service, token) =>
            {
                try
                {
                    var url = service.Url("healthcheck");
                    var client = new HttpClient() { Timeout = 10.Seconds() };
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

            return Content("Done");
        }

        [Route("error")]
        public async Task<ActionResult> Error() => View("error");

        [Route("error/404")]
        public new async Task<ActionResult> NotFound() => View("error-404");

        [HttpPost, Route("upload")]
        [Authorize]
        public async Task<ActionResult> UploadTempFileToServer(IFormFile[] files)
        {
            // Note: This will prevent uploading of all unsafe files defined at Blob.UnsafeExtensions
            // If you need to allow them, then comment it out.
            if (Blob.HasUnsafeFileExtension(files[0].FileName))
                return Json(new { Error = "Invalid file extension." });

            return Json(await FileRequestService.TempSaveUploadedFile(files[0]));
        }

        [Route("file")]
        public async Task<ActionResult> DownloadFile()
        {
            var path = Request.QueryString.ToString().TrimStart('?');
            var accessor = await FileAccessorFactory.Create(path, User);
            if (!accessor.IsAllowed()) return new UnauthorizedResult();

            if (accessor.Blob.IsMedia())
                return await RangeFileContentResult.From(accessor.Blob);
            else return await File(accessor.Blob);
        }

        [Route("temp-file/{key}")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public Task<ActionResult> DownloadTempFile(string key) => FileRequestService.Download(key);

        [HttpPost("local-setup")]
        public async Task<ActionResult> LocalSetup()
        {
            if (Context.Current.Environment().EnvironmentName != "Development") return Content("error-404");

            var data = JsonConvert.DeserializeObject<LocalIncommingData>(await Request.Body.ReadAllText());
            if (data == null) return Content("Data is null");

            StructureDeserializer.AddService(data.Service);
            data.Features.Do(x => x.For(data.Service));
            StructureDeserializer.AddFeatures(data.Features);
            StructureDeserializer.AddSources(data.BoardSources, data.Service, data.GlobalySearchable);

            return Content("OK");
        }
    }
}