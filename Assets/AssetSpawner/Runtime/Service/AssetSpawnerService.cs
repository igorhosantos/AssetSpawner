using System;
using System.Collections;
using System.Collections.Generic;
using AssetSpawner.Providers;
using UnityEngine;
using Object = UnityEngine.Object;


namespace AssetSpawner.Service
{
    public class AssetSpawnerService : IAssetSpawnerService
    {
        private Dictionary<string, GameObject> _allGameObjects = new Dictionary<string, GameObject>();
        private AssetSpawnerSettings _settings;
        private IAssetProviderService _assetProvider;
        
        public AssetSpawnerService(AssetSpawnerSettings settings)
        {
            Debug.Log($"[AssetSpawnerService] Initialized: {settings.ProviderType} - {settings.LoadingType}");
            _settings = settings;
            _assetProvider = AssetProvidersFactory.CreateProvider(settings);
        }
        public IEnumerator SpawnAsset(string assetKey, Transform container)
        {
            yield return new WaitUntil(() => _assetProvider.LoadState != IAssetProviderService.AssetLoadState.Loading);

            if (_assetProvider.LoadState == IAssetProviderService.AssetLoadState.Failed)
            {
                Debug.LogError($"[AssetSpawnerService] Cannot spawn '{assetKey}': asset loading failed.");
                yield break;
            }

            switch (_settings.LoadingType)
            {
                case AssetSpawnerSettings.LoadingTypes.LoadAndCacheAll:
                    yield return SpawnAssetFromCache(assetKey,container);
                    break;
                case AssetSpawnerSettings.LoadingTypes.OnDemand:
                    yield return LoadAndSpawnAsset(assetKey,container);
                    break;
                default:
                    Debug.LogError($"[AssetSpawnerService] unexpected loading type: {_settings.LoadingType}");
                    break;
            }
           
        }

        private IEnumerator SpawnAssetFromCache(string assetKey, Transform container)
        {
            try
            {
                if (!_allGameObjects.TryGetValue(assetKey, out GameObject gameObject))
                {
                    Debug.LogError($"[AssetSpawnerService] '{assetKey}' not found.");
                    yield break;
                }
                
                Object.Instantiate(gameObject, container);
                Debug.Log($"[AssetSpawnerService] '{assetKey}' spawned successfully.");
            }
            catch (Exception e)
            {
                Debug.LogError($"[AssetSpawnerService] Error during spawn asset: {e}");
            }
        }
        
        private IEnumerator LoadAndSpawnAsset(string assetGUI, Transform container)
        {
           yield return _assetProvider.FetchAsset(assetGUI);
           yield return _assetProvider.GetAsset(assetGUI, result =>
           {
               Object.Instantiate(result, container);
               Debug.Log($"[AssetSpawnerService] '{assetGUI}' spawned successfully.");
           });
        }

        public IEnumerator InitializeService()
        {
            yield return _assetProvider.PrefetchAssets();

            //optional layer to load all the game object elements and keep each type in cache
            if (_settings.LoadingType == AssetSpawnerSettings.LoadingTypes.LoadAndCacheAll)
            {
                yield return _assetProvider.FetchAllAssets(result =>
                {
                    _allGameObjects = result;
                });
            }
            else
            {
                _assetProvider.LoadState = IAssetProviderService.AssetLoadState.Ready;
            }
        }
    }
}
