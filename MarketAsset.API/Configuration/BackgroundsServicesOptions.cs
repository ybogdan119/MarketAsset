namespace MarketAsset.API.Configuration
{
    public class BackgroundsServicesOptions
    {
        public string WebSocketUrl { get; set; } = string.Empty;
        public int WsDelaySeconds { get; set; } = 30;
        public int UpdateAssetsIntervalSeconds { get; set; } = 60;
        public int SleepIntervalSeconds { get; set; } = 60;
    }
}
