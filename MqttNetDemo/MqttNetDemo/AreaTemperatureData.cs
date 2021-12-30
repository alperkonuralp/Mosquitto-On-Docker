namespace MqttNetDemo
{
    public class AreaTemperatureData
    {
        public string? Name { get; set; }

        public double Temperature { get; set; }

        public long DateTimeOfUnixTimestamp { get; set; }
    }
}