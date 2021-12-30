// See https://aka.ms/new-console-template for more information

using MQTTnet;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MqttNetDemo;

var managedMqttClient = new MqttClient("localhost", 1883, "mosquitto", "P@ssw0rd");

managedMqttClient.OnClientConnected += ClientConnected;
managedMqttClient.OnClientDisconnected += ClientDisconnected;

managedMqttClient.OnApplicationMessageReceived += HandleReceivedApplicationMessage;
await managedMqttClient.SubscribeAsync("SimpleMessage");


await managedMqttClient.ConnectAsync();

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

            await managedMqttClient.PublishAsync("SimpleMessage", simpleMessage);

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

            await managedMqttClient.PublishAsync("RoomTemperatures", areaTemperatureData);
        }),
        Task.Run(async () => { await Task.Delay(TimeSpan.FromSeconds(3)); })
        );
}

await Task.Delay(TimeSpan.FromSeconds(10));

managedMqttClient.Close();

void ClientConnected(object? sender, MqttClientConnectedEventArgs e)
{
    Console.WriteLine("Client Connection Result : " + e.ConnectResult.ResultCode);
}

void ClientDisconnected(object? sender, MqttClientDisconnectedEventArgs e)
{
    Console.WriteLine("Client Disconnection Result : " + e.Reason);
}


void HandleReceivedApplicationMessage(object? sender, MqttApplicationMessageReceivedEventArgs args)
{
    var message = System.Text.Encoding.UTF8.GetString(args.ApplicationMessage.Payload);
    Console.WriteLine($"Message Received from {args.ApplicationMessage.Topic} : {message}");
}