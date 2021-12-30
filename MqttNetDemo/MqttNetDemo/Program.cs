// See https://aka.ms/new-console-template for more information

using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MqttNetDemo;

var managedMqttClient = new MqttClient("localhost", 1883, "mosquitto", "P@ssw0rd");

managedMqttClient.OnClientConnected += ClientConnected;
managedMqttClient.OnClientDisconnected += ClientDisconnected;

await managedMqttClient.Connect();

while (managedMqttClient.Client != null && !managedMqttClient.Client.IsConnected)
{
    await Task.Delay(TimeSpan.FromSeconds(1));
}

Random rnd = new();

for (int i = 0; i < 20; i++)
{
    await Task.WhenAll(
        Task.Run(async () =>
        {
            var simpleMessage = $"Simple Message - {i + 1}";

            await managedMqttClient.Publish("SimpleMessage", simpleMessage);

            Console.WriteLine($"'{simpleMessage}' send.");
        }),
        Task.Run(async () =>
        {
            var areaTemperatureData = new AreaTemperatureData
            {
                Id = i + 1,
                Name = $"Room {(i % 3) + 1}",
                Temperature = Math.Round(rnd.NextDouble() * 2 + 23, 2),
                DateTimeOfUnixTimestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds()
            };

            await managedMqttClient.Publish("RoomTemperatures", areaTemperatureData);
        }),
        Task.Run(async () => { await Task.Delay(TimeSpan.FromSeconds(3)); })
        );
}

await Task.Delay(TimeSpan.FromSeconds(10));

managedMqttClient.Close();

void ClientConnected(object? sender, MqttClientConnectedEventArgs e)
{
    Console.WriteLine("Client Connection Result : " + e.ConnectResult.ReasonString);
}

void ClientDisconnected(object? sender, MqttClientDisconnectedEventArgs e)
{
    Console.WriteLine("Client Disconnection Result : " + e.Reason);
}