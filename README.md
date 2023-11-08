# NeopixelTcp

Neopixel TCP is a suite of software for making addressable RGB-Stripes show any data you want. At it's base Neopixel TCP is just a Software that allows clients
to take control over a Neopixel Stripe via TCP. This means you can easily control your Neopixel-Stripe from anywhere in your Wifi or LAN.

## Libraries

There are a few client libraries for different programming languages that allow you to control the neopixel server easily. The main library is written in C# however
there also a simple one for Java.

### The C# Library

The C# Library is the main client library. I is based purely on the TcpClient class from the System.Net namespace. It works on all dotnet supported platforms. It is also currently the only client library implementing the complete feature set.

## Implementations

Neopixel TCP is made to be implemented into as much software as possible. For example there already are implementations for:

Games:
- Potion Craft
- Minecraft Java Edition (Currently only shows health)

Programs:
- Blender
