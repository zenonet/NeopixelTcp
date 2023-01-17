namespace Neopixel.Client;

public class Stripe
{
    private readonly Pixel[] pixels;

    public IEnumerable<Pixel> Pixels => pixels;

    /// <summary>
    /// This event is raised when the pixel collection is changed locally.
    /// </summary>
    public event StripeChangedEventHandler OnStripeChangedLocally;

    /// <summary>
    /// This event is raised when the pixel collection is changed remotely.
    /// </summary>
    public event StripeChangedEventHandler OnStripeChangedRemotely;

    /// <summary>
    /// This event is raised when the pixel collection is changed (Remotely or Locally).
    /// </summary>
    public StripeChangedEventHandler OnStripeChanged { get; set; }

    internal void RaiseOnStripeChangedLocally(int index)
    {
        OnStripeChangedLocally?.Invoke(this, index);
        OnStripeChanged?.Invoke(this, index);
    }

    internal void RaiseOnStripeChangedRemotely(int index)
    {
        OnStripeChangedRemotely?.Invoke(this, index);
        OnStripeChanged?.Invoke(this, index);
    }

    /// <summary>
    /// Gets the number of pixels in the stripe.
    /// </summary>
    public int PixelCount => pixels.Length;

    internal bool SuppressSync { get; set; }

    internal Stripe(int pixelCount)
    {
        pixels = new Pixel[pixelCount];

        for (int i = 0; i < pixelCount; i++)
        {
            pixels[i] = new(i, this);
        }
    }

    public Pixel this[int index]
    {
        get => this.pixels[index % PixelCount].Clone();
        set
        {
            // Update the pixels stripe reference.
            value.Stripe = this;

            // Update the pixel index property
            pixels[index % PixelCount].Index = index;

            SetPixel(index, value);

            RaiseOnStripeChangedLocally(index);
        }
    }

    private void SetPixel(int index, Pixel pixel)
    {
        pixels[index].R = pixel.R;
        pixels[index].G = pixel.G;
        pixels[index].B = pixel.B;
    }

    /// <summary>
    /// Returns the stripe as a Pixel array.
    /// </summary>
    /// <returns></returns>
    public Pixel[] ToArray()
    {
        return this.pixels;
    }
}

public delegate void StripeChangedEventHandler(Stripe sender, int index);