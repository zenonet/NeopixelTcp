using System;
using System.Drawing;
using MQTTnet.Server;
using MQTTnet;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Neopixel.JsonApi;
using static System.Console;



// Create the options for MQTT Broker
MqttServerOptionsBuilder? options = new MqttServerOptionsBuilder().WithDefaultEndpointPort(1883).WithDefaultEndpoint();
// Create a new mqtt server
MqttServer? server = new MqttFactory().CreateMqttServer(options.Build());

JsonApiBroker broker = new ("192.168.1.157");

//Add Interceptor for logging incoming messages
server.InterceptingPublishAsync += Server_InterceptingPublishAsync;


// Start the server
await server.StartAsync();
// Keep application running until user press a key
ReadLine();


async Task Server_InterceptingPublishAsync(InterceptingPublishEventArgs arg)
{
    // Convert Payload to string
    string? payload = arg.ApplicationMessage?.Payload == null ? null : Encoding.UTF8.GetString(arg.ApplicationMessage?.Payload);

    if(payload == null)
        return;
    
    broker.RunFromJson(payload);

    WriteLine(
        " TimeStamp: {0} -- Message: ClientId = {1}, Topic = {2}, Payload = {3}, QoS = {4}, Retain-Flag = {5}",

        DateTime.Now,
        arg.ClientId,
        arg.ApplicationMessage?.Topic,
        payload,
        arg.ApplicationMessage?.QualityOfServiceLevel,
        arg.ApplicationMessage?.Retain);
}