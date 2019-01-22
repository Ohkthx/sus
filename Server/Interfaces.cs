using System;
using System.Data.SQLite;
using SUS.Server.Objects;
using SUS.Shared;

namespace SUS.Server
{
    public interface IPoint2D
    {
        int X { get; }
        int Y { get; }
    }

    public interface IWeapon
    {
        int MaxRange { get; }
        void OnBeforeSwing(Mobile attacker, IDamageable damageable);
        TimeSpan OnSwing(Mobile attacker, IDamageable damageable);
        void GetStatusDamage(Mobile from, out int min, out int max);
        TimeSpan GetDelay(Mobile attacker);
    }

    public interface ISpawner
    {
        Guid ID { get; }
        Point2D HomeLocation { get; }
        int HomeRange { get; }
    }

    public interface ISpawnable : IEntity
    {
        ISpawner Spawner { get; set; }
    }

    public interface IEntity : IComparable, IComparable<IEntity>
    {
        Serial Serial { get; }
        string Name { get; }
        MobileTypes Type { get; }
        Point2D Location { get; }
        bool IsDeleted { get; }
        void Delete();
    }

    public interface IDamageable : IEntity
    {
        int CR { get; }
        int Hits { get; set; }
        int HitsMax { get; }
        bool Alive { get; }

        int Damage(int amount, Mobile attacker);
    }

    public interface ISQLCompatible
    {
        string ToString();
        int GetHashCode();
        bool Equals(object obj);
        void ToInsert(SQLiteCommand cmd);
    }

    public interface IPlayer : IDamageable
    {
        ulong PlayerID { get; }

        void AddKill();
    }
}