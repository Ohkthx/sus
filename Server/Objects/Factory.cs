using System;
using SUS.Server.Objects.Items;
using SUS.Server.Objects.Items.Equipment;
using SUS.Server.Objects.Mobiles;
using SUS.Server.Objects.Mobiles.Spawns;

namespace SUS.Server.Objects
{
    public static class Factory
    {
        public static BaseCreature GetSpawn(SpawnTypes spawn)
        {
            switch (spawn)
            {
                case SpawnTypes.Skeleton:
                    return new Skeleton();
                case SpawnTypes.Zombie:
                    return new Zombie();
                case SpawnTypes.Ghoul:
                    return new Ghoul();
                case SpawnTypes.Wraith:
                    return new Wraith();
                case SpawnTypes.Lizardman:
                    return new Lizardman();
                case SpawnTypes.Ettin:
                    return new Ettin();
                case SpawnTypes.Orc:
                    return new Orc();
                case SpawnTypes.Cyclops:
                    return new Cyclops();
                case SpawnTypes.Titan:
                    return new Titan();
                default:
                    throw new InvalidFactoryException($"Spawn: {Enum.GetName(typeof(SpawnTypes), spawn)}");
            }
        }

        public static Weapon GetWeapon(WeaponTypes weapon, Weapon.Materials material)
        {
            switch(weapon)
            {
                case WeaponTypes.ShortBow:
                    return new ShortBow(material);
                case WeaponTypes.CompositeBow:
                    return new CompositeBow(material);

                case WeaponTypes.Dagger:
                    return new Dagger(material);
                case WeaponTypes.Kryss:
                    return new Kryss(material);

                case WeaponTypes.Mace:
                    return new Mace(material);
                case WeaponTypes.Maul:
                    return new Maul(material);

                case WeaponTypes.ShortSword:
                    return new ShortSword(material);
                case WeaponTypes.TwoHandedSword:
                    return new TwoHandedSword(material);

                default:
                    throw new InvalidFactoryException($"Weapon: {Enum.GetName(typeof(WeaponTypes), weapon)}, Material: {Enum.GetName(typeof(Weapon.Materials), material)}");
            }
        }
    }
}
