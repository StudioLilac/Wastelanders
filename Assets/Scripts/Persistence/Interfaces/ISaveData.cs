using System;
using UnityEngine;

namespace Systems.Persistence {
    public interface ISaveData {
        public string SaveName { get; }
    }
}