using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.WebHost.UseUrls("http://0.0.0.0:5273");

builder.Services.AddSingleton<IInfluxDBClient>(sp =>
{
    var influxUrl = "http://localhost:8086";  
    var token = "WeatherMonitor12345";         
    return InfluxDBClientFactory.Create(influxUrl, token.ToCharArray());
});

var app = builder.Build();

app.UseRouting();
app.MapControllers();

app.Run();
