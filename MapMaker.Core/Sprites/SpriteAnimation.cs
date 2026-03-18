namespace MapMaker.Core.Sprites;

public enum AnimationLoopMode
{
    Loop,       // Loopa för evigt
    PingPong,   // Fram och tillbaka
    Once        // Spela en gång
}

public class SpriteAnimation
{
    public uint             Id       { get; set; }
    public string           Name     { get; set; } = string.Empty;
    public List<uint>       Frames   { get; set; } = new();  // SpriteIDs i ordning
    public int              SpeedMs  { get; set; } = 150;
    public AnimationLoopMode LoopMode { get; set; } = AnimationLoopMode.Loop;
}