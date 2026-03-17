namespace MapMaker.Core.Items;

[Flags]
public enum ItemFlags : uint
{
    None             = 0,
    Walkable         = 1 << 0,
    BlockSolid       = 1 << 1,
    BlockMissiles    = 1 << 2,
    BlockPathfinder  = 1 << 3,
    Pickupable       = 1 << 4,
    Stackable        = 1 << 5,
    Movable          = 1 << 6,
    Rotatable        = 1 << 7,
    Hangable         = 1 << 8,
    HookSouth        = 1 << 9,
    HookEast         = 1 << 10,
    Readable         = 1 << 11,
    MultiUse         = 1 << 12,
    ForceUse         = 1 << 13,
    FullGround       = 1 << 14,
    HasElevation     = 1 << 15,
    IgnoreLook       = 1 << 16,
}