using System;
using System.Data.SQLite;
using SUS.Objects;
using SUS.Shared;

namespace SUS
{
    public class Gamestate : ISQLCompatible
    {
        private UInt64 m_PlayerID;
        private Player m_Account;
        private Regions m_Unlocked = Regions.None;

        #region Constructors
        public Gamestate(UInt64 playerID, Player account, Regions unlocked)
        {
            PlayerID = playerID;
            Account = account;
            Account.PlayerID = PlayerID;
            m_Unlocked |= unlocked;
            World.AddGamestate(this);
        }
        #endregion

        #region Overrides
        public void ToInsert(SQLiteCommand cmd)
        {
            cmd.Parameters.Add(new SQLiteParameter("@p1", PlayerID));
            cmd.Parameters.Add(new SQLiteParameter("@p2", ToByte()));
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 13;
                hash = (hash * 7) + (!Object.ReferenceEquals(null, PlayerID) ? PlayerID.GetHashCode() : 0);
                hash = (hash * 7) + (!Object.ReferenceEquals(null, Account.Serial) ? Account.Serial.GetHashCode() : 0);
                hash = (hash * 7) + (!Object.ReferenceEquals(null, Account.Type) ? Account.Type.GetHashCode() : 0);
                return hash;
            }
        }

        public static bool operator ==(Gamestate gs1, Gamestate gs2)
        {
            if (Object.ReferenceEquals(gs1, gs2)) return true;
            if (Object.ReferenceEquals(null, gs1)) return false;
            return (gs1.Equals(gs2));
        }

        public static bool operator !=(Gamestate gs1, Gamestate gs2)
        {
            return !(gs1 == gs2);
        }

        public override bool Equals(object value)
        {
            if (Object.ReferenceEquals(null, value)) return false;
            if (Object.ReferenceEquals(this, value)) return true;
            if (value.GetType() != this.GetType()) return false;
            return IsEqual((Gamestate)value);
        }

        public bool Equals(Gamestate gamestate)
        {
            if (Object.ReferenceEquals(null, gamestate)) return false;
            if (Object.ReferenceEquals(this, gamestate)) return true;
            return IsEqual(gamestate);
        }

        private bool IsEqual(Gamestate value)
        {
            return (value != null)
                && (value.Account != null)
                && (PlayerID == value.PlayerID)
                && (Account.Type == value.Account.Type)
                && (Account.Serial == value.Account.Serial);
        }
        #endregion

        #region Getters / Setters
        public UInt64 PlayerID
        {
            get { return m_PlayerID; }
            set
            {
                if (value == PlayerID)
                    return;

                m_PlayerID = value;
            }
        }

        public Player Account
        {
            get { return m_Account; }
            set
            {
                if (value == null || !value.IsPlayer)
                    return;

                m_Account = value;
            }
        }
        #endregion

        public ClientState ToClientState()
        {
            return new ClientState(PlayerID, Account.Base(), World.FindNode(Account.Region).GetBase(), m_Unlocked);
        }

        public byte[] ToByte() { return Network.Serialize(this); }
    }
}
