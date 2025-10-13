namespace Olive.Microservices.Hub.Domain.Theme.Types
{
    public class MagicLink
    {
        public int ExpirationMinutes { get; set; }
        public bool LogInvalidLinks { get; set; }
        public string[] Providers { get; set; }
        public string[] EmailTo { get; set; }
    }
}