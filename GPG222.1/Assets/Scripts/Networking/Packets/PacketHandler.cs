using System.IO;
using UnityEngine;

public static class PacketHandler
{
    public static byte[] Encode(BasePacket packet)
    {
        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms);

        writer.Write((int)packet.Type);
        packet.WriteTo(writer);

        return ms.ToArray();
    }

    public static BasePacket Decode(byte[] data, int offset, int length)
    {
        if (length < 4)
        {
            Debug.LogError("Failed to decode packet: Not enough data to read packet type");
            return null;
        }

        using var ms = new MemoryStream(data, offset, length);
        using var reader = new BinaryReader(ms);

        try
        {
            PacketType type = (PacketType)reader.ReadInt32();
            return type switch
            {
                PacketType.PlayerTransform => PlayerTransformPacket.Read(reader),
                PacketType.FoodEaten => FoodEatenPacket.Read(reader),
                PacketType.FoodSpawn => FoodSpawnPacket.Read(reader),
                _ => null
            };
        }
        catch (EndOfStreamException e)
        {
            Debug.LogError($"Failed to decode packet: {e.Message}");
            return null;
        }
    }
}
