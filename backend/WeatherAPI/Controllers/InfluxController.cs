using Microsoft.AspNetCore.Mvc;
using InfluxDB.Client;
using InfluxDB.Client.Writes;
using InfluxDB.Client.Api.Domain;
using WeatherAPI.Models;
using RestSharp;
using CsvHelper.Expressions;
using Microsoft.AspNetCore.Components.Routing;

namespace WeatherAPI.AddControllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InfluxController : ControllerBase
    {
        private readonly IInfluxDBClient _influxDBClient;
        private const string Org = "WeatherMonitor";       
        private const string Bucket = "WeatherBucket";

        public InfluxController(IInfluxDBClient influxDBClient)
        {
            _influxDBClient = influxDBClient;
        }

        [HttpPost("write")]
        public async Task<IActionResult> WriteData([FromBody] SensorDataDto data)
        {
            if (data == null)
                return BadRequest("Invalid JSON body");

            var point = PointData.Measurement("Measurements")
            .Tag("Location", data.Location)
            .Field("Temperature", data.Temperature)
            .Field("Humidity", data.Humidity)
            .Field("AirQuality", data.AirPPM)
            .Timestamp(DateTime.UtcNow, WritePrecision.Ns);

            var writeApi = _influxDBClient.GetWriteApiAsync();
            await writeApi.WritePointAsync(point, Bucket, Org);

            return Ok("Data written to database");
        }

        [HttpGet("read/graphs")]
        public async Task<IActionResult> ReadData([FromQuery] string? location = null)
        {
            var queryApi = _influxDBClient.GetQueryApi();
            var query = $"from(bucket: \"{Bucket}\") |> range(start: -1h)";

            query += " |> filter(fn: (r) => r._measurement == \"Measurements\")";

            if (!string.IsNullOrEmpty(location))
            {
                location = Uri.UnescapeDataString(location);
                query += $" |> filter(fn : (r) => r.Location == \"{location}\")";
            }

            query += " |> pivot(rowKey:[\"_time\"], columnKey:[\"_field\"], valueColumn:\"_value\")";

            var tables = await queryApi.QueryAsync(query, Org);

            var results = tables.SelectMany(table => table.Records.Select(record => new
            {
                time = record.GetTime()?.ToDateTimeUtc().ToString("o"),
                temperature = record.GetValueByKey("Temperature"),
                humidity = record.GetValueByKey("Humidity"),
                airQuality = record.GetValueByKey("AirQuality")
            }));

            return Ok(results);
        }

        [HttpGet("read/average")]
        public async Task<IActionResult> ReadDataAverage([FromQuery] DateTime week, [FromQuery] string? location = null)
        {
            var queryApi = _influxDBClient.GetQueryApi();

            var stop = week.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");
            var start = week.AddDays(-7).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");

            var query = $"from(bucket: \"{Bucket}\") |> range(start: time(v: \"{start}\"), stop: time(v: \"{stop}\"))";

            query += " |> filter(fn: (r) => r._measurement == \"Measurements\")";

            if (!string.IsNullOrEmpty(location))
            {
                location = Uri.UnescapeDataString(location);
                query += $" |> filter(fn : (r) => r.Location == \"{location}\")";
            }

            query += " |> filter(fn: (r) => r._field == \"Temperature\" or r._field == \"Humidity\" or r._field == \"AirQuality\")";
            query += " |> aggregateWindow(every: 1d, fn: mean)";
            query += " |> filter(fn: (r) => r._time <= today())";
            query += " |> pivot(rowKey:[\"_time\"], columnKey:[\"_field\"], valueColumn:\"_value\")";

            var tables = await queryApi.QueryAsync(query, Org);

            var results = tables.SelectMany(table => table.Records.Select(record => new
            {
                time = record.GetTime()?.ToDateTimeUtc().ToString("o"),
                temperature = record.GetValueByKey("Temperature"),
                humidity = record.GetValueByKey("Humidity"),
                airQuality = record.GetValueByKey("AirQuality")
            }));

            return Ok(results);
        }
    }
}