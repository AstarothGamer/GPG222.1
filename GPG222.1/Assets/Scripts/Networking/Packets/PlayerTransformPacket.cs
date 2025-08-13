using System.IO;
using UnityEngine;

public class PlayerTransformPacket : BasePacket
{
    public string playerId;
    public string playerName;
    public Vector3 position;
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
        writer.Write(position.z);
        writer.Write(scale);
    }

    public override void Read(BinaryReader reader)
    {
        playerId = reader.ReadString();
        playerName = reader.ReadString();
        position = new Vector3(
            reader.ReadSingle(),
            reader.ReadSingle(),
            reader.ReadSingle()
        );
        scale = reader.ReadSingle();
    }
}