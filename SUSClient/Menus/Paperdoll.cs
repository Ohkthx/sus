using System;
using SUS.Shared;
using SUS.Shared.Packets;

namespace SUSClient.MenuItems
{
    public class Paperdoll : Menu
    {
        private UInt64 m_PlayerID;
        private BaseMobile m_Mobile;

        #region Constructors
        public Paperdoll(UInt64 playerID, BaseMobile mobile) 
            : base ("What information would you like to observe?")
        {
            m_PlayerID = playerID;
            m_Mobile = mobile;

            foreach (GetMobilePacket.RequestReason opt in Enum.GetValues(typeof(GetMobilePacket.RequestReason)))
                if (opt != GetMobilePacket.RequestReason.None)
                    Options.Add(Enum.GetName(typeof(GetMobilePacket.RequestReason), opt));
        }
        #endregion

        public override Packet Display()
        {
            ShowMenu();
            PrintOptions();

            return MakeRequest(ParseOptions(GetInput()));
        }

        public GetMobilePacket MakeRequest(int input)
        {
            return new GetMobilePacket((GetMobilePacket.RequestReason)input, m_PlayerID);
        }

        protected override void PrintOptions()
        {
            foreach (string str in Options)
                Console.Write($"{str}  ");
            Console.WriteLine();
        }

        protected override int ParseOptions(string input)
        {
            input = input.ToLower();
            foreach (GetMobilePacket.RequestReason opt in Enum.GetValues(typeof(GetMobilePacket.RequestReason)))
            {
                if (opt == GetMobilePacket.RequestReason.None)
                    continue;
                else if (Enum.GetName(typeof(GetMobilePacket.RequestReason), opt).ToLower() == input)
                    return (int)opt;
            }
            return (int)GetMobilePacket.RequestReason.None;
        }
    }
}
