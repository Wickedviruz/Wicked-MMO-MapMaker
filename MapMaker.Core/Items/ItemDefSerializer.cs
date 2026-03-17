using System.Text;

namespace MapMaker.Core.Items;

public static class ItemDefSerializer
{
    private static readonly byte[] Magic = "WDEF"u8.ToArray();
    private const ushort Version = 1;

    public static void Save(IEnumerable<ItemDefinition> items, string path)
    {
        using var fs = File.OpenWrite(path);
        using var writer = new BinaryWriter(fs, Encoding.UTF8);

        writer.Write(Magic);
        writer.Write(Version);

        var list = items.ToList();
        writer.Write(list.Count);

        foreach (var item in list)
        {
            writer.Write(item.Id);
            writer.Write(item.SpriteId);
            writer.Write(item.Name);
            writer.Write((byte)item.Type);
            writer.Write((uint)item.Flags);
            writer.Write((byte)item.StackOrder);
            writer.Write(item.LightLevel);
            writer.Write(item.LightColor);
            writer.Write(item.MinimapColor);
            writer.Write(item.WareId);
            writer.Write(item.ReadLength);
            writer.Write(item.WriteLength);
        }
    }

    public static List<ItemDefinition> Load(string path)
    {
        using var fs = File.OpenRead(path);
        using var reader = new BinaryReader(fs, Encoding.UTF8);

        var magic = reader.ReadBytes(4);
        if (!magic.SequenceEqual(Magic))
            throw new InvalidDataException("Ogiltig .wdef-fil.");

        var version = reader.ReadUInt16();
        if (version != Version)
            throw new InvalidDataException($"Version {version} stöds ej.");

        var count = reader.ReadInt32();
        var items = new List<ItemDefinition>(count);

        for (int i = 0; i < count; i++)
        {
            items.Add(new ItemDefinition
            {
                Id           = reader.ReadUInt32(),
                SpriteId     = reader.ReadUInt32(),
                Name         = reader.ReadString(),
                Type         = (ItemType)reader.ReadByte(),
                Flags        = (ItemFlags)reader.ReadUInt32(),
                StackOrder   = (StackOrder)reader.ReadByte(),
                LightLevel   = reader.ReadByte(),
                LightColor   = reader.ReadByte(),
                MinimapColor = reader.ReadUInt32(),
                WareId       = reader.ReadUInt16(),
                ReadLength   = reader.ReadUInt16(),
                WriteLength  = reader.ReadUInt16()
            });
        }

        return items;
    }
}