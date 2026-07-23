using AssetSpawner.Service;
using UnityEngine;

namespace AssetSpawner.Model
{
    [CreateAssetMenu(fileName = "AssetSpawnerSettingsScriptableObject",
        menuName = "AssetSpawer/AssetSpawnerSettingsScriptableObject")]
    public class AssetSpawnerSettingsScriptableObject : ScriptableObject
    {
        [SerializeField] private AssetSpawnerSettings settings;
        public AssetSpawnerSettings Settings => settings;
    }
}
