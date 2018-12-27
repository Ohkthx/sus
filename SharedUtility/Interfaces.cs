using System;

namespace SUS.Shared
{
    public interface IPacket
    {
        UInt64 PlayerID { get; }
        Packets.PacketTypes Type { get; }
        byte[] ToByte();
    }
}
