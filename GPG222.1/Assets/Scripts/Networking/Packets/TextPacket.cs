using System.IO;

public class TextPacket : BasePacket
{
    public string message;

    public TextPacket() {}

    public TextPacket(string msg)
    {
        message = msg;
    }

    public override PacketType Type => PacketType.Text;

    public override void WriteTo(BinaryWriter writer)
    {
        writer.Write(message ?? "");
    }

    public override void Read(BinaryReader reader)
    {
        message = reader.ReadString();
    }
}