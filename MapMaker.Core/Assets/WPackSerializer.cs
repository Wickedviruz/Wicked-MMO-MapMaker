using System.IO;
using System.Linq;
using System.Text.Json;
using System.Collections.Generic;
using MapMaker.Core.Sprites;

namespace MapMaker.Core.Assets;

public static class WPackSerializer
{
    private const string CatalogFile     = "catalog.json";
    private const string AppearancesFile = "appearances.wbin";

    public static void Save(string folderPath, SpriteAtlas atlas)
    {
        Directory.CreateDirectory(folderPath);

        var groups     = atlas.Sprites.GroupBy(s => s.ImagePath).ToList();
        var catalog    = new Catalog();
        int sheetIndex = 0;

        foreach (var group in groups)
        {
            var sprites   = group.OrderBy(s => s.Id).ToList();
            var sheetName = $"sheet_{sheetIndex:D4}.png";
            var destPath  = Path.Combine(folderPath, sheetName);

            // Kopiera PNG till projektmappen om den inte redan ligger där
            if (!string.Equals(group.Key, destPath, StringComparison.OrdinalIgnoreCase))
                File.Copy(group.Key, destPath, overwrite: true);

            catalog.Sheets.Add(new CatalogEntry
            {
                File       = sheetName,
                FirstId    = sprites.First().Id,
                LastId     = sprites.Last().Id,
                TileWidth  = sprites.First().Width,
                TileHeight = sprites.First().Height
            });

            sheetIndex++;
        }

        // Spara catalog.json
        File.WriteAllText(
            Path.Combine(folderPath, CatalogFile),
            JsonSerializer.Serialize(catalog, new JsonSerializerOptions { WriteIndented = true }));

        // Spara appearances.wbin
        using var writer = new BinaryWriter(
            File.Create(Path.Combine(folderPath, AppearancesFile)));

        writer.Write("WBIN"u8.ToArray());
        writer.Write((ushort)1);
        writer.Write(atlas.Sprites.Count);

        foreach (var sprite in atlas.Sprites)
        {
            writer.Write(sprite.Id);
            writer.Write(sprite.Name);
            writer.Write((byte)sprite.Category);
            writer.Write(sprite.X);
            writer.Write(sprite.Y);
            writer.Write(sprite.Width);
            writer.Write(sprite.Height);
            writer.Write(Path.GetFileName(sprite.ImagePath));
        }
    }

    public static SpriteAtlas Load(string folderPath)
    {
        var catalogPath = Path.Combine(folderPath, CatalogFile);
        if (!File.Exists(catalogPath))
            throw new InvalidDataException("Saknar catalog.json");

        var catalog = JsonSerializer.Deserialize<Catalog>(File.ReadAllText(catalogPath))
            ?? throw new InvalidDataException("Ogiltig catalog.json");

        var appearancesPath = Path.Combine(folderPath, AppearancesFile);
        if (!File.Exists(appearancesPath))
            throw new InvalidDataException("Saknar appearances.wbin");

        var atlas = new SpriteAtlas();

        using var reader = new BinaryReader(File.OpenRead(appearancesPath));

        var magic = reader.ReadBytes(4);
        if (!magic.SequenceEqual("WBIN"u8.ToArray()))
            throw new InvalidDataException("Ogiltig appearances.wbin");

        reader.ReadUInt16();
        int count = reader.ReadInt32();

        for (int i = 0; i < count; i++)
        {
            atlas.Sprites.Add(new SpriteEntry
            {
                Id        = reader.ReadUInt32(),
                Name      = reader.ReadString(),
                Category  = (SpriteCategory)reader.ReadByte(),
                X         = reader.ReadInt32(),
                Y         = reader.ReadInt32(),
                Width     = reader.ReadInt32(),
                Height    = reader.ReadInt32(),
                // Direkt sökväg till PNG i projektmappen
                ImagePath = Path.Combine(folderPath, reader.ReadString())
            });
        }

        return atlas;
    }
}