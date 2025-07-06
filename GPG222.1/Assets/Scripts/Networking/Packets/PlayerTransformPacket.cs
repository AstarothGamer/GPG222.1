using System.IO;
using UnityEngine;

public class PlayerTransformPacket : BasePacket
{
    public string playerId;
    public string playerName;
    public Vector2 position;
    public float scale;

    public PlayerTransformPacket() {}

    public PlayerTransformPacket(string id, Transform t)
    {
        playerId = id;
        playerName = Client.Instance.pd.playerName;
        if (t != null)
        {
            position = t.position;
            scale = t.localScale.x;
        }
    }

    public override PacketType Type => PacketType.PlayerTransform;

    public override void WriteTo(BinaryWriter writer)
    {
        writer.Write(playerId);
        writer.Write(playerName);
        writer.Write(position.x);
        writer.Write(position.y);
        writer.Write(scale);
    }

    public static PlayerTransformPacket Read(BinaryReader reader)
    {
        var packet = new PlayerTransformPacket();
        packet.playerId = reader.ReadString();
        packet.playerName = reader.ReadString();
        float x = reader.ReadSingle();
        float y = reader.ReadSingle();
        packet.position = new Vector2(x, y);
        packet.scale = reader.ReadSingle();
        return packet;
    }
}