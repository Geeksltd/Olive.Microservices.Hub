namespace Olive.Microservices.Hub.Domain.Theme.Types
{
    public class OtpConfigs
    {
        public string? EmailTemplate { get; set; }
        public int? ExpirationMinutes { get; set; }
        public string? EmailSubject { get; set; }
        public string? FromAddress { get; set; }
        public string? FromName { get; set; }
    }
}