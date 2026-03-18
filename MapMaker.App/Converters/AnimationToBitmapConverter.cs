using System;
using System.Globalization;
using System.IO;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using MapMaker.App.State;
using MapMaker.Core.Sprites;
using SkiaSharp;
using System.Collections.Generic;

namespace MapMaker.App.Converters;

public class AnimationToBitmapConverter : IValueConverter
{
    public static readonly AnimationToBitmapConverter Instance = new();

    private readonly SpriteToBitmapConverter _spriteConverter = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not SpriteAnimation anim) return null;
        if (anim.Frames.Count == 0) return null;

        var firstSprite = EditorSession.Current.Atlas.GetById(anim.Frames[0]);
        if (firstSprite is null) return null;

        return _spriteConverter.Convert(firstSprite, targetType, parameter, culture);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}