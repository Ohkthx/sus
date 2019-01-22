using SUS.Shared.Packets;

namespace SUS.Shared
{
    public interface IPacket
    {
        ulong PlayerId { get; }
        PacketTypes Type { get; }
        byte[] ToByte();
    }
}