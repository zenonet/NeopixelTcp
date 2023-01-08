using System.Drawing;

namespace Neopixel.Client.NeopixelObjects;

/// <summary>
/// Represents a scene consisting of multiple objects that can be drawn to a neopixel stripe.
/// </summary>
public class NeoScene
{
    public NeoScene(NeopixelClient client)
    {
        Client = client;
        Stripe = client.Stripe;
    }

    /// <summary>
    /// A list of all objects in the scene
    /// </summary>
    public List<NeoObject> Objects { get; set; } = new();
    
    /// <summary>
    /// The stripe to render the scene to
    /// </summary>
    public Stripe Stripe { get; set; }
    
    public NeopixelClient Client { get; set; }

    /// <summary>
    /// Call this method to render the scene to the stripe
    /// </summary>
    public void Render()
    {
        Client.IsTransacting = true;
        Client.Fill(Color.Black);
        foreach (NeoObject o in Objects)
        {
            o.Render();
        }
        Client.IsTransacting = false;
    }

    /// <summary>
    /// Adds a neo object to the scene
    /// This will set the object's scene to this scene
    /// </summary>
    /// <param name="obj">The object to add</param>
    public void Add(NeoObject obj)
    {
        obj.Scene = this;
        Objects.Add(obj);
        obj.OnInitialize();
    }
}