namespace MarketAsset.API.DTO
{
    /// <summary>
    /// Represents the price information for a specific asset.
    /// </summary>
    public class AssetPriceDto
    {
        /// <summary>
        /// The trading symbol of the asset (e.g., "EUR/USD").
        /// </summary>
        public string Symbol { get; set; } = string.Empty;

        /// <summary>
        /// The latest known price of the asset. Can be null if the price has not yet been received.
        /// </summary>
        public decimal? LatestPrice { get; set; }

        /// <summary>
        /// The timestamp of the last price update. Can be null if there was no update yet.
        /// </summary>
        public DateTime? LastUpdated { get; set; }
    }
}
