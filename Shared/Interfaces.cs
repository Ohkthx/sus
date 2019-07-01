namespace SUS.Shared
{
    public interface IPacket
    {
        ulong PlayerId { get; }

        byte[] ToByte();
    }

    public interface IAction
    {
        int MobileId { get; }
    }
}