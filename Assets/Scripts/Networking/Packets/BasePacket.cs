using System.IO;

public abstract class BasePacket
{
    public abstract PacketType Type { get; }
    public abstract void WriteTo(BinaryWriter writer);
}

public enum PacketType
{
    PlayerTransform = 0,
    FoodEaten = 1,
    FoodSpawn = 2
}
