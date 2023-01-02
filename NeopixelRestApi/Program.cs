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

        client.IsTransacting = true;
        client.Fill(Color.FromArgb(255, r, g, b));
        client.IsTransacting = false;
        
        client.Dispose();
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

        client.IsTransacting = true;
        for (int i = 50; i < 74; i++)
        {
            stripe[i] = pixel;
        }
        client.IsTransacting = false;

        client.Dispose();
    }
    catch (Exception e)
    {
        return "{\n\"status\":\"error\",\n\"message\": \"" + e.Message + "\"\n}";
    }

    return "{\n\"status\":\"ok\"\n}";
}).WithDescription("Fills the pixels above the door with the given color").WithOpenApi();

app.Run();

NeopixelClient GetNeopixelClient()
{
    return new("192.168.1.157");
}