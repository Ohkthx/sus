using System.Data.SQLite;
using SUS.Server.Objects.Mobiles;
using SUS.Shared;

namespace SUS.Server
{
    public class Gamestate : ISQLCompatible
    {
        private readonly Regions _unlocked = Regions.None;
        private Player _account;
        private ulong _playerId;

        #region Constructors

        public Gamestate(ulong playerId, Player account, Regions unlocked)
        {
            PlayerId = playerId;
            Account = account;
            Account.PlayerID = PlayerId;
            _unlocked |= unlocked;
            World.AddGamestate(this);
        }

        #endregion

        public ClientState ToClientState()
        {
            return new ClientState(PlayerId, Account.Base(), World.FindNode(Account.Region).GetBase(), _unlocked);
        }

        private byte[] ToByte()
        {
            return Network.Serialize(this);
        }

        #region Overrides

        public void ToInsert(SQLiteCommand cmd)
        {
            cmd.Parameters.Add(new SQLiteParameter("@p1", PlayerId));
            cmd.Parameters.Add(new SQLiteParameter("@p2", ToByte()));
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = 13;
                hash = hash * 7 + PlayerId.GetHashCode();
                hash = hash * 7 + Account.Serial.GetHashCode();
                hash = hash * 7 + Account.Type.GetHashCode();
                return hash;
            }
        }

        public static bool operator ==(Gamestate gs1, Gamestate gs2)
        {
            if (ReferenceEquals(gs1, gs2)) return true;

            return !ReferenceEquals(null, gs1) && gs1.Equals(gs2);
        }

        public static bool operator !=(Gamestate gs1, Gamestate gs2)
        {
            return !(gs1 == gs2);
        }

        public override bool Equals(object value)
        {
            if (ReferenceEquals(null, value)) return false;

            if (ReferenceEquals(this, value)) return true;

            return value.GetType() == GetType() && IsEqual((Gamestate) value);
        }

        private bool Equals(Gamestate gamestate)
        {
            if (ReferenceEquals(null, gamestate)) return false;

            return ReferenceEquals(this, gamestate) || IsEqual(gamestate);
        }

        private bool IsEqual(Gamestate value)
        {
            return value != null
                   && value.Account != null
                   && PlayerId == value.PlayerId
                   && Account.Type == value.Account.Type
                   && Account.Serial == value.Account.Serial;
        }

        #endregion

        #region Getters / Setters

        public ulong PlayerId
        {
            get => _playerId;
            private set
            {
                if (value == PlayerId) return;

                _playerId = value;
            }
        }

        public Player Account
        {
            get => _account;
            private set
            {
                if (value == null || !value.IsPlayer) return;

                _account = value;
            }
        }

        #endregion
    }
}