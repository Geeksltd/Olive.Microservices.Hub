using Olive.Microservices.Hub.Domain.Theme.Contracts;
using Olive.Microservices.Hub.Domain.Theme.LoginLoggers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Olive.Microservices.Hub.Domain.Theme
{
    public class ThemeLoginLoggers : IThemeLoginLoggers
    {
        static List<IThemeLoginLogger> _loggers = new();
        public async Task Log(Types.Theme currentTheme, string email, LoginLogStatus status, string message = null)
        {
            if (currentTheme.LogUserLogins?.Enabled != true || !email.HasValue()) return;
            var loggers = currentTheme.LogUserLogins.Providers.Distinct();

            if (_loggers.Count == 0) { LoadMap(); }

            foreach (var loggerName in loggers)
            {
                var loginLogger = _loggers.SingleOrDefault(x => x.Name == loggerName);
                if (loginLogger == null) continue;
                await loginLogger.Log(email, status, message);
            }
        }

        void LoadMap()
        {
            var types = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(a => a.IsClass && typeof(IThemeLoginLogger).IsAssignableFrom(a))
                .ToList();

            _loggers = types
                .Select(a => Activator.CreateInstance(a) as IThemeLoginLogger)
                .Where(a => a is not null)
                .Cast<IThemeLoginLogger>()
                .ToList();
        }
    }
}
