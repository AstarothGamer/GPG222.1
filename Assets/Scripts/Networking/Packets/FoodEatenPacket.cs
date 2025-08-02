using System.IO;

public class FoodEatenPacket : BasePacket
{
    public int foodId;

    public FoodEatenPacket() {}

    public FoodEatenPacket(int id)
    {
        foodId = id;
    }

    public override PacketType Type => PacketType.FoodEaten;

    public override void WriteTo(BinaryWriter writer)
    {
        writer.Write(foodId);
    }

    public static FoodEatenPacket Read(BinaryReader reader)
    {
        int id = reader.ReadInt32();
        return new FoodEatenPacket(id);
    }
}