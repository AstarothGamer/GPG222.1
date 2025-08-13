using System.IO;
using UnityEngine;

public class FoodSpawnPacket : BasePacket
{
    public int foodId;
    public Vector2 position;

    public FoodSpawnPacket() { }

    public FoodSpawnPacket(int id, Vector2 pos)
    {
        foodId = id;
        position = pos;
    }

    public override PacketType Type => PacketType.FoodSpawn;

    public override void WriteTo(BinaryWriter writer)
    {
        writer.Write(foodId);
        writer.Write(position.x);
        writer.Write(position.y);
    }

    public override void Read(BinaryReader reader)
    {
        foodId = reader.ReadInt32();
        position = new Vector2(reader.ReadSingle(), reader.ReadSingle());
    }
}
