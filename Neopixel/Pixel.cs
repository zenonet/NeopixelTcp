﻿using System.Drawing;

namespace Neopixel.Client;

public class Pixel
{
    private byte r;
    private byte g;
    private byte b;

    public byte R
    {
        get => r;
        set
        {
            r = value;
            InvokeOnChanged();
        }
    }

    public byte G
    {
        get => g;
        set
        {
            g = value;
            InvokeOnChanged();
        }
    }

    public byte B
    {
        get => b;
        set
        {
            b = value;
            InvokeOnChanged();
        }
    }

    private void InvokeOnChanged()
    {
        Stripe?.RaiseOnStripeChangedLocally(Index);
    }

    /// <summary>
    /// The index of the current pixel in the stripe
    /// </summary>
    public int Index { get; internal set; }

    /// <summary>
    /// Gets a reference to the stripe this pixel is part of.
    /// </summary>
    public Stripe? Stripe { get; internal set; }

    /// <summary>
    /// Gets a value indicating whether this pixel is part of a stripe.
    /// </summary>
    public bool IsPartOfStripe => Stripe != null;

    public Pixel(byte r, byte g, byte b)
    {
        R = r;
        G = g;
        B = b;
    }

    internal Pixel(int index, Stripe stripe)
    {
        Index = index;
        Stripe = stripe;
    }

    public float Brightness
    {
        get => (R + G + B) / 3f;
        set
        {
            Stripe.SuppressSync = true;
            // Change the brightness of the pixel by the given value.
            R = (byte) (value * (R / Brightness)); // 255
            G = (byte) (value * (G / Brightness)); // 20
            B = (byte) (value * (B / Brightness)); // 0
            Stripe.SuppressSync = false;
            Stripe.RaiseOnStripeChangedLocally(Index);
        }
    }

    public Pixel Clone()
    {
        return new(R, G, B);
    }

    public static implicit operator Pixel(Color color) => new(color.R, color.G, color.B);

    public static implicit operator Color(Pixel pixel) => Color.FromArgb(pixel.R, pixel.G, pixel.B);

    public static implicit operator Pixel((byte, byte, byte) tuple) => new(tuple.Item1, tuple.Item2, tuple.Item3);

    public static implicit operator (byte, byte, byte)(Pixel pixel) => (pixel.R, pixel.G, pixel.B);
}