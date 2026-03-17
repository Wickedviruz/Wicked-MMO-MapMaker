namespace MapMaker.Core.Sprites;

public enum ImportMode
{
    Single,     // En PNG = en sprite
    Sheet       // En PNG = många sprites
}

public class SheetOptions
{
    public int TileWidth { get; set; } = 32;
    public int TileHeight { get; set; } = 32;
    public int Columns { get; set; }
    public int Rows { get; set; }
    public int Spacing { get; set; } = 0;   // Pixlar mellan sprites
    public int Margin { get; set; } = 0;    // Pixlar runt kanten
}

public class ImportResult
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public List<SpriteEntry> ImportedSprites { get; set; } = new();
}

public static class SpriteImporter
{
    public static ImportResult ImportSingle(
        string imagePath,
        uint startId,
        string name,
        SpriteCategory category)
    {
        if (!File.Exists(imagePath))
            return Fail($"Filen hittades inte: {imagePath}");

        var ext = Path.GetExtension(imagePath).ToLower();
        if (ext != ".png")
            return Fail("Endast PNG-filer stöds.");

        var entry = new SpriteEntry
        {
            Id       = startId,
            Name     = name,
            X        = 0,
            Y        = 0,
            Width    = 0,   // Sätts av UI när bilden laddas
            Height   = 0,
            Category = category,
            ImagePath = imagePath
        };

        return new ImportResult
        {
            Success = true,
            ImportedSprites = [entry]
        };
    }

    public static ImportResult ImportSheet(
        string imagePath,
        uint startId,
        SpriteCategory category,
        SheetOptions options)
    {
        if (!File.Exists(imagePath))
            return Fail($"Filen hittades inte: {imagePath}");

        var ext = Path.GetExtension(imagePath).ToLower();
        if (ext != ".png")
            return Fail("Endast PNG-filer stöds.");

        if (options.Columns <= 0 || options.Rows <= 0)
            return Fail("Kolumner och rader måste vara större än 0.");

        if (options.TileWidth <= 0 || options.TileHeight <= 0)
            return Fail("Tile-storlek måste vara större än 0.");

        var sprites = new List<SpriteEntry>();
        uint currentId = startId;

        for (int row = 0; row < options.Rows; row++)
        {
            for (int col = 0; col < options.Columns; col++)
            {
                int x = options.Margin + col * (options.TileWidth + options.Spacing);
                int y = options.Margin + row * (options.TileHeight + options.Spacing);

                sprites.Add(new SpriteEntry
                {
                    Id        = currentId++,
                    Name      = $"Sprite_{currentId}",
                    X         = x,
                    Y         = y,
                    Width     = options.TileWidth,
                    Height    = options.TileHeight,
                    Category  = category,
                    ImagePath = imagePath
                });
            }
        }

        return new ImportResult
        {
            Success = true,
            ImportedSprites = sprites
        };
    }

    private static ImportResult Fail(string error) => new()
    {
        Success = false,
        Error = error
    };
}