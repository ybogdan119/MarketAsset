using System.Text.Json.Serialization;

namespace MarketAsset.API.DTO;

/// <summary>
/// Represents the response received after successfully requesting an access token from the authentication server.
/// </summary>
public class TokenResponse
{
    /// <summary>
    /// The access token string used for authenticating subsequent API requests.
    /// </summary>
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// The duration in seconds until the token expires.
    /// </summary>
    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }
}
