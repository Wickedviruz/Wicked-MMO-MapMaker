using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using MapMaker.Core.Sprites;

namespace MapMaker.Core.Assets;

public static class WPackSerializer
{
    private const string CatalogFile     = "catalog.json";
    private const string AppearancesFile = "appearances.wbin";

    public static void Save(string folderPath, SpriteAtlas atlas)
    {
        Directory.CreateDirectory(folderPath);

        var groups      = atlas.Sprites.GroupBy(s => s.ImagePath).ToList();
        var catalog     = new Catalog();
        var fileToSheet = new Dictionary<string, string>();
        int sheetIndex  = 0;

        foreach (var group in groups)
        {
            var sprites   = group.OrderBy(s => s.Id).ToList();
            var sheetName = $"sheet_{sheetIndex:D4}.wdat";
            var destPath  = Path.Combine(folderPath, sheetName);

            // Läs PNG och komprimera till .wdat med GZip
            var pngBytes = File.ReadAllBytes(group.Key);
            using (var outStream = File.Create(destPath))
            using (var gzip = new GZipStream(outStream, CompressionLevel.Optimal))
                gzip.Write(pngBytes, 0, pngBytes.Length);

            fileToSheet[group.Key] = sheetName;

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

        // Sprites
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
            writer.Write(fileToSheet[sprite.ImagePath]);
        }

        // Animationer
        writer.Write(atlas.Animations.Count);
        foreach (var anim in atlas.Animations)
        {
            writer.Write(anim.Id);
            writer.Write(anim.Name);
            writer.Write(anim.SpeedMs);
            writer.Write((byte)anim.LoopMode);
            writer.Write(anim.Frames.Count);
            foreach (var frameId in anim.Frames)
                writer.Write(frameId);
        }
    }

    public static SpriteAtlas Load(string folderPath)
    {
        var catalogPath = Path.Combine(folderPath, CatalogFile);
        if (!File.Exists(catalogPath))
            throw new InvalidDataException("Saknar catalog.json");

        var catalog = JsonSerializer.Deserialize<Catalog>(File.ReadAllText(catalogPath))
            ?? throw new InvalidDataException("Ogiltig catalog.json");

        // Dekomprimera .wdat → temp PNG-filer i minnet
        var sheetPngBytes = new Dictionary<string, byte[]>();
        foreach (var sheet in catalog.Sheets)
        {
            var sheetPath = Path.Combine(folderPath, sheet.File);
            using var inStream  = File.OpenRead(sheetPath);
            using var gzip      = new GZipStream(inStream, CompressionMode.Decompress);
            using var ms        = new MemoryStream();
            gzip.CopyTo(ms);
            sheetPngBytes[sheet.File] = ms.ToArray();
        }

        var appearancesPath = Path.Combine(folderPath, AppearancesFile);
        if (!File.Exists(appearancesPath))
            throw new InvalidDataException("Saknar appearances.wbin");

        var atlas = new SpriteAtlas();

        using var reader = new BinaryReader(File.OpenRead(appearancesPath));

        var magic = reader.ReadBytes(4);
        if (!magic.SequenceEqual("WBIN"u8.ToArray()))
            throw new InvalidDataException("Ogiltig appearances.wbin");

        reader.ReadUInt16();

        int spriteCount = reader.ReadInt32();
        for (int i = 0; i < spriteCount; i++)
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
                ImagePath = reader.ReadString() // sheet_0000.wdat
            });
        }

        int animCount = reader.ReadInt32();
        for (int i = 0; i < animCount; i++)
        {
            var anim = new SpriteAnimation
            {
                Id       = reader.ReadUInt32(),
                Name     = reader.ReadString(),
                SpeedMs  = reader.ReadInt32(),
                LoopMode = (AnimationLoopMode)reader.ReadByte()
            };
            int frameCount = reader.ReadInt32();
            for (int f = 0; f < frameCount; f++)
                anim.Frames.Add(reader.ReadUInt32());
            atlas.Animations.Add(anim);
        }

        // Sätt bytes på atlassen så converter kan läsa dem
        atlas.SheetBytes = sheetPngBytes;

        return atlas;
    }
}