using System.IO;
using UnityEngine;

public class PlayerKilledPacket : BasePacket
{
    public string playerId;
    public bool canDie;

    public PlayerKilledPacket()
    {
        canDie = true;
    }

    public override PacketType Type => PacketType.PlayerKilled;

    public override void WriteTo(BinaryWriter writer)
    {
        writer.Write(playerId);
        writer.Write(canDie);
    }

    public override void Read(BinaryReader reader)
    {
        playerId = reader.ReadString();
        canDie = reader.ReadBoolean();
    }
}
