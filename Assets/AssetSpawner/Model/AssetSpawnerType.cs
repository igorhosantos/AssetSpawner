using System;
using UnityEngine;

namespace AssetSpawner.Model
{
    [Serializable]
    public class AssetSpawnerInfo
    {
        public string Key;
        public string Address;
        
        public void SetData(GameObject prefab)
        {
            Key = prefab.name;
            Address = "Prefabs";
        }
    }
}
