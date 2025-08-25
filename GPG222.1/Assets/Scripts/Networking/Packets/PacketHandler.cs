using System;
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

        int type = BitConverter.ToInt32(data, offset);
        BasePacket packet = type switch
        {
            0 => new PlayerTransformPacket(),
            1 => new FoodEatenPacket(),
            2 => new FoodSpawnPacket(),
            3 => new SpeedBoostSpawnPacket(),
            4 => new BoostCollectedPacket(),
            5 => new PlayerKilledPacket(),
            6 => new GameStatePacket(),
            _ => null
        };

        if (packet != null)
        {
            using var ms = new MemoryStream(data, offset, length);
            using var reader = new BinaryReader(ms);
            reader.ReadInt32(); // skip type
            packet.Read(reader);
        }

        return packet;
    }
}
