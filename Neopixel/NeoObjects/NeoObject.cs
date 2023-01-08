using System.Drawing;

namespace Neopixel.Client.NeopixelObjects;

/// <summary>
/// Represents an object that can be drawn on a neopixel stripe.
/// You can either use it as a base class for your own objects or create and modify an instance of the base class.
/// </summary>
public class NeoObject
{
    /// <summary>
    /// The position of the object measured from the start of the stripe to the pivot point
    /// </summary>
    public int Position { get; set; }
    /// <summary>
    /// The size in pixels of the object
    /// </summary>
    public int Size { get; set; }
    /// <summary>
    /// The color of the object
    /// </summary>
    public Color Color { get; set; }
    /// <summary>
    /// The pivot of the object. This will affect from which part of the object the position is calculated.
    /// Note that changing this value will not change the position of the object but the visual position of it.
    /// </summary>
    public Pivot Pivot { get; set; }

    /// <summary>
    /// The NeoScene this object is part of.
    /// </summary>
    public NeoScene Scene { get; set; }

    public Stripe Stripe => Scene.Stripe;

    public NeopixelClient Client => Scene.Client;

    public event Action<Stripe, NeoObject>? OnRenderEvent;

    #region Pivot Transformed Position Properties

    public int CenterAlignedPosition
    {
        get => Position - (sbyte) Pivot;
        set => Position = value + (sbyte) Pivot;
    }

    public int LeftAlignedPosition
    {
        get => CenterAlignedPosition - Size / 2;
        set => CenterAlignedPosition = value + Size / 2;
    }

    public int RightAlignedPosition
    {
        get => CenterAlignedPosition + Size / 2;
        set => CenterAlignedPosition = value - Size / 2;
    }

    #endregion

    /// <summary>
    /// Renders the object to the stripe
    /// </summary>
    public void Render()
    {
        OnRenderEvent?.Invoke(Stripe, this);
        OnRender();
        switch (Pivot)
        {
            case Pivot.Left:
                for (int i = Position; i < Position + Size; i++)
                {
                    if (i >= Stripe.PixelCount)
                        break;

                    Stripe[i] = Color;
                }

                break;
            case Pivot.Right:
                for (int i = Position; i > Position - Size; i--)
                {
                    if (i <= 0)
                        break;

                    Stripe[i] = Color;
                }

                break;
            case Pivot.Center:
                for (int i = Position - Size / 2; i < Position + Size / 2; i++)
                {
                    if (i >= Stripe.PixelCount || i <= 0)
                        break;
                    Stripe[i] = Color;
                }

                break;
        }
    }

    public NeoObject(int position, int size, Color color, Pivot pivot = Pivot.Left, Action<Stripe, NeoObject>? onRender = null)
    {
        Position = position;
        Size = size;
        Color = color;
        Pivot = pivot;
        OnRenderEvent = onRender;
    }

    public NeoObject()
    {
    }

    /// <summary>
    /// This callback is called before the scene is rendered.
    /// </summary>
    /// <remarks>There is no need to call base.OnRender()</remarks>
    public virtual void OnRender()
    {
    }

    /// <summary>
    /// This callback is called when the object is added to a scene.
    /// </summary>
    /// <remarks>There is no need to call base.OnInitialize()</remarks>
    protected internal virtual void OnInitialize()
    {
    }

    #region Helper Methods

    /// <summary>
    /// Returns whether the given object overlaps with this object
    /// </summary>
    public bool OverlapsWith(NeoObject other)
    {
        // Check if the two objects are overlapping
        return LeftAlignedPosition < other.RightAlignedPosition && RightAlignedPosition > other.LeftAlignedPosition;
    }

    /// <summary>
    /// Returns whether the given position is within the bounds of this object
    /// </summary>
    public bool OverlapsWith(int position)
    {
        // Check if the two objects are overlapping
        return LeftAlignedPosition < position && RightAlignedPosition > position;
    }

    /// <summary>
    /// Returns whether this object is overlapping with the bounds of the stripe
    /// </summary>
    /// <returns></returns>
    public bool OverlapsWithBounds()
    {
        // Check if this object is overlapping with the bounds of the scene
        return LeftAlignedPosition < 0 || RightAlignedPosition > Stripe.PixelCount - 1;
    }

    /// <summary>
    /// Return whether this object is next to, overlapping or outside of the bounds of the stripe
    /// </summary>
    /// <returns></returns>
    public bool TouchesBounds()
    {
        // Check if this object is touching the bounds of the scene or is outside of it
        return LeftAlignedPosition <= 0 || RightAlignedPosition >= Stripe.PixelCount - 1;
    }

    #endregion
}