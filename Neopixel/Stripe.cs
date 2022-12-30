using System.Collections;

namespace Neopixel.Client;

public class Stripe : IEnumerable<Pixel>
{
    private readonly Pixel[] pixels;

    /// <summary>
    /// This event is raised when the pixel collection changes.
    /// </summary>
    public Action<int> OnChanged { get; set; }
    
    /// <summary>
    /// Gets the number of pixels in the stripe.
    /// </summary>
    public int PixelCount => pixels.Length;
    
    internal Stripe(int pixelCount)
    {
        pixels = new Pixel[pixelCount];
    }

    public Pixel this[int index]
    {
        get => this.pixels[index];
        set
        {
            // Update the pixels stripe reference.
            value.Stripe = this;
            
            this.pixels[index] = value;
            OnChanged.Invoke(index);
        }
    }

    public IEnumerator<Pixel> GetEnumerator()
    {
        return (IEnumerator<Pixel>) this.pixels.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
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