# The NeoServer Protocol

## Version 1.0

### Handshake

When connected using TCP, the server will send the following data:

1. Its version code (2 bytes(1 byte for major version, 1 byte for minor version))
2. The number of pixels (4 byte integer)

### Keep Alive

The client needs to send a keep alive message every 5 seconds. If the server doesn't receive a keep alive message regularly (The exact timeout is part of the server config).<br>
The keep alive message is a 2 byte message with the value 0x01 0x05.

### Set Pixel

The client can set a pixel by sending a 9 byte message with the following format:

1. The length of the following data (1 byte, usually 0x08)
2. The opcode (1 byte, for setting a pixel the opcode is 0x02)
3. The pixel index (4 byte integer)
4. The 3 color values (rgb, 3 bytes)