// See https://aka.ms/new-console-template for more information

using MqttNetDemo;

var managedMqttClientPublisher = await MqttClient.Connect();

Random rnd = new Random();

for (int i = 0; i < 20; i++)
{
    var simpleMessage = $"Simple Message - {i + 1}";

    await MqttClient.Publish(managedMqttClientPublisher, "SimpleMessage", simpleMessage);

    Console.WriteLine($"'{simpleMessage}' send.");


    var areaTemperatureData = new AreaTemperatureData
    {
        Name = $"Room {(i % 3) + 1}",
        Temperature = Math.Round(rnd.NextDouble() * 2 + 23, 2),
        DateTimeOfUnixTimestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds()
    };

    await MqttClient.Publish(managedMqttClientPublisher, "RoomTemperatures", areaTemperatureData);

    await Task.Delay(TimeSpan.FromSeconds(3));
}

await Task.Delay(TimeSpan.FromSeconds(10));