using Microsoft.EntityFrameworkCore;

namespace MarketAsset.API.Data;

/// <summary>
/// Represents the Entity Framework Core database context for the Market Asset API.
/// It provides access to application-level database sets and configuration.
/// </summary>
public class AppDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AppDbContext"/> class using the specified options.
    /// </summary>
    /// <param name="options">The options to be used by the DbContext.</param>
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    /// <summary>
    /// Gets or sets the table of market assets stored in the local database.
    /// </summary>
    public DbSet<Asset> Assets { get; set; } = null!;
}
