
using System;
using System.Collections.Generic;
using System.Linq;
using AssetSpawner.Model;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AssetSpawner.Service
{
    public class AssetSpawnerService: IAssetSpawnerService
    {
        private Dictionary<string, AssetSpawnerInfo> _assetSpawnerInfos = new  Dictionary<string, AssetSpawnerInfo>();
        private const string DataKey = "AssetSpawner";
        
        public AssetSpawnerService()
        {
            _assetSpawnerInfos = FetchAssetInfo();
        }

        public void RegisterAsset(AssetSpawnerInfo assetSpawnerInfos)
        {
            Debug.Log($"AssetSpawnerService RegisterAsset {this.GetHashCode()}");
            _assetSpawnerInfos = FetchAssetInfo();
            
            if (_assetSpawnerInfos.TryGetValue(assetSpawnerInfos.Key, out _))
            {
                Debug.LogError($"[AssetSpawnerService] asset already exist: {assetSpawnerInfos.Key}");
                return;
            }
            
            _assetSpawnerInfos.Add(assetSpawnerInfos.Key, assetSpawnerInfos);
            SaveAssetInfo();
        }
        public void SpawnAsset(string assetKey, Transform container)
        {
            if (!_assetSpawnerInfos.TryGetValue(assetKey, out AssetSpawnerInfo assetSpawner))
            {
                Debug.LogError($"[AssetSpawnerService] cannot find: {assetKey}");
                return;
            }

            var prefabToLoad = Resources.Load($"{assetSpawner.Address}/{assetSpawner.Key}");
            var instance = Object.Instantiate(prefabToLoad, container);
            
            Debug.Log($"[AssetSpawnerService] {assetKey} spawn Successfully");               
        }


        private Dictionary<string, AssetSpawnerInfo> FetchAssetInfo()
        {
            var serializedData = PlayerPrefs.GetString(DataKey);
            Dictionary<string, AssetSpawnerInfo> data = new Dictionary<string, AssetSpawnerInfo>();
            
            if(string.IsNullOrEmpty(serializedData))
                return data;
            
            try
            {
                var assetList  = JsonUtility.FromJson<SerializedListWrapper>(serializedData);
                data = assetList.Assets.ToDictionary(x => x.Key, x => x);
            }
            catch (Exception e)
            {
                Debug.LogError($"[AssetSpawnerService] Cannot find data {e}");         
            }

            return data;
        }
        
        private void SaveAssetInfo()
        {
            try
            {
                var serializedListWrapper = new SerializedListWrapper
                {
                    Assets = _assetSpawnerInfos.Values.ToList()
                };
                var serializedData = JsonUtility.ToJson(serializedListWrapper);
                PlayerPrefs.SetString(DataKey, serializedData);
            }
            catch (Exception e)
            {
                Debug.LogError($"[AssetSpawnerService] Error on saving data {e}");         
            }
        }
        
        [Serializable]
        public class SerializedListWrapper
        {
            public List<AssetSpawnerInfo> Assets;
            
        }
    }
}
