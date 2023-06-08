import bpy
import socket
import time
from threading import Thread
from bpy.types import Operator, AddonPreferences
from bpy.props import StringProperty, IntProperty, BoolProperty
from bpy.app.handlers import persistent


bl_info = {
    "name": "Neopixel Render Progress",
    "blender": (3, 3, 0),
    "category": "System",
    "description": "This addon allows you to connect your blender instance to a neopixel server (https://github.com/zenonet/NeopixelTcp) and display rendering progress on the stripe",
}

class NeopixelPreferences(AddonPreferences):
    # this must match the add-on name, use '__package__'
    # when defining this in a submodule of a python package.
    bl_idname = __name__

    ipAddress: StringProperty(
        name="IP Address",
        description="The IP Address of you neopixel server",
        default="",
    )

    enabled: BoolProperty(
        name="Enabled",
        description="Enable or disable the neopixel render progress",
        default=True,
    )

    def draw(self, context):
        layout = self.layout
        layout.prop(self, "ipAddress")
        layout.prop(self, "enabled")


def recvThread(sock: socket.socket):
    t = time.time()
    #sock.setblocking(0)
    while True:
        if(time.time() - t > 4):
            # Send a keep alive packet
            sock.sendall(bytearray())
            sock.sendall(bytearray([1, 5]))
            t = time.time()

        try:
            op = int.from_bytes(sock.recv(1), "little")

            #if op == 0:
                #print("Server is disconnecting")
                #quit()
            if op == 1:
                # Implement pixel updates from the server
                pass
        except:
            pass

def setPixel(sock, pixel, r, g, b):
    buffer = bytearray()

    # Add the packet length
    buffer += b'\x08'

    # Add the op code
    buffer += b'\x02'

    # Add the pixel number
    buffer += pixel.to_bytes(4, "little")

    # Add the color
    buffer += r.to_bytes(1, "little")
    buffer += g.to_bytes(1, "little")
    buffer += b.to_bytes(1, "little")

    sock.sendall(buffer)
    
def clearStripe(sock):
    buffer = bytearray()

    # Add the packet length
    buffer += b'\x04'

    # Add the op code
    buffer += b'\x03'

    # Add the color
    buffer += (0).to_bytes(1, "little")
    buffer += (0).to_bytes(1, "little")
    buffer += (0).to_bytes(1, "little")

    sock.sendall(buffer)
    
def startTransaction(sock):
    buffer = bytearray()

    # Add the packet length
    buffer += b'\x01'

    # Add the op code
    buffer += b'\xFE'

    sock.sendall(buffer)
    
def stopTransaction(sock):
    buffer = bytearray()

    # Add the packet length
    buffer += b'\x01'

    # Add the op code
    buffer += b'\xFF'

    sock.sendall(buffer)


isRendering = False
s: socket.socket = None
preferences: bpy.types.Preferences = None

@persistent
def startNeopixel():
    global s
    global pixel_count
    global preferences
    s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    s.connect((bpy.context.preferences.addons[__name__].preferences.ipAddress, PORT))

    serversion = s.recv(2)
    print("Server is running on version " + str(serversion[0]) + "." + str(serversion[1]))
        
    pixel_count = int.from_bytes(s.recv(4), "little")
    print("Server is running with " + str(pixel_count) + " pixels")

    Thread(target=recvThread, args=(s)).start()

@persistent
def updateNeopixel(scene):
    global s
    global isRendering
    global pixel_count
    global preferences

    print(isRendering)
    print("Enabled: ", bpy.context.preferences.addons[__name__].preferences.enabled)
    if bpy.context.preferences.addons[__name__].preferences.enabled:
        if s is None:
            startNeopixel()

        length = scene.frame_end - scene.frame_start
        pixelsPerFrame = pixel_count / length
        print("Pixels per frame", pixelsPerFrame)
        print("Now setting a pixel..." + str(int(pixelsPerFrame * scene.frame_current)))
                
        startTransaction(s)
        clearStripe(s)
        for i in range(int(pixelsPerFrame * (scene.frame_current - scene.frame_start))):
            setPixel(s, i, 0, 255, 0)
        stopTransaction(s)

def register():
    bpy.utils.register_class(NeopixelPreferences)

    bpy.app.handlers.render_post.append(updateNeopixel)

def unregister():
    bpy.utils.unregister_class(NeopixelPreferences)

    bpy.app.handlers.render_post.remove(updateNeopixel)
