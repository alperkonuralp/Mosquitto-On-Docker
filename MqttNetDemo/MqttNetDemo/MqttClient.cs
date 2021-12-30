using MessagePack;
using MQTTnet;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;
using MQTTnet.Client.Receiving;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Formatter;
using MQTTnet.Protocol;
using System.Text;

namespace MqttNetDemo
{
    public class MqttClient
    {
        private static readonly MqttFactory mqttFactory = new MqttFactory();

        public string Server { get; set; }

        public int Port { get; set; }

        public string ClientId { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public IManagedMqttClient? Client { get; set; } = null;

        public event EventHandler<MqttClientConnectedEventArgs>? OnClientConnected;

        public event EventHandler<MqttClientDisconnectedEventArgs>? OnClientDisconnected;

        public event EventHandler<MqttApplicationMessageReceivedEventArgs>? OnApplicationMessageReceived;

        public MqttClient(string server, int port, string userName, string password, string? clientId = null)
        {
            Server = server;
            Port = port;
            UserName = userName;
            Password = password;
            ClientId = clientId ?? "Client_" + Guid.NewGuid();
        }

        public async Task ConnectAsync()
        {
            var tlsOptions = new MqttClientTlsOptions
            {
                UseTls = false,
                IgnoreCertificateChainErrors = true,
                IgnoreCertificateRevocationErrors = true,
                AllowUntrustedCertificates = true
            };

            var options = new MqttClientOptions
            {
                ClientId = ClientId,
                ProtocolVersion = MqttProtocolVersion.V311,
                ChannelOptions = new MqttClientTcpOptions
                {
                    Server = Server,
                    Port = Port,
                    TlsOptions = tlsOptions
                }
            };

            if (options.ChannelOptions == null)
            {
                throw new InvalidOperationException();
            }

            options.Credentials = new MqttClientCredentials
            {
                Username = UserName,
                Password = Encoding.UTF8.GetBytes(Password)
            };

            options.CleanSession = true;
            options.KeepAlivePeriod = TimeSpan.FromSeconds(5);
            Client = mqttFactory.CreateManagedMqttClient();

            Client.ConnectedHandler = new MqttClientConnectedHandlerDelegate(OnConnected);
            Client.DisconnectedHandler = new MqttClientDisconnectedHandlerDelegate(OnDisconnected);
            Client.ApplicationMessageReceivedHandler = new MqttApplicationMessageReceivedHandlerDelegate(HandleReceivedApplicationMessage);

            await Client.StartAsync(
                new ManagedMqttClientOptions
                {
                    ClientOptions = options
                });
        }

        public async Task PublishAsync<T>(string topic, T messageObject)
        {
            if (Client == null) throw new InvalidOperationException();
            if (messageObject == null) throw new ArgumentNullException(nameof(messageObject));

            string messageText;

            if (messageObject is string stringMessageObject)
            {
                messageText = stringMessageObject;
            }
            else
            {
                byte[] bytes = MessagePackSerializer.Typeless.Serialize(messageObject);
                messageText = MessagePackSerializer.ConvertToJson(bytes);
            }

            var payload = Encoding.UTF8.GetBytes(messageText);
            var message = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payload)
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                .WithRetainFlag()
                .Build();

            await Client.PublishAsync(message);
        }

        public void Close()
        {
            if (Client != null)
            {
                Client.Dispose();
                Client = null;
            }
        }

        private void OnConnected(MqttClientConnectedEventArgs args)
        {
            OnClientConnected?.Invoke(this, args);
        }

        private void OnDisconnected(MqttClientDisconnectedEventArgs args)
        {
            OnClientDisconnected?.Invoke(this, args);
        }

        private void HandleReceivedApplicationMessage(MqttApplicationMessageReceivedEventArgs args)
        {
            OnApplicationMessageReceived?.Invoke(this, args);
        }
    }
}