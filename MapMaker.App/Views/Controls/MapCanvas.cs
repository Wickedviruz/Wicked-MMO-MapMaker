using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using MapMaker.App.State;
using MapMaker.App.ViewModels;
using SkiaSharp;

namespace MapMaker.App.Views.Controls;

public class MapCanvas : Control
{
    private SKBitmap?  _renderBuffer;
    private float      _zoom        = 1.0f;
    private float      _offsetX     = 0;
    private float      _offsetY     = 0;
    private bool       _isPanning   = false;
    private Point      _lastPanPoint;
    private bool       _isPainting  = false;

    public MapWorkspaceViewModel? ViewModel { get; set; }

    public override void Render(DrawingContext context)
    {
        var map = EditorSession.Current.CurrentMap;
        if (map is null)
        {
            // Rita placeholder
            context.DrawText(
                new FormattedText("Ingen karta laddad — skapa en ny via File → New Map",
                    System.Globalization.CultureInfo.InvariantCulture,
                    FlowDirection.LeftToRight,
                    Typeface.Default, 14, Brushes.Gray),
                new Point(20, 20));
            return;
        }

        var tileSize = map.TileSize * _zoom;
        var atlas    = EditorSession.Current.Atlas;

        // Rita bakgrund
        context.FillRectangle(new SolidColorBrush(Color.Parse("#1e1e1e")),
            new Rect(Bounds.Size));

        // Rita tiles per lager (underifrån och upp)
        foreach (var layer in map.Layers)
        {
            if (!layer.Visible) continue;

            foreach (var (pos, tile) in layer.Tiles)
            {
                var sprite = atlas.GetById(tile.SpriteId);
                if (sprite is null) continue;

                var screenX = _offsetX + pos.X * tileSize;
                var screenY = _offsetY + pos.Y * tileSize;

                // Kolla om tiles är synlig
                if (screenX + tileSize < 0 || screenX > Bounds.Width)  continue;
                if (screenY + tileSize < 0 || screenY > Bounds.Height) continue;

                // Hämta bitmap från converter
                var bmp = GetSpriteBitmap(sprite);
                if (bmp is null) continue;

                context.DrawImage(bmp,
                    new Rect(0, 0, bmp.PixelSize.Width, bmp.PixelSize.Height),
                    new Rect(screenX, screenY, tileSize, tileSize));
            }
        }

        // Rita grid
        if (ViewModel?.ShowGrid == true)
            DrawGrid(context, tileSize);

        // Rita cursor highlight
        if (ViewModel is not null)
            DrawCursor(context, tileSize);
    }

    private void DrawGrid(DrawingContext context, float tileSize)
    {
        var pen = new Pen(new SolidColorBrush(Color.FromArgb(40, 255, 255, 255)), 1);

        var startX = (int)(_offsetX % tileSize);
        var startY = (int)(_offsetY % tileSize);

        for (float x = startX; x < Bounds.Width; x += tileSize)
            context.DrawLine(pen, new Point(x, 0), new Point(x, Bounds.Height));

        for (float y = startY; y < Bounds.Height; y += tileSize)
            context.DrawLine(pen, new Point(0, y), new Point(Bounds.Width, y));
    }

    private void DrawCursor(DrawingContext context, float tileSize)
    {
        if (ViewModel?.CursorTileX is null) return;

        var screenX = _offsetX + ViewModel.CursorTileX.Value * tileSize;
        var screenY = _offsetY + ViewModel.CursorTileY!.Value * tileSize;

        var brushSize = ViewModel.BrushSize;
        var rect = new Rect(screenX, screenY,
            tileSize * brushSize, tileSize * brushSize);

        context.DrawRectangle(
            new SolidColorBrush(Color.FromArgb(60, 255, 255, 255)),
            new Pen(Brushes.White, 1),
            rect);
    }

    // Sprite bitmap cache
    private readonly System.Collections.Generic.Dictionary<uint, Avalonia.Media.Imaging.Bitmap> _bitmapCache = new();

    private Avalonia.Media.Imaging.Bitmap? GetSpriteBitmap(MapMaker.Core.Sprites.SpriteEntry sprite)
    {
        if (_bitmapCache.TryGetValue(sprite.Id, out var cached)) return cached;

        var converter = new Converters.SpriteToBitmapConverter();
        var bmp = converter.Convert(sprite, typeof(Avalonia.Media.Imaging.Bitmap), null,
            System.Globalization.CultureInfo.InvariantCulture) as Avalonia.Media.Imaging.Bitmap;

        if (bmp is not null)
            _bitmapCache[sprite.Id] = bmp;

        return bmp;
    }

    // Input
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        var point = e.GetCurrentPoint(this);

        if (point.Properties.IsMiddleButtonPressed)
        {
            _isPanning   = true;
            _lastPanPoint = e.GetPosition(this);
            e.Handled    = true;
            return;
        }

        if (point.Properties.IsLeftButtonPressed)
        {
            _isPainting = true;
            PaintAt(e.GetPosition(this));
            e.Handled = true;
        }

        if (point.Properties.IsRightButtonPressed)
        {
            EraseAt(e.GetPosition(this));
            e.Handled = true;
        }
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        var pos = e.GetPosition(this);

        if (_isPanning)
        {
            _offsetX += (float)(pos.X - _lastPanPoint.X);
            _offsetY += (float)(pos.Y - _lastPanPoint.Y);
            _lastPanPoint = pos;
            InvalidateVisual();
            return;
        }

        // Uppdatera cursor-position
        UpdateCursorTile(pos);

        if (_isPainting)
            PaintAt(pos);

        InvalidateVisual();
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        _isPanning  = false;
        _isPainting = false;
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        var pos     = e.GetPosition(this);
        var oldZoom = _zoom;

        _zoom *= e.Delta.Y > 0 ? 1.1f : 0.9f;
        _zoom  = Math.Clamp(_zoom, 0.25f, 8.0f);

        // Zooma mot musposition
        _offsetX = (float)(pos.X - (_zoom / oldZoom) * (pos.X - _offsetX));
        _offsetY = (float)(pos.Y - (_zoom / oldZoom) * (pos.Y - _offsetY));

        if (ViewModel is not null)
            ViewModel.ZoomLabel = $"{(int)(_zoom * 100)}%";

        InvalidateVisual();
    }

    private void UpdateCursorTile(Point pos)
    {
        if (ViewModel is null) return;
        var map = EditorSession.Current.CurrentMap;
        if (map is null) return;

        var tileSize = map.TileSize * _zoom;
        var tileX    = (int)((pos.X - _offsetX) / tileSize);
        var tileY    = (int)((pos.Y - _offsetY) / tileSize);

        ViewModel.CursorTileX   = tileX;
        ViewModel.CursorTileY   = tileY;
        ViewModel.CursorPosition = $"X: {tileX}, Y: {tileY}";
    }

    private void PaintAt(Point pos)
    {
        if (ViewModel is null) return;
        var map   = EditorSession.Current.CurrentMap;
        var layer = ViewModel.ActiveLayer;
        if (map is null || layer is null) return;

        var selectedSprite = ViewModel.SelectedSpriteId;
        if (selectedSprite is null) return;

        var tileSize = map.TileSize * _zoom;
        var tileX    = (int)((pos.X - _offsetX) / tileSize);
        var tileY    = (int)((pos.Y - _offsetY) / tileSize);

        var brushSize = ViewModel.BrushSize;
        for (int bx = 0; bx < brushSize; bx++)
        for (int by = 0; by < brushSize; by++)
            layer.SetTile(tileX + bx, tileY + by, selectedSprite.Value);

        EditorSession.Current.MarkDirty();
        InvalidateVisual();
    }

    private void EraseAt(Point pos)
    {
        if (ViewModel is null) return;
        var map   = EditorSession.Current.CurrentMap;
        var layer = ViewModel.ActiveLayer;
        if (map is null || layer is null) return;

        var tileSize = map.TileSize * _zoom;
        var tileX    = (int)((pos.X - _offsetX) / tileSize);
        var tileY    = (int)((pos.Y - _offsetY) / tileSize);

        var brushSize = ViewModel.BrushSize;
        for (int bx = 0; bx < brushSize; bx++)
        for (int by = 0; by < brushSize; by++)
            layer.RemoveTile(tileX + bx, tileY + by);

        EditorSession.Current.MarkDirty();
        InvalidateVisual();
    }

    public void ClearBitmapCache() => _bitmapCache.Clear();
}