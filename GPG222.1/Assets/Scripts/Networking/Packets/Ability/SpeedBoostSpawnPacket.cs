using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;


public class SpeedBoostSpawnPacket : BasePacket
{
    public int boostId;
    public Vector2 position;

    public SpeedBoostSpawnPacket() { }

    public SpeedBoostSpawnPacket(int id, Vector2 pos)
    {
        boostId = id;
        position = pos;
    }

    public override PacketType Type => PacketType.SpeedBoostSpawn;

    public override void WriteTo(BinaryWriter writer)
    {
        writer.Write(boostId);
        writer.Write(position.x);
        writer.Write(position.y);
    }

    public override void Read(BinaryReader reader)
    {
        boostId = reader.ReadInt32();
        position = new Vector2(reader.ReadSingle(), reader.ReadSingle());
    }
}
