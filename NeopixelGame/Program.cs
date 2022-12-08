using System.Drawing;
using Neopixel.Client;
using NeopixelGame;

Console.WriteLine("Connecting to server...");
NeopixelClient client = new ("192.168.1.157");
Console.WriteLine("Connected!");

client.OnDisconnection += () =>
{
    Console.WriteLine("Disconnected!");
    Environment.Exit(1);
};

client.Fill(Color.Blue);

int position = 0;
while (true)
{
    int lastPos = position;
    
    if (NativeKeyboard.IsKeyDown(KeyCode.Left))
    {
        position--;
    }

    if (NativeKeyboard.IsKeyDown(KeyCode.Right))
    {
        position++;
    }
    
    if (position < 0)
    {
        position = 82;
    }

    if (position > 82)
    {
        position = 0;
    }

    if (position != lastPos)
    {
        Console.WriteLine(position);

        // Set the position
        client.SetPixel(position, Color.Red);
        client.SetPixel(lastPos, Color.Black);

        await Task.Delay(50);
    }

    /*
    string line = Console.ReadLine()!;
    if(line == "q")
        break;

    int index = int.Parse(line[..^1]);
    
    Color color = line.Last() switch
    {
        'b' => Color.Blue,
        'c' => Color.Cyan,
        'r' => Color.Red,
        'g' => Color.Green,
        _ => Color.CornflowerBlue,
    };
    client.SetPixel(index, color);
*/
}