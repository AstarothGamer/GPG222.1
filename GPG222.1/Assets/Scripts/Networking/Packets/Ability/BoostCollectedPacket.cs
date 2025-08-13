using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;


public class BoostCollectedPacket : BasePacket
{
    public int boostId;

    public BoostCollectedPacket() { }

    public BoostCollectedPacket(int id)
    {
        boostId = id;
    }

    public override PacketType Type => PacketType.BoostCollected;

    public override void WriteTo(BinaryWriter writer)
    {
        writer.Write(boostId);
    }

    public override void Read(BinaryReader reader)
    {
        boostId = reader.ReadInt32();
    }
}
