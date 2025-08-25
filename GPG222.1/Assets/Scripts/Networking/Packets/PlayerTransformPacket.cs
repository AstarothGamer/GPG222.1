using System.IO;
using UnityEngine;

public class PlayerTransformPacket : BasePacket
{
    public string playerId;
    public string playerName;
    public Vector3 position;
    public float scale;
    public float colorR, colorG, colorB, colorA;

    public PlayerTransformPacket() {}

    public PlayerTransformPacket(string id, Transform t, Color color)
    {
        playerId = id;
        playerName = Client.Instance.pd.playerName;
        if (t != null)
        {
            position = t.position;
            scale = t.localScale.x;
        }

        colorR = color.r;
        colorG = color.g;
        colorB = color.b;
        colorA = color.a;
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
        // Write color components
        writer.Write(colorR);
        writer.Write(colorG);
        writer.Write(colorB);
        writer.Write(colorA);
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
        colorR = reader.ReadSingle();
        colorG = reader.ReadSingle();
        colorB = reader.ReadSingle();
        colorA = reader.ReadSingle();
    }
}