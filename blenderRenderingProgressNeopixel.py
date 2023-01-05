import bpy
import socket
import time
from threading import Thread


HOST = "192.168.1.157"
PORT = 2688

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
    

s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
s.connect((HOST, PORT))

serversion = s.recv(2)
print("Server is running on version " + str(serversion[0]) + "." + str(serversion[1]))
    
pixel_count = int.from_bytes(s.recv(4), "little")
print("Server is running with " + str(pixel_count) + " pixels")

Thread(target=recvThread, args=(s,)).start()



scene = bpy.data.scenes[0]


isRendering = False

def set_render_state_true(scene):
    global isRendering
    isRendering = True


def set_render_state_false(scene):
    global isRendering
    isRendering = False

class ModalTimerOperator(bpy.types.Operator):
    """Operator which runs itself from a timer"""
    bl_idname = "wm.modal_timer_operator"
    bl_label = "Modal Timer Operator"

    _timer = None
    
    lastFrame = 0

    def modal(self, context, event):
        if event.type in {'RIGHTMOUSE', 'ESC'}:
            self.cancel(context)
            return {'CANCELLED'}

        if event.type == 'TIMER':
            # change theme color, silly!
            print(isRendering)
            if self.lastFrame != scene.frame_current and isRendering:
                self.lastFrame = scene.frame_current
                length = scene.frame_end - scene.frame_start
                pixelsPerFrame = pixel_count / length
                print("Pixels per frame", pixelsPerFrame)
                print("Now setting a pixel..." + str(int(pixelsPerFrame * scene.frame_current)))
                
                startTransaction(s)
                clearStripe(s)
                for i in range(int(pixelsPerFrame * (scene.frame_current - scene.frame_start))):
                    setPixel(s, i, 0, 255, 0)
                stopTransaction(s)
        
        return {'PASS_THROUGH'}

    def execute(self, context):
        wm = context.window_manager
        self._timer = wm.event_timer_add(0.3, window=context.window)
        wm.modal_handler_add(self)
        return {'RUNNING_MODAL'}

    def cancel(self, context):
        wm = context.window_manager
        wm.event_timer_remove(self._timer)


def menu_func(self, context):
    self.layout.operator(ModalTimerOperator.bl_idname, text=ModalTimerOperator.bl_label)




def register():
    bpy.utils.register_class(ModalTimerOperator)
    bpy.types.VIEW3D_MT_view.append(menu_func)
    
    bpy.app.handlers.render_init.append(set_render_state_true)
    bpy.app.handlers.render_cancel.append(set_render_state_false)
    bpy.app.handlers.render_complete.append(set_render_state_false)


# Register and add to the "view" menu (required to also use F3 search "Modal Timer Operator" for quick access).
def unregister():
    bpy.utils.unregister_class(ModalTimerOperator)
    bpy.types.VIEW3D_MT_view.remove(menu_func)


if __name__ == "__main__":
    register()

    # test call
    bpy.ops.wm.modal_timer_operator()


