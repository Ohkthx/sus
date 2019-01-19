namespace SUS.Shared
{
    public interface IPacket
    {
        ulong PlayerId { get; }
        Packets.PacketTypes Type { get; }
        byte[] ToByte();
    }
}
