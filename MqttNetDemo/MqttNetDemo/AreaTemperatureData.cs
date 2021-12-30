namespace MqttNetDemo
{
    public class AreaTemperatureData
    {
        public int Id { get; set; }
        public string? Name { get; set; }

        public double Temperature { get; set; }

        public long DateTimeOfUnixTimestamp { get; set; }
    }
}