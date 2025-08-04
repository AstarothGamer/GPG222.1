using System.IO;
using UnityEngine;

public class FoodSpawnPacket : BasePacket
{
    public int foodId;
    public Vector2 position;

    public override PacketType Type => PacketType.FoodSpawn;

    public override void WriteTo(BinaryWriter writer)
    {
        writer.Write(foodId);
        writer.Write(position.x);
        writer.Write(position.y);
        writer.Write(position.x);
    }

    public static FoodSpawnPacket Read(BinaryReader reader)
    {
        return new FoodSpawnPacket
        {
            foodId = reader.ReadInt32(),
            position = new Vector2(reader.ReadSingle(), reader.ReadSingle())
        };
    }
}
