using System.Collections;

namespace Neopixel.Client;

public class Stripe
{
    private readonly Pixel[] pixels;

    public IEnumerable<Pixel> Pixels => pixels;

    /// <summary>
    /// This event is raised when the pixel collection changes.
    /// </summary>
    public Action<int> OnChanged { get; set; }
    
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
        get => this.pixels[index].Clone();
        set
        {
            // Update the pixels stripe reference.
            value.Stripe = this;
            
            // Update the pixel index property
            pixels[index].Index = index;
            
            SetPixel(index, value);

            OnChanged.Invoke(index);
            
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