using Olive.Microservices.Hub.Domain.Theme.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Olive.Microservices.Hub.Domain.Theme
{
    public class InvalidMagicLinkLoggers : IInvalidMagicLinkLoggers
    {
        static List<IInvalidMagicLinkLogger> _loggers = new();
        public async Task Log(Types.Theme currentTheme, string email, string token, DateTime? createOn)
        {
            if (currentTheme.MagicLink?.LogInvalidLinks != true) return;
            var loggers = currentTheme.MagicLink.Providers.Distinct();
            if (!loggers.Any()) return;

            if (_loggers.Count == 0) { LoadMap(); }

            foreach (var loggerName in loggers)
            {
                var loginLogger = _loggers.SingleOrDefault(x => x.Name == loggerName);
                if (loginLogger == null) continue;
                await loginLogger.Log(currentTheme.MagicLink.EmailTo, email, token, createOn);
            }
        }

        void LoadMap()
        {
            var types = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(a => a.IsClass && typeof(IInvalidMagicLinkLogger).IsAssignableFrom(a))
                .ToList();

            _loggers = types
                .Select(a => Activator.CreateInstance(a) as IInvalidMagicLinkLogger)
                .Where(a => a is not null)
                .Cast<IInvalidMagicLinkLogger>()
                .ToList();
        }
    }
}
