namespace WeatherAPI.Models
{
    public class SensorDataDto
    {
        public string? Location { get; set; }
        public double Temperature { get; set; }
        public double Humidity { get; set; }
        public double AirPPM { get; set; }
    }
}