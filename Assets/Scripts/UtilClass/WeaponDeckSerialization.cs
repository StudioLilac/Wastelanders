using System;
using System.Collections.Generic;
using UnityEngine;
using static CardDatabase;

namespace WeaponDeckSerialization
{
    [System.Serializable]
    public class WeaponProficiency
    {
        [field: SerializeField] public CardDatabase.WeaponType WeaponType { get; set; }

        [field: SerializeField] public int MaxPoints { get; set; }

        public WeaponProficiency(CardDatabase.WeaponType weaponType, int maxPoints)
        {
            WeaponType = weaponType;
            MaxPoints = maxPoints;
        }
    }

    [System.Serializable]
    public class SerializableWeaponListEntry
    {
        public CardDatabase.WeaponType weapon;
        public List<SerializableActionClassInfo> weaponDeck;

        public SerializableWeaponListEntry(WeaponType weapon, List<SerializableActionClassInfo> weaponDeck)
        {
            this.weapon = weapon;
            this.weaponDeck = weaponDeck;
        }
    }

    // If we want to remember any specific ActionClass information add it here
    public interface ActionClassInfo
    {
        public bool IsEvolved { get; set;  }
    }

    // holds information that may be useful for an ActionClass when instantiating one. 
    public class InstantiableActionClassInfo : ActionClassInfo
    {
        // Holds a reference to an ActionClass Prefab
        public ActionClass ActionClass { get; set; }
        public bool IsEvolved { get; set; }

        public InstantiableActionClassInfo(ActionClass actionClass, bool isEvolved)
        {
            ActionClass = actionClass;
            IsEvolved = isEvolved;
            CheckIntegrity();
        }

        private void CheckIntegrity()
        {
            if (ActionClass == null)
            {
                Debug.LogWarning("Warning, there may be data corruption due to saving: " + ActionClass);
            }
        }
    }

[System.Serializable]
    public class SerializableActionClassInfo : ActionClassInfo
    {
        [field: SerializeField] private string _actionClassName;
        [field: SerializeField] public bool IsEvolved { get; set; }

        public string ActionClassName
        {
            get
            {
                CheckIntegrity();
                return _actionClassName;
            }
            set
            {
                _actionClassName = value;
            }
        }

        public SerializableActionClassInfo(string actionClassName, bool isEvolved = false)
        {
            _actionClassName = actionClassName;
            IsEvolved = isEvolved;
            CheckIntegrity();
        }

        private void CheckIntegrity()
        {
            if (string.IsNullOrEmpty(_actionClassName))
            {
                Debug.LogWarning("Warning, there may be data corruption due to saving: " + _actionClassName);
            }
        }
    }

}
