using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;



public class SpeedBoostSpawnPacket : BasePacket
{

    public int boostId;

    public Vector2 position;


    public override PacketType Type => PacketType.SpeedBoostSpawn;

    public override void WriteTo(BinaryWriter writer)
    {
        writer.Write(boostId);

        writer.Write(position.x);

        writer.Write(position.y);


    }

    public static SpeedBoostSpawnPacket Read(BinaryReader reader)
    {
        return new SpeedBoostSpawnPacket
        {
            boostId = reader.ReadInt32(),
            position = new Vector2(reader.ReadSingle(), reader.ReadSingle())


        };



    }

    //void Start()
    //{
        



    //}




    //void Update()
    //{
        



    //}





}
