﻿# The NeoServer Protocol

## Version 1.0

### Handshake

When connected using TCP, the server will send the following data:

1. Its version code (2 bytes(1 byte for major version, 1 byte for minor version))
2. The number of pixels (4 byte integer)

### Keep Alive

The client needs to send a keep alive message every 5 seconds. If the server doesn't receive a keep alive message regularly (The exact timeout is part of the server config).<br>
The keep alive message is a 2 byte message with the value 0x01 0x05.

### Clear

The client can clear the pixels by sending a 2 byte message with the value 0x01 0x01.

This is more or less useless because it is just 3 bytes shorter than sending a fill command but it is there for backwards compatibility.

### Fill

The client can fill the whole strip with a color by sending a 6 byte message with the following format:

1. The length of the following data (1 byte, usually 0x05)
2. The opcode (1 byte, for filling the strip the opcode is 0x03)
3. The 3 color values (rgb, 3 bytes)


### Set Pixel

The client can set a pixel by sending a 9 byte message with the following format:

1. The length of the following data (1 byte, usually 0x08)
2. The opcode (1 byte, for setting a pixel the opcode is 0x02)
3. The pixel index (4 byte integer)
4. The 3 color values (rgb, 3 bytes)

### Transactions

Transactions can be used to ensure that multiple pixels are set at the same time. This is useful for animations. The client can start a transaction by sending a 2 byte message with the following format:

1. The length of the following data (1 byte, usually 0x01)
2. The opcode (1 byte, for starting a transaction the opcode is 0xFE)

The client can end a transaction by sending a 2 byte message with the following format:

1. The length of the following data (1 byte, usually 0x01)
2. The opcode (1 byte, for ending a transaction the opcode is 0xFF)

When a transaction is ended the changes are applied to the pixels.

### Disconnect

The client can disconnect by sending a 2 byte message with the value 0x01 0x04. This isn't strictly
necessary because the server will disconnect the client after a timeout but it's a little cleaner.