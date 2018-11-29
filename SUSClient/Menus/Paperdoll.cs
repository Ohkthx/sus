using System;
using System.Collections.Generic;
using SUS.Shared.Objects;
using SUS.Shared.Packets;

namespace SUSClient.MenuItems
{
    public class Paperdoll : Menu
    {
        private BasicMobile m_Mobile    = null;

        public Paperdoll(BasicMobile mobile) : base ("What information would you like to observe?")
        {
            if (mobile == null)
                return;

            m_Mobile = mobile;

            foreach (GetMobilePacket.RequestReason opt in Enum.GetValues(typeof(GetMobilePacket.RequestReason)))
                if (opt != GetMobilePacket.RequestReason.None)
                    Options.Add(Enum.GetName(typeof(GetMobilePacket.RequestReason), opt));
        }

        public override Packet Display()
        {
            ShowMenu();
            PrintOptions();

            return MakeRequest(ParseOptions(GetInput()));
        }

        public GetMobilePacket MakeRequest(int input)
        {
            return new GetMobilePacket(m_Mobile, (GetMobilePacket.RequestReason)input);
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
