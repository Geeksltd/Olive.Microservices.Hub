using Olive.Microservices.Hub.Domain.Theme.Contracts;
using Olive.Mvc;
using System;
using System.Threading.Tasks;

namespace Olive.Microservices.Hub.Domain.Theme.LoginLoggers
{
    public partial class EmailMagicLinkLogger : IInvalidMagicLinkLogger
    {
        public string Name => "Email";

        public async Task Log(string[] to, string email, string token, DateTime? createOn)
        {
            var title = $"Invalid magic link";

            var returnUrl = Context.Current.Request().Param("ReturnUrl");
            returnUrl = returnUrl.HasValue() ? returnUrl.FromSafeZippedUrl() : "N/A";

            string[] logMessage = [$"On: {createOn?.ToLongDateString()} {createOn?.ToShortTimeString()}", $"Email: {email}", $"Token: {token}", $"ReturnUrl: {returnUrl}"];

            await to.DoAsync(async recipient =>
            {
                await new EmailService.SendEmailCommand
                {
                    To = recipient,
                    Subject = title,
                    Body = logMessage.ToString("<br/>")
                }.Publish();
            });

        }
    }
}