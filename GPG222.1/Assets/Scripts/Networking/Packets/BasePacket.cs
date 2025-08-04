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
    FoodSpawn = 2,

    SpeedBoostSpawn = 3,

    BoostCollected = 4,

    PlayerKilled = 5
}
