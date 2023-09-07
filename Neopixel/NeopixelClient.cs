using System.Diagnostics;
using System.Drawing;
using System.Net;
using System.Net.Sockets;

namespace Neopixel.Client;

public class NeopixelClient : IDisposable
{
    public static readonly (byte, byte) Version = (1, 0);

    public TcpClient TcpClient { get; private set; }

    public ClientState State { get; private set; }

    public event Action? OnDisconnection;

    public bool UpdateManually { get; set; }

    /// <summary>
    /// A representation of the stripe this client is in control of
    /// </summary>
    public Stripe Stripe { get; }


    private bool isTransacting;

    /// <summary>
    /// This property indicates if the client is currently transacting with the server
    /// You can start/stop a transaction by setting this property
    /// Learn more about transactions here: 
    /// </summary>
    public bool IsTransacting
    {
        get => isTransacting;
        set
        {
            if (IsTransacting == value)
                return;

            isTransacting = value;

            SendTransactionPacket(value);
        }
    }

    private void SendTransactionPacket(bool isTransacting)
    {
        NetworkStream stream = TcpClient.GetStream();

        byte[] buffer = new byte[2];

        // Add the size of the packet
        buffer[0] = 1;

        // Add the transaction state
        buffer[1] = (byte) (isTransacting ? 0xFE : 0xFF);

        // Send the packet
        stream.Write(buffer, 0, buffer.Length);
    }

    public NeopixelClient(string ip, int port = 2688, bool updateManually = false)
    {
        UpdateManually = updateManually;

        State = ClientState.Connecting;
        TcpClient = new();

        TcpClient.Connect(new(IPAddress.Parse(ip), port));

        #region Get Data from Server

        Stopwatch timeout = Stopwatch.StartNew();
        while (true)
        {
            if (timeout.ElapsedMilliseconds > 600)
            {
                throw new("Connection timed out");
            }

            if (TcpClient.Available != 6)
                continue;

            byte[] buffer = new byte[6];
            _ = TcpClient.GetStream().Read(buffer, 0, 6);

            // Check if the server has sent the correct version code
            if (buffer[0] != Version.Item1 || buffer[1] != Version.Item2)
                throw new($"Server version is not compatible (Server version: {buffer[0]}.{buffer[1]})");

            // Get the pixel count from the network stream
            int pixelCount = BitConverter.ToInt32(buffer, 2);

            // Initialize the stripe
            Stripe = new(pixelCount);

            State = ClientState.Connected;
            break;
        }

        #endregion

        State = ClientState.Connected;
        StartBackgroundWorker();

        Stripe.OnStripeChangedLocally += SyncChanges;
    }

    private void SyncChanges(Stripe _, int index)
    {
        if (Stripe.SuppressSync)
            return;

        SetPixel(index, Stripe[index]);
    }

    #region SetPixel and SetPixelAsync

    /// <summary>
    /// Manually sends a pixel change packet to the server
    /// </summary>
    /// <param name="index">The LED's index</param>
    /// <param name="color">The color to set</param>
    /// <remarks>Calling this method does not update the Stripe Property essentially desyncing client and server</remarks>
    public void SetPixel(int index, Color color)
    {
        SetPixel(index, color.R, color.G, color.B);
    }

    /// <summary>
    /// Manually sends a pixel change packet to the server
    /// </summary>
    /// <param name="index">The LED's index</param>
    /// <param name="r">The red component of the color</param>
    /// <param name="g">The green component of the color</param>
    /// <param name="b">The blue component of the color</param>
    /// <remarks>Calling this method does not update the Stripe Property essentially desyncing client and server</remarks>
    public void SetPixel(int index, byte r, byte g, byte b)
    {
        SetPixelAsync(index, r, g, b).Wait();
    }

    /// <summary>
    /// Manually sends a pixel change packet to the server ansynchronously
    /// </summary>
    /// <param name="index">The LED's index</param>
    /// <param name="color">The color to set</param>
    /// <remarks>Calling this method does not update the Stripe Property essentially desyncing client and server</remarks>
    public async Task SetPixelAsync(int index, Color color)
    {
        await SetPixelAsync(index, color.R, color.G, color.B);
    }

    /// <summary>
    /// Manually sends a pixel change packet to the server asynchronously
    /// </summary>
    /// <param name="index">The LED's index</param>
    /// <param name="r">The red component of the color</param>
    /// <param name="g">The green component of the color</param>
    /// <param name="b">The blue component of the color</param>
    /// <remarks>Calling this method does not update the Stripe Property essentially desyncing client and server</remarks>
    public async Task SetPixelAsync(int index, byte r, byte g, byte b)
    {
        byte[] buffer = new byte[9];
        // Command length
        buffer[0] = 8;

        // Opcode
        buffer[1] = 0x02;

        // Get the index as as byte array
        byte[] indexAsBytes = BitConverter.GetBytes(index);

        // Copy the index to the buffer
        Array.Copy(indexAsBytes, 0, buffer, 2, 4);

        // Color values
        buffer[6] = r;
        buffer[7] = g;
        buffer[8] = b;
        await TcpClient.GetStream().WriteAsync(buffer, 0, buffer.Length);
    }

    #endregion

    #region Fill and FillAsync

    /// <summary>
    /// Fills the whole stripe with the specified color
    /// </summary>
    public void Fill(Color color)
    {
        Fill(color.R, color.G, color.B);
    }

    /// <summary>
    /// Fills the whole stripe with the specified color
    /// </summary>
    private void Fill(byte r, byte g, byte b)
    {
        FillAsync(r, g, b).Wait();
    }

    /// <summary>
    /// Fills the whole stripe with the specified color asynchronously
    /// </summary>
    public async Task FillAsync(Color color)
    {
        await FillAsync(color.R, color.G, color.B);
    }

    /// <summary>
    /// Fills the whole stripe with the specified color asynchronously
    /// </summary>
    private async Task FillAsync(byte r, byte g, byte b)
    {
        byte[] buffer = new byte[6];
        // Command length
        buffer[0] = 4;

        // Opcode
        buffer[1] = 0x03;

        // Color values
        buffer[2] = r;
        buffer[3] = g;
        buffer[4] = b;
        await TcpClient.GetStream().WriteAsync(buffer, 0, buffer.Length);

        // Update the stripe
        Stripe.SuppressSync = true;
        for (int x = 0; x < Stripe.PixelCount; x++)
        {
            Stripe[x] = new(r, g, b);
        }
        Stripe.SuppressSync = false;
    }

    #endregion

    private Task StartBackgroundWorker()
    {
        return Task.Run(() =>
        {
            Stopwatch sw = Stopwatch.StartNew();
            while (State == ClientState.Connected)
            {
                // Check if the server has sent a command
                if (TcpClient.Available > 0)
                {
                    byte op = (byte) TcpClient.GetStream().ReadByte();


                    // Check for the disconnection packet
                    if (op == 0x01)
                    {
                        State = ClientState.NotConnected;
                        TcpClient.Close();
                        OnDisconnection?.Invoke();
                    }

                    // Check if the stripe has been changed
                    if (op == 0x02)
                    {
                        byte[] buffer = new byte[7];
                        _ = TcpClient.GetStream().Read(buffer, 0, 7);

                        int index = BitConverter.ToInt32(buffer, 0);
                        byte r = buffer[4];
                        byte g = buffer[5];
                        byte b = buffer[6];

                        Stripe.SuppressSync = true;
                        Stripe[index] = new(r, g, b);
                        Stripe.SuppressSync = false;

                        Stripe.RaiseOnStripeChangedRemotely(index);
                    }
                }
                // Timeout
                if (sw.Elapsed.Milliseconds > 4000)
                {
                    sw.Restart();
                    // Send a disconnection packet
                    TcpClient.GetStream().Write(new byte[] {0x01, 0x05}, 0, 2);
                }
            }
        });
    }

    public void Dispose()
    {
        State = ClientState.Disposing;

        OnDisconnection?.Invoke();

        //Tell the server close the connection

        // Write the lenght of the operation
        TcpClient.GetStream().WriteByte(0x01);

        // Write the opt code
        TcpClient.GetStream().WriteByte(0x04);
        Console.WriteLine("Close connection");
        TcpClient.Dispose();
    }

    ~NeopixelClient() => Dispose();
}