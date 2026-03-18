using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using MapMaker.Core.Sprites;
using SkiaSharp;

namespace MapMaker.App.Converters;

public class SpriteToBitmapConverter : IValueConverter
{
    public static readonly SpriteToBitmapConverter Instance = new();

    private readonly Dictionary<string, SKBitmap> _cache = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not SpriteEntry sprite) return null;
        if (string.IsNullOrEmpty(sprite.ImagePath)) return null;
        if (!File.Exists(sprite.ImagePath)) return null;

        if (!_cache.TryGetValue(sprite.ImagePath, out var source))
        {
            source = SKBitmap.Decode(sprite.ImagePath);
            if (source is null) return null;
            _cache[sprite.ImagePath] = source;
        }

        int w = sprite.Width  > 0 ? sprite.Width  : source.Width;
        int h = sprite.Height > 0 ? sprite.Height : source.Height;

        var cropped = new SKBitmap(w, h);
        using var canvas = new SKCanvas(cropped);
        canvas.DrawBitmap(source,
            new SKRectI(sprite.X, sprite.Y, sprite.X + w, sprite.Y + h),
            new SKRect(0, 0, w, h));

        using var image  = SKImage.FromBitmap(cropped);
        using var data   = image.Encode(SKEncodedImageFormat.Png, 100);
        using var stream = new MemoryStream(data.ToArray());
        return new Bitmap(stream);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}