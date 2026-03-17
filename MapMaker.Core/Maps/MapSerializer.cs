using System.Text;
using MapMaker.Core.Map;

namespace MapMaker.Core.Maps;

public static class MapSerializer
{
    private static readonly byte[] Magic = "WMAP"u8.ToArray();
    private const ushort Version = 1;

    public static void Save(RMap map, SpawnData spawns, string path)
    {
        // Spara kart-data
        using var fs = File.OpenWrite(path);
        using var writer = new BinaryWriter(fs, Encoding.UTF8);

        // Header
        writer.Write(Magic);
        writer.Write(Version);
        writer.Write(map.Name);
        writer.Write(map.Width);
        writer.Write(map.Height);
        writer.Write(map.TileSize);

        // Lager
        writer.Write(map.Layers.Count);
        foreach (var layer in map.Layers)
        {
            writer.Write(layer.Id);
            writer.Write(layer.Name);
            writer.Write((byte)layer.Type);
            writer.Write(layer.Visible);

            writer.Write(layer.Tiles.Count);
            foreach (var (pos, tile) in layer.Tiles)
            {
                writer.Write(pos.X);
                writer.Write(pos.Y);
                writer.Write(tile.SpriteId);
                writer.Write(tile.Flags);
            }
        }

        // Spara spawn-XML bredvid kartan
        SpawnSerializer.Save(spawns, path);
    }

    public static (RMap map, SpawnData spawns) Load(string path)
    {
        using var fs = File.OpenRead(path);
        using var reader = new BinaryReader(fs, Encoding.UTF8);

        var magic = reader.ReadBytes(4);
        if (!magic.SequenceEqual(Magic))
            throw new InvalidDataException("Ogiltig .wmap-fil.");

        var version = reader.ReadUInt16();
        if (version != Version)
            throw new InvalidDataException($"Version {version} stöds ej.");

        var map = new RMap
        {
            Name     = reader.ReadString(),
            Width    = reader.ReadInt32(),
            Height   = reader.ReadInt32(),
            TileSize = reader.ReadInt32()
        };

        var layerCount = reader.ReadInt32();
        for (int i = 0; i < layerCount; i++)
        {
            var layer = new MapLayer
            {
                Id      = reader.ReadByte(),
                Name    = reader.ReadString(),
                Type    = (LayerType)reader.ReadByte(),
                Visible = reader.ReadBoolean()
            };

            var tileCount = reader.ReadInt32();
            for (int t = 0; t < tileCount; t++)
            {
                int x         = reader.ReadInt32();
                int y         = reader.ReadInt32();
                uint spriteId = reader.ReadUInt32();
                byte flags    = reader.ReadByte();
                layer.SetTile(x, y, spriteId, flags);
            }

            map.Layers.Add(layer);
        }

        // Ladda spawn-XML
        var spawns = SpawnSerializer.Load(path);

        return (map, spawns);
    }
}