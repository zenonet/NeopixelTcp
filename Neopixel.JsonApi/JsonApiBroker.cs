using System.Text.Json;
using System.Text.Json.Serialization;
using Neopixel.Client;

namespace Neopixel.JsonApi;

public class JsonApiBroker
{
    public JsonApiBroker(NeopixelClient client)
    {
        Client = client;
    }

    public JsonApiBroker(string host, int port = 2688)
    {
        Client = new(host, port);
    }

    public NeopixelClient Client { get; set; }

    public void RunFromJson(string json)
    {
        // Allow for comments and trailing commas in the JSON
        JsonSerializerOptions options = new()
        {
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
            PropertyNameCaseInsensitive = true,
        };
        
        NeopixelRequest? neopixelRequest = JsonSerializer.Deserialize<NeopixelRequest>(json);

        if (neopixelRequest is null)
        {
            throw new InvalidNeopixelRequestException("Invalid neopixel request!");
        }

        neopixelRequest.Execute(Client);
    }
}

public class PreprocessedNeopixelRequest
{
    public bool Transact { get; set; }
}

public class NeopixelRequest
{
    public bool Transact { get; set; }

    public NeopixelCommand[] Commands { get; set; }

    public void Execute(NeopixelClient client)
    {
        client.IsTransacting = Transact;

        foreach (var command in Commands)
        {
            command.Execute(client);
        }

        client.IsTransacting = false;
    }
}

[JsonDerivedType(typeof(FillCommand), typeDiscriminator:"Fill")]
[JsonDerivedType(typeof(SetPixelCommand), typeDiscriminator:"SetPixel")]
[JsonDerivedType(typeof(SetPixelRangeCommand), typeDiscriminator:"SetRange")]
[JsonDerivedType(typeof(WaitCommand), typeDiscriminator:"Wait")]
public abstract class NeopixelCommand
{
    public abstract void Execute(NeopixelClient client);
}

public class FillCommand : NeopixelCommand
{
    public Color Color { get; set; }

    public override void Execute(NeopixelClient client)
    {
        client.Fill(Color);
    }
}

public class SetPixelCommand : NeopixelCommand
{
    public int Index { get; set; }
    public Color Color { get; set; }

    public override void Execute(NeopixelClient client)
    {
        client.SetPixel(Index, Color);
    }
}

public class SetPixelRangeCommand : NeopixelCommand
{
    public int StartIndex { get; set; }
    public int EndIndex { get; set; }
    public Color Color { get; set; }

    public override void Execute(NeopixelClient client)
    {
        client.IsTransacting = true;

        for (int i = StartIndex; i <= EndIndex; i++)
        {
            client.SetPixel(i, Color);
        }

        client.IsTransacting = false;
    }
}

public class WaitCommand : NeopixelCommand
{
    public int Milliseconds { get; set; }

    public override void Execute(NeopixelClient client)
    {
        Task.Delay(Milliseconds);
    }
}

public class InvalidNeopixelRequestException : Exception
{
    public InvalidNeopixelRequestException(string message) : base(message)
    {
    }
}

public class Color
{
    public Color(int r, int g, int b)
    {
        R = r;
        G = g;
        B = b;
    }

    public int R { get; set; }
    public int G { get; set; }
    public int B { get; set; }

    public static implicit operator System.Drawing.Color(Color color) => System.Drawing.Color.FromArgb(color.R, color.G, color.B);
}