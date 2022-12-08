# The NeoServer Protocol

## Version 1.0

### Handshake

When connected using TCP, the server will send the following data:

1. His version code (2 bytes(1 byte for major version, 1 byte for minor version))
2. The number of pixels (4 byte integer)
