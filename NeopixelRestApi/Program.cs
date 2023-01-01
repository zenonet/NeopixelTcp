using System.Drawing;
using Neopixel.Client;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

string MyHandler(int r, int g, int b)
{
    try
    {
        NeopixelClient client = GetNeopixelClient();

        client.Fill(Color.FromArgb(255, r, g, b));

        client.Dispose();
    }
    catch (NeopixelServerOccupiedException)
    {
        return "{\n\"status\":\"error\",\n\"message\": \"Server is already controlled by another client\"\n}";
    }
    catch (Exception e)
    {
        return "{\n\"status\":\"error\",\n\"message\": \"" + e.Message + "\"\n}";
    }

    return "{\n\"status\":\"ok\"\n}";
}

app.MapGet("/fill", MyHandler);

app.MapGet("/fillDoor", (int r, int g, int b) =>
{
    try
    {
        NeopixelClient client = GetNeopixelClient();
        Stripe stripe = client.Stripe;

        Pixel pixel = Color.FromArgb(255, r, g, b);

        for (int i = 50; i < 74; i++)
        {
            stripe[i] = pixel;
        }

        client.Dispose();
    }
    catch (NeopixelServerOccupiedException)
    {
        return "{\n\"status\":\"error\",\n\"message\": \"Server is already controlled by another client\"\n}";
    }
    catch (Exception e)
    {
        return "{\n\"status\":\"error\",\n\"message\": \"" + e.Message + "\"\n}";
    }

    return "{\n\"status\":\"ok\"\n}";
}).WithDescription("Fills the pixels above the door with the given color");

app.Run();

NeopixelClient GetNeopixelClient()
{
    return new("192.168.1.157");
}