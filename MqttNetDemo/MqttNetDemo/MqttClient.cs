using MessagePack;
using MQTTnet;
using MQTTnet.Client.Options;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Formatter;
using MQTTnet.Protocol;
using System.Text;

namespace MqttNetDemo
{
    public static class MqttClient
    {
        public static async Task<IManagedMqttClient> Connect()
        {
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

            return managedMqttClientPublisher;
        }

        public static async Task Publish(IManagedMqttClient managedMqttClientPublisher, string topic, string messageText)
        {
            var payload = Encoding.UTF8.GetBytes(messageText);
            var message = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payload)
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                .WithRetainFlag()
                .Build();

            await managedMqttClientPublisher.PublishAsync(message);
        }

        public static async Task Publish<T>(IManagedMqttClient managedMqttClientPublisher, string topic, T messageObject)
        {
            byte[] bytes = MessagePackSerializer.Typeless.Serialize(messageObject);
            string messageText = MessagePackSerializer.ConvertToJson(bytes);

            var payload = Encoding.UTF8.GetBytes(messageText);
            var message = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payload)
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                .WithRetainFlag()
                .Build();

            await managedMqttClientPublisher.PublishAsync(message);
        }
    }
}