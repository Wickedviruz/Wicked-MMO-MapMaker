using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using SkiaSharp;
using MapMaker.Core.Sprites;

namespace MapMaker.App.Converters;

public class SpriteToBitmapConverter : IValueConverter
{
    public static readonly SpriteToBitmapConverter Instance = new();

    // Cache så vi inte läser PNG från disk varje gång
    private readonly Dictionary<string, SKBitmap> _cache = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not SpriteEntry sprite) return null;
        if (string.IsNullOrEmpty(sprite.ImagePath)) return null;
        if (!System.IO.File.Exists(sprite.ImagePath)) return null;

        // Ladda source-bilden (cachad)
        if (!_cache.TryGetValue(sprite.ImagePath, out var source))
        {
            source = SKBitmap.Decode(sprite.ImagePath);
            if (source is null) return null;
            _cache[sprite.ImagePath] = source;
        }

        // Om enskild sprite utan definierad storlek — returnera hela bilden
        int w = sprite.Width  > 0 ? sprite.Width  : source.Width;
        int h = sprite.Height > 0 ? sprite.Height : source.Height;

        // Klipp ut rätt region
        var cropped = new SKBitmap(w, h);
        using var canvas = new SKCanvas(cropped);
        var srcRect  = new SKRectI(sprite.X, sprite.Y, sprite.X + w, sprite.Y + h);
        var destRect = new SKRect(0, 0, w, h);
        canvas.DrawBitmap(source, srcRect, destRect);

        // Konvertera SKBitmap → Avalonia Bitmap
        using var image = SKImage.FromBitmap(cropped);
        using var data  = image.Encode(SKEncodedImageFormat.Png, 100);
        using var stream = new System.IO.MemoryStream(data.ToArray());
        return new Bitmap(stream);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}