using Azure.Core;
using Azure.Identity;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Extensions;
using System.Diagnostics;
using System.Text;


internal class Program
{
    DefaultAzureCredential defaultCredential = new();

    private static async Task Main(string[] args)
    {
        if (Environment.GetEnvironmentVariable("MQTT_LOG_ENABLED")! == "true")
            Trace.Listeners.Add(new ConsoleTraceListener());
        
        await new Program().RunAsync();
    }

    public async Task RunAsync()
    {
        IMqttClient mqttClient = new MqttFactory().CreateMqttClient(MqttNetTraceLogger.CreateTraceLogger());
        MqttConnectionSettings cs = MqttConnectionSettings.CreateFromEnvVars();
        int timeoutMs = 100;

        mqttClient.DisconnectedAsync += MqttClient_DisconnectedAsync;

        async Task MqttClient_DisconnectedAsync(MqttClientDisconnectedEventArgs arg)
        {
            Console.WriteLine($"Client Disconnected: {arg.ClientWasConnected} with reason {arg.Reason}. Reconnecting in {timeoutMs} ms");
            await Task.Delay(timeoutMs);
            await mqttClient.ReconnectAsync();
            timeoutMs = timeoutMs * 2;
        }

        MqttClientConnectResult connAck = await mqttClient!.ConnectAsync(new MqttClientOptionsBuilder()
            .WithJWT(cs, GetToken, mqttClient, TimeSpan.FromHours(1))
            .Build());

        Console.WriteLine($"Client Connected: {mqttClient.IsConnected} with CONNACK: {connAck.ResultCode} with auth method {mqttClient.Options.AuthenticationMethod}");

        mqttClient.ApplicationMessageReceivedAsync += m => Console.Out.WriteLineAsync(
           $"Received message {m.PacketIdentifier} on topic: '{m.ApplicationMessage.Topic}' with content: {m.ApplicationMessage.ConvertPayloadToString()}");

        MqttClientSubscribeResult suback = await mqttClient.SubscribeAsync("sample/+", MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce);
        suback.Items.ToList().ForEach(s => Console.WriteLine($"subscribed to '{s.TopicFilter.Topic}'  with '{s.ResultCode}'"));

        int counter = 0;
        while (true)
        {
            Console.WriteLine($"->Sending Message {counter}");
            MqttClientPublishResult puback = await mqttClient.PublishStringAsync("sample/topic1", "hello world!" + counter++, MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce);
            await Console.Out.WriteLineAsync($" <- PubAck {puback.PacketIdentifier} {puback.ReasonString} {puback.ReasonCode}");
            await Task.Delay(10000);
        }
    }

    byte[] GetToken()
    {
        Console.WriteLine($"---- Get Token {DateTime.Now:o} ----");
        AccessToken jwt = defaultCredential.GetToken(new TokenRequestContext(["https://eventgrid.azure.net/.default"]));
        return Encoding.UTF8.GetBytes(jwt.Token);
    }
}