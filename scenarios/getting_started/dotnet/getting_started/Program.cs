
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Extensions;

//System.Diagnostics.Trace.Listeners.Add(new System.Diagnostics.ConsoleTraceListener());

MqttConnectionSettings cs = MqttConnectionSettings.CreateFromEnvVars();
Console.WriteLine($"Connecting to {cs}");

IMqttClient mqttClient = new MqttFactory().CreateMqttClient(MqttNetTraceLogger.CreateTraceLogger());

int delay = 100;
mqttClient.DisconnectedAsync += MqttClient_DisconnectedAsync;
mqttClient.ConnectingAsync += MqttClient_ConnectingAsync;
mqttClient.ConnectedAsync += MqttClient_ConnectedAsync;

async Task MqttClient_ConnectedAsync(MqttClientConnectedEventArgs arg)
{
    MqttClientSubscribeResult suback = await mqttClient.SubscribeAsync("sample/+", MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce);
    suback.Items.ToList().ForEach(s => Console.WriteLine($"subscribed to '{s.TopicFilter.Topic}'  with '{s.ResultCode}'"));
}

async Task MqttClient_ConnectingAsync(MqttClientConnectingEventArgs arg)
{
    Console.WriteLine($"Connecting client {arg.ClientOptions.ClientId}");
    await Task.Delay(delay);
    delay *= 2;
}

async Task MqttClient_DisconnectedAsync(MqttClientDisconnectedEventArgs arg)
{
    Console.WriteLine($"ClientDisconnected {arg.Reason}, waiting {delay}.");
    await Task.Delay(delay);
    await mqttClient.ReconnectAsync();
    delay *= 2;
}

MqttClientConnectResult connAck = await mqttClient!.ConnectAsync(new MqttClientOptionsBuilder().WithConnectionSettings(cs).Build());
Console.WriteLine($"Client Connected: {mqttClient.IsConnected} with CONNACK: {connAck.ResultCode}");

mqttClient.ApplicationMessageReceivedAsync += async m => await Console.Out.WriteAsync(
    $"Received message on topic: '{m.ApplicationMessage.Topic}' with content: '{m.ApplicationMessage.ConvertPayloadToString()}'\n\n");



while (true)
{
    await Task.Delay(1000);
    try
    { 
        MqttClientPublishResult puback = await mqttClient.PublishStringAsync("sample/topic1", "hello world!", MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce);
        Console.WriteLine(puback.ReasonCode);
    }
    catch(Exception ex)
    {
        Console.WriteLine("missing one message" + ex.Message);
    }
}

//Console.ReadLine();

