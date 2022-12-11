import time
import board
import neopixel
import socket
import select

num_pixels = 83

version: bytes = bytes(b'\x01\x00')

clear_on_start = True

TIMEOUT = 5

def getCommands(data: bytes) -> list[bytes]:
    commands: list[bytes] = []
    while len(data) > 0:
        while data[0] == 0x00:
            data = data[1:]

        # Get the command length and cut it off
        length = data[0]
        data = data[1:]

        if length > len(data):
            return commands

        # Get the command and cut it off
        commands.append(data[:length])
        data = data[length:]
    
    return commands



pixels: neopixel.NeoPixel = neopixel.NeoPixel(board.D18, num_pixels, auto_write=False)

# Create a TCP/IP socket
sock:socket.socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)


# Bind the socket to the port
server_address = ('0.0.0.0', 2688)

print('starting up on {} port {}'.format(*server_address))

sock.bind(server_address)

# Listen for incoming connections
sock.listen()

if clear_on_start:
    pixels.fill((0, 0, 0))

def handle_client(connection, client_address):
    # send version code
    connection.sendall(version)

    connection.sendall(num_pixels.to_bytes(4, 'big'))

    start_time = time.time()
    try:
        print('connection from', client_address)

        while True:

            if time.time() - start_time > TIMEOUT:
                print("Timeout")
                try:
                    # Send the disconnect command
                    connection.sendall(b'\x01\x04')
                except:
                    pass
                #close the connection
                connection.close()

                return
            # Check if the client still exists (as described here: https://stackoverflow.com/questions/17386487/python-detect-when-a-socket-disconnects-for-any-reason)
            try:
                ready_to_read, ready_to_write, in_error = \
                    select.select([connection,], [connection,], [], 5)
            except select.error:
                # connection error event here, maybe reconnect
                print('connection error')
                # Return to the outer loop (waiting for a new connection)
                break

            try:
                length: int = 0
                while length == 0 or length == b'':
                    length = connection.recv(1)[0]
                    if length == 0:
                        continue
                    print("length:", int(length))

                data = connection.recv(length)

                for i in data:
                    print(hex(i), end=" ")
            except:
                continue

            if not data or data == b'':
                continue

            length = data[0]

            print("Data length: " + str(len(data)))

            start_time = time.time()
            if data[0] == 0x01:
                # Clear the strip
                pixels.fill((0, 0, 0))
                pixels.show()
                data = data[1:]

            elif data[0] == 0x02:
                print("Setting pixel {} to {}".format(data[1], data[2:5]))
                # Set a single pixel
                if len(data) != 5:
                    continue
                
                # Length check
                if data[1] >= num_pixels:
                    continue
                
                pixels[data[1]] = (int(data[2]), int(data[3]), int(data[4]))
                pixels.show()
                # Slice the data

                print(len(data))
                data = data[5:]
                print(len(data))
                #input()


            elif data[0] == 0x03:
                # Fill the strip
                if len(data) != 4:
                    continue
                pixels.fill((int(data[1]), int(data[2]), int(data[3])))
                pixels.show()

                data = data[4:]
            elif data[0] == 0x04:
                print("Closed the connection to the current client")
                connection.close()
                break
            elif data[0] == 0x05:
                # Do nothing, just keep the connection alive
                data = data[1:]
    finally:
        # Clean up the connection
        connection.close()

while True:
    # Wait for a connection
    print('waiting for a connection...')

    connection:socket.socket
    connection, client_address = sock.accept()

    handle_client(connection, client_address)