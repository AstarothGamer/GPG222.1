using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStatePacket : BasePacket
{
    public int NumberOfPlayers = 0;
    public int MaxPlayers = 0;
    public bool CanJoin = false;
    public float Timer = 0f;
    public override PacketType Type => PacketType.GameState;
    
    public override void WriteTo(System.IO.BinaryWriter writer)
    {
        writer.Write(NumberOfPlayers);
        writer.Write(MaxPlayers);
        writer.Write(CanJoin);
        writer.Write(Timer);
    }
    public override void Read(System.IO.BinaryReader reader)
    {
        NumberOfPlayers = reader.ReadInt32();
        MaxPlayers = reader.ReadInt32();
        CanJoin = reader.ReadBoolean();
        Timer = reader.ReadSingle();
    }
}
