namespace Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json;
    using Olive;
    using Olive.Entities;
    using Olive.Microservices.Hub;
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
        public async Task<ActionResult> HealthCheckAll([FromServices] IConfiguration configuration)
        {
            var microserviceKeys = configuration
                .GetSection("Microservice")
                .GetChildren()
                .Select(x => x.GetValue<string>("Name"))
                .ExceptNull()
                .Distinct()
                .Cast<string>()
                .ToArray();

            var result = new ConcurrentDictionary<string, string>(microserviceKeys.ToDictionary(
                key => key,
                key => ""
            ));

            await Parallel.ForEachAsync(microserviceKeys, async (key, token) =>
            {
                try
                {
                    var service = Microservice.Of(key);
                    if (service == null)
                    {
                        result[key] = "Not Configured";
                        return;
                    }
                    var url = service.Url("healthcheck");
                    var client = new HttpClient();
                    var response = await client.GetStringAsync(url);
                    if (response.HasValue())
                        result[key] = "OK: " + response;
                    else
                        result[key] = "No Response";
                }
                catch (Exception ex)
                {
                    result[key] = "Error: " + ex.Message;
                }
            });

            var html = "<h1>Health Check - All Microservices</h1><table border='1' cellpadding='5'>" +
                "<tr><th>Microservice</th><th>Status</th></tr>" +
                result.OrderBy(x => x.Key).Select(x => $"<tr><td>{x.Key}</td><td>{x.Value.HtmlEncode()}</td></tr>").ToString("") +
                "</table>";

            return Content(html, "text/html");
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