using System;
using SUS.Shared;
using SUS.Shared.Packets;

namespace SUS.Client.Menus
{
    public class Paperdoll : Menu
    {
        private readonly BaseMobile m_Mobile;
        private readonly ulong m_PlayerId;

        #region Constructors

        public Paperdoll(ulong playerId, BaseMobile mobile)
            : base("What information would you like to observe?")
        {
            m_PlayerId = playerId;
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

        private GetMobilePacket MakeRequest(int input)
        {
            return new GetMobilePacket((GetMobilePacket.RequestReason) input, m_PlayerId);
        }

        protected override void PrintOptions()
        {
            foreach (var str in Options) Console.Write($"{str}  ");

            Console.WriteLine();
        }

        protected override int ParseOptions(string input)
        {
            input = input.ToLower();
            foreach (GetMobilePacket.RequestReason opt in Enum.GetValues(typeof(GetMobilePacket.RequestReason)))
            {
                if (opt == GetMobilePacket.RequestReason.None) continue;

                if (Enum.GetName(typeof(GetMobilePacket.RequestReason), opt)?.ToLower() == input) return (int) opt;
            }

            return (int) GetMobilePacket.RequestReason.None;
        }
    }
}