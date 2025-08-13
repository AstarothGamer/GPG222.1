using System.IO;

public class FoodEatenPacket : BasePacket
{
    public int foodId;

    public FoodEatenPacket() { }

    public FoodEatenPacket(int id)
    {
        foodId = id;
    }

    public override PacketType Type => PacketType.FoodEaten;

    public override void WriteTo(BinaryWriter writer)
    {
        writer.Write(foodId);
    }

    public override void Read(BinaryReader reader)
    {
        foodId = reader.ReadInt32();
    }
}