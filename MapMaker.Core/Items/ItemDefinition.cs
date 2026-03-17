namespace MapMaker.Core.Items;

public enum ItemType : byte
{
    None,
    Ground,
    Container,
    Weapon,
    Armor,
    Helmet,
    Legs,
    Boots,
    Shield,
    Ammo,
    Food,
    Key,
    Readable,
    Fluid,
    Splash,
    Door,
    Teleport,
    MagicField,
    Bed
}

public enum StackOrder : byte
{
    None,
    Border,
    Bottom,
    Top
}

public class ItemDefinition
{
    public uint Id { get; set; }
    public uint SpriteId { get; set; }
    public string Name { get; set; } = string.Empty;
    public ItemType Type { get; set; } = ItemType.None;
    public ItemFlags Flags { get; set; } = ItemFlags.None;
    public StackOrder StackOrder { get; set; } = StackOrder.None;
    public byte LightLevel { get; set; }
    public byte LightColor { get; set; }
    public uint MinimapColor { get; set; }
    public ushort WareId { get; set; }
    public ushort ReadLength { get; set; }
    public ushort WriteLength { get; set; }

    // Helpers
    public bool HasFlag(ItemFlags flag) => Flags.HasFlag(flag);
    public void SetFlag(ItemFlags flag, bool value)
    {
        if (value) Flags |= flag;
        else Flags &= ~flag;
    }
}