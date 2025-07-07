namespace MarketAsset.API.Configuration
{
    public class FintachartsOptions
    {
        public string BaseUrl { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string TokenEndpoint { get; set; } = string.Empty;
        public string InstrumentsEndpoint { get; set; } = string.Empty;
        public string HistoryEndpoint { get; set; } = string.Empty;
        public int PageSize { get; set; } = 10000;
    }
}
