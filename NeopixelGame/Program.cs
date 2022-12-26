using System.Drawing;
using Neopixel.Client;
using NeopixelGame;

Console.WriteLine("Connecting to server...");
NeopixelClient client = new ("<IpAddressOfYourServerHere>");
Console.WriteLine("Connected!");

client.OnDisconnection += () =>
{
    Console.WriteLine("Disconnected!");
    Environment.Exit(1);
};

client.Fill(Color.Blue);


Stripe stripe = client.Stripe;

await KeyboardMovement(stripe);

async Task KeyboardMovement(Stripe stripe1)
{
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
            stripe1[position] = Color.Red;
            stripe1[lastPos] = Color.Black;

            await Task.Delay(50);
        }
    }
}