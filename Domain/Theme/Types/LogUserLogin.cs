using System.Collections.Generic;

namespace Olive.Microservices.Hub.Domain.Theme.Types
{
    public class LogUserLogin
    {
        public bool Enabled { get; set; }
        public string[] Providers { get; set; }

        // Map of LoginLogStatus names to recipient emails. Both sides accept a
        // comma-separated list, e.g. "FailedManualLogin,FailedExternalLogin" -> "a@x, b@x".
        public Dictionary<string, string>? Notifications { get; set; }
    }
}
