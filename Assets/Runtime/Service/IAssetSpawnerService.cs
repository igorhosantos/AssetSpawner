
using System;
using System.Collections;
using UnityEngine;

namespace AssetSpawner.Service
{
    public interface IAssetSpawnerService
    {
        IEnumerator InitializeService();
        IEnumerator SpawnAsset(string assetKey, Transform container);
    }

    [Serializable]
    public class AssetSpawnerSettings
    {
        public enum Providers
        {
            StandardAssetBundle = 0,
        }
        
        public enum LoadingTypes
        {
            OnDemand = 0,
            LoadAndCacheAll = 1,
        }
        
        public Providers ProviderType;
        public LoadingTypes LoadingType;
        public string StreamingAssetPath;
    }
    
}
