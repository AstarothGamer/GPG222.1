using System.IO;
using UnityEngine;

public class PlayerTransformPacket : BasePacket
{
    public string playerId;
    public string playerName;
    public Vector2 position;
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
        // Write player ID and name
        writer.Write(playerId);
        writer.Write(playerName);
        // Write transform data
        writer.Write(position.x);
        writer.Write(position.y);
        writer.Write(scale);
        // Write color components
        writer.Write(colorR);
        writer.Write(colorG);
        writer.Write(colorB);
        writer.Write(colorA);
    }

    public static PlayerTransformPacket Read(BinaryReader reader)
    {
        var packet = new PlayerTransformPacket();
        // Read player ID and name
        packet.playerId = reader.ReadString();
        packet.playerName = reader.ReadString();
        // Read transform data
        float x = reader.ReadSingle();
        float y = reader.ReadSingle();
        packet.position = new Vector2(x, y);
        packet.scale = reader.ReadSingle();
        // Read color components
        packet.colorR = reader.ReadSingle();
        packet.colorG = reader.ReadSingle();
        packet.colorB = reader.ReadSingle();
        packet.colorA = reader.ReadSingle();
        
        return packet;
    }
}