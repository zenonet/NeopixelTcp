using System.Collections;

namespace Neopixel.Client;

public class Stripe : IEnumerable<Pixel>
{
    private readonly Pixel[] pixels;
    
    public int LastHash;

    public event Action<int>? OnChanged;

    public Stripe(int pixelCount)
    {
        this.pixels = new Pixel[pixelCount];
    }

    public Pixel this[int index]
    {
        get
        {
            LastHash = pixels.GetHashCode();
            return this.pixels[index];
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
}