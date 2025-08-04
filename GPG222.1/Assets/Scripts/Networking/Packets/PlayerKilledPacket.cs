using System.IO;
using UnityEngine;

public class PlayerKilledPacket : BasePacket
{
    public string playerId;
    public bool canDie;

    public override PacketType Type => PacketType.PlayerKilled;

    public override void WriteTo(BinaryWriter writer)
    {
        writer.Write(playerId);
        writer.Write(canDie);
    }

    public static PlayerKilledPacket Read(BinaryReader reader)
    {
        return new PlayerKilledPacket
        {
            playerId = reader.ReadString(),
            canDie = reader.ReadBoolean()
        };
    }
}
