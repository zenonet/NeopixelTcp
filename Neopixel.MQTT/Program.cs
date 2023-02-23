using MQTTnet.Server;
using MQTTnet;
using System.Text;
using System.Text.Json;
using Neopixel.JsonApi;
using static System.Console;


var req = new NeopixelRequest
{
    Commands = new NeopixelCommand[]
    {
        new FillCommand
        {
            Color = new Color(255, 255, 255)
        },
    }
};

Console.WriteLine(JsonSerializer.Serialize(req));


// Create the options for MQTT Broker
MqttServerOptionsBuilder? options = new MqttServerOptionsBuilder().WithDefaultEndpointPort(1883).WithDefaultEndpoint();
// Create a new mqtt server
MqttServer? server = new MqttFactory().CreateMqttServer(options.Build());

JsonApiBroker broker = null;

//Add Interceptor for logging incoming messages
server.InterceptingPublishAsync += Server_InterceptingPublishAsync;

// Start the server
await server.StartAsync();
// Keep application running until user press a key
while (true)
{
    
}

async Task Server_InterceptingPublishAsync(InterceptingPublishEventArgs arg)
{
    // Late initialization of the broker
    if (broker == null)
    {
        broker = new("192.168.1.157");
    }
    
    // Ensure that the server doesn't stop when an exception occurs
    try
    {
        // Convert Payload to string
        string? payload = arg.ApplicationMessage?.Payload == null ? null : Encoding.UTF8.GetString(arg.ApplicationMessage?.Payload);

        if (payload == null)
            return;

        broker.RunFromJson(payload);

    }catch(Exception ex)
    {
        WriteLine(ex.Message);
    }
}