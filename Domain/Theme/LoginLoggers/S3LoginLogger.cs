
using Olive.BlobAws;
using Olive.Microservices.Hub.Domain.Theme.Contracts;
using System;
using System.Threading.Tasks;

namespace Olive.Microservices.Hub.Domain.Theme.LoginLoggers
{
    public partial class S3LoginLogger : IThemeLoginLogger
    {
        public string Name => "S3";

        public async Task Log(string email, LoginLogStatus status, string? message = null)
        {
            var fileKey = $"login-logs/{email}/{System.DateTime.Now:yyyy-MM-dd}.log";
            if (await RawS3.Exists(fileKey))
            {
                var log = await RawS3.ReadTextFile(fileKey);
                log = $"{System.DateTime.Now:HH:mm:ss} - {status}" + (message.HasValue() ? $" - {message}" : "") + Environment.NewLine + log;
                await RawS3.WriteTextFile(fileKey, log);
            }
            else
            {
                var log = $"{System.DateTime.Now:HH:mm:ss} - {status}" + (message.HasValue() ? $" - {message}" : "") + "\r\n";
                await RawS3.WriteTextFile(fileKey, log);
            }
        }
    }
}