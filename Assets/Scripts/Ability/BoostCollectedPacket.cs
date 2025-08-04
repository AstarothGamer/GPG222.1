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


    public static BoostCollectedPacket Read(BinaryReader reader)
    {

        int id = reader.ReadInt32();

        return new BoostCollectedPacket(id);


    }


    //void Update()
    //{
        




    //}
}
