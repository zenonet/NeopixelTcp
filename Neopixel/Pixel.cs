﻿using System.Drawing;

namespace Neopixel.Client;

public struct Pixel
{
    public Color Color { get; set; }
    public byte R;
    public byte G;
    public byte B;

    public Pixel(byte r, byte g, byte b)
    {
        R = r;
        G = g;
        B = b;
    }

    public float Brightness
    {
        get => (R + G + B) / 3f;
        set
        {
            R = (byte) (value * R / Brightness);
            G = (byte) (value * G / Brightness);
            B = (byte) (value * B / Brightness);
        }
    }

    public static implicit operator Pixel(Color color) => new(color.R, color.G, color.B);

    public static implicit operator Color(Pixel pixel) => Color.FromArgb(pixel.R, pixel.G, pixel.B);

    public static implicit operator Pixel((byte, byte, byte) tuple) => new(tuple.Item1, tuple.Item2, tuple.Item3);
    
    public static implicit operator (byte, byte, byte)(Pixel pixel) => (pixel.R, pixel.G, pixel.B);
}