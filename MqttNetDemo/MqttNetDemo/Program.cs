// See https://aka.ms/new-console-template for more information
//Console.WriteLine("Hello, World!");

using MQTTnet;
using MQTTnet.Client.Options;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Formatter;
using MQTTnet.Protocol;
using System.Text;

var mqttFactory = new MqttFactory();

var tlsOptions = new MqttClientTlsOptions
{
    UseTls = false,
    IgnoreCertificateChainErrors = true,
    IgnoreCertificateRevocationErrors = true,
    AllowUntrustedCertificates = true
};

var options = new MqttClientOptions
{
    ClientId = "ClientPublisher",
    ProtocolVersion = MqttProtocolVersion.V311,
    ChannelOptions = new MqttClientTcpOptions
    {
        Server = "localhost",
        Port = 1883,
        TlsOptions = tlsOptions
    }
};

if (options.ChannelOptions == null)
{
    throw new InvalidOperationException();
}

options.Credentials = new MqttClientCredentials
{
    Username = "mosquitto",
    Password = Encoding.UTF8.GetBytes("P@ssw0rd")
};

options.CleanSession = true;
options.KeepAlivePeriod = TimeSpan.FromSeconds(5);
var managedMqttClientPublisher = mqttFactory.CreateManagedMqttClient();

await managedMqttClientPublisher.StartAsync(
    new ManagedMqttClientOptions
    {
        ClientOptions = options
    });

for (int i = 0; i < 20; i++)
{
    var simpleMessage = $"Simple Message - {i + 1}";

    var payload = Encoding.UTF8.GetBytes(simpleMessage);
    var message = new MqttApplicationMessageBuilder()
        .WithTopic("Simple Message")
        .WithPayload(payload)
        .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
        .WithRetainFlag()
        .Build();

    await managedMqttClientPublisher.PublishAsync(message);

    Console.WriteLine($"'{simpleMessage}' send.");

    await Task.Delay(TimeSpan.FromSeconds(3));
}

await Task.Delay(TimeSpan.FromSeconds(10));