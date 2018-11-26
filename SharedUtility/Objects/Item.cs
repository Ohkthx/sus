using System;

namespace SUS.Shared.Objects
{
    [Serializable, Flags]
    public enum ItemTypes
    {
        None        = 0x00000000,

        Consumable  = 0x00000001,

        // Equippables
        Armor       = 0x00000002,
        Weapon      = 0x00000004,

        Equippable = Armor | Weapon,
    }

    [Serializable]
    public abstract class Item
    {
        protected Guid m_Guid;
        protected string m_Name;
        protected ItemTypes m_Type;
        protected int m_Weight;
        protected bool m_isDestroyable;

        #region Constructors
        protected Item(ItemTypes type)
        {
            Type = type;
        }
        #endregion

        #region Getters / Setters
        public Guid Guid
        {
            get
            {
                if (m_Guid == null || m_Guid == Guid.Empty)
                    m_Guid = Guid.NewGuid();

                return m_Guid;
            }
        }

        public string Name
        {
            get
            {
                if (m_Name != null)
                    return m_Name;
                else
                    return "Unknown";
            }
            set
            {
                if (value != m_Name)
                    m_Name = value;
            }
        }

        public ItemTypes Type
        {
            get { return m_Type; }
            protected set
            {
                if (value != ItemTypes.None && value != Type)
                    m_Type = value;
            }
        }

        public int Weight 
        {
            get { return m_Weight; }
            protected set
            {
                if (value != Weight)
                    m_Weight = value;
            }
        }

        public bool IsEquippable { get { return (ItemTypes.Equippable & Type) == Type; } }

        public bool IsDestroyable { get { return m_isDestroyable; } }
        #endregion
    }
}
