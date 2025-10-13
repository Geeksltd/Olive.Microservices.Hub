
using Olive.BlobAws;
using Olive.Microservices.Hub.Domain.Theme.Contracts;
using Olive.Mvc;
using System;
using System.Threading.Tasks;

namespace Olive.Microservices.Hub.Domain.Theme.LoginLoggers
{
    public partial class S3LoginLogger : IThemeLoginLogger
    {
        public string Name => "S3";

        public async Task Log(string email, LoginLogStatus status, string? message = null)
        {
            var fileKey = $"login-logs/{email}/{DateTime.Now:yyyy-MM-dd}.log";

            var returnUrl = Context.Current.Request().Param("ReturnUrl");
            returnUrl = returnUrl.HasValue() ? returnUrl.FromSafeZippedUrl() : "N/A";

            var logMessage = $"{DateTime.Now:HH:mm:ss} - {status}{(message.HasValue() ? $" - {message}" : "")} | ReturnUrl={returnUrl}";

            if (await RawS3.Exists(fileKey))
            {
                var log = await RawS3.ReadTextFile(fileKey);
                logMessage = $"{logMessage}{Environment.NewLine}{Environment.NewLine}{log}";
            }

            await RawS3.WriteTextFile(fileKey, logMessage);
        }
    }
}