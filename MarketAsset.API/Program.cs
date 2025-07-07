using MarketAsset.API.Configuration;
using MarketAsset.API.Data;
using MarketAsset.API.Services;
using MarketAsset.API.Services.IServices;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.Configure<FintachartsOptions>(
    builder.Configuration.GetSection("Fintacharts"));

builder.Services.Configure<BackgroundsServicesOptions>(
    builder.Configuration.GetSection("BackgroundServices"));

builder.Services.AddSingleton<IFintachartsService, FintachartsService>();
builder.Services.AddSingleton<AssetUpdaterControlService>();

builder.Services.AddHostedService<AssetBackgroundUpdater>();
builder.Services.AddHostedService<FintaPriceWebSocketService>();


builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(8080);
});


var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthorization();

app.MapControllers();

// Ensure the database is created and migrations are applied
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.Run();
