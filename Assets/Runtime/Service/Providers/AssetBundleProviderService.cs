
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using AssetSpawner.Model;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AssetSpawner.Providers
{
    public class AssetBundleProviderService : IAssetProviderService
    {
        private readonly List<AssetBundle> _loadedBundles = new List<AssetBundle>();
        private static string basePath = $"{Application.streamingAssetsPath}/AssetBundles";
        private static string manifestBundlePath = Path.Combine(basePath, "AssetBundles");
        private Dictionary<string, GameObject> _loadedAssets = new Dictionary<string, GameObject>();
        public IAssetProviderService.AssetLoadState LoadState { get; set; } =
            IAssetProviderService.AssetLoadState.NotInitialized;
        
        public IEnumerator PrefetchAssets()
        {
            LoadState = IAssetProviderService.AssetLoadState.Loading;
            
            AssetBundleCreateRequest manifestLoadRequest = AssetBundle.LoadFromFileAsync(manifestBundlePath);
            
            yield return manifestLoadRequest;

            AssetBundle manifestBundle = manifestLoadRequest.assetBundle;
            if (manifestBundle == null)
            {
                SetLoadFailed($"[AssetSpawnerService] Failed to load main manifest bundle at: {manifestBundlePath}");
                yield break;
            }

            AssetBundleManifest manifest = manifestBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
            if (manifest == null)
            {
                manifestBundle.Unload(false);
                SetLoadFailed("[AssetSpawnerService] Failed to extract AssetBundleManifest object.");
                yield break;
            }

            string[] bundleNames = manifest.GetAllAssetBundles();
            Debug.Log($"[AssetSpawnerService] Found {bundleNames.Length} bundles to load.");

            foreach (string bundleName in bundleNames)
            {
                string individualBundlePath = Path.Combine(basePath, bundleName);
                AssetBundleCreateRequest bundleLoadRequest = AssetBundle.LoadFromFileAsync(individualBundlePath);
                yield return bundleLoadRequest;

                AssetBundle loadedBundle = bundleLoadRequest.assetBundle;
                if (loadedBundle != null)
                {
                    _loadedBundles.Add(loadedBundle);
                    Debug.Log($"[AssetSpawnerService] Successfully loaded bundle: {bundleName}");
                }
                else
                {
                    Debug.LogError($"[AssetSpawnerService] Failed to load bundle: {bundleName} at path {individualBundlePath}");
                }
            }

            manifestBundle.Unload(false);
        }
        
        public IEnumerator FetchAllAssets(Action<Dictionary<string, GameObject>> callback)
        {
            if (!TryLoadAssetManifest(basePath, out AssetManifest assetManifest))
            {
                SetLoadFailed(
                    $"[AssetSpawnerService] Missing or invalid asset manifest at '{Path.Combine(basePath, AssetManifest.FileName)}'. Rebuild AssetBundles from the editor.");
                
                yield break;
            }

            var prefabsByName = new Dictionary<string, GameObject>();

            foreach (AssetBundle loadedBundle in _loadedBundles)
            {
                AssetBundleRequest request = loadedBundle.LoadAllAssetsAsync<GameObject>();
                yield return request;

                foreach (Object asset in request.allAssets)
                {
                    if (asset is GameObject prefab)
                    {
                        if (!prefabsByName.TryAdd(prefab.name, prefab))
                        {
                            Debug.LogWarning(
                                $"[AssetSpawnerService] Duplicate prefab name '{prefab.name}' found while caching bundles.");
                        }
                    }
                }
            }

            foreach (AssetManifestEntry entry in assetManifest.Entries)
            {
                if (string.IsNullOrEmpty(entry.AssetKey))
                {
                    continue;
                }

                if (!prefabsByName.TryGetValue(entry.AssetName, out GameObject prefab))
                {
                    Debug.LogWarning(
                        $"[AssetSpawnerService] Manifest entry '{entry.AssetKey}' references missing prefab '{entry.AssetName}'.");
                    continue;
                }

                if (!_loadedAssets.TryAdd(entry.AssetKey, prefab))
                {
                    Debug.LogWarning(
                        $"[AssetSpawnerService] Duplicate AssetKey '{entry.AssetKey}' found in manifest.");
                }
            }

            if (_loadedAssets.Count == 0)
            {
                SetLoadFailed("No spawnable assets were cached from the asset manifest.");
                yield break;
            }

            Debug.Log($"All AssetBundles have been successfully cached ({_loadedAssets.Count} assets).");
            callback?.Invoke(_loadedAssets);
            LoadState = IAssetProviderService.AssetLoadState.Ready;
        }

        public IEnumerator FetchAsset(string assetKey)
        {
            if (!TryLoadAssetManifest(basePath, out AssetManifest assetManifest))
            {
                SetLoadFailed(
                    $"[AssetSpawnerService] Missing or invalid asset manifest at '{Path.Combine(basePath, AssetManifest.FileName)}'. Rebuild AssetBundles from the editor.");
                
                yield break;
            }
            
            //means the asset is already in cache to use
            if (_loadedAssets.TryGetValue(assetKey, out _))
            {
                yield return null;
            }

            //check the id of the bundle and access them in the _loadedBundles vars
            AssetBundle bundleToCheck = null;
            AssetManifestEntry assetManifestEntry = null;
            foreach (var entry in assetManifest.Entries)
            {
                //find the register on the manifest
                if (entry.AssetKey == assetKey)
                {
                    //get the bundle that is the asset is currently registered
                    string assetBundleName = entry.AssetBundleName;
                    foreach (var loadedBundle in _loadedBundles)
                    {
                        if (loadedBundle.name == assetBundleName)
                        {
                            bundleToCheck = loadedBundle;
                            assetManifestEntry = entry;
                            break;
                        }
                    }
                }
            }

            try
            {
                //load the bundle by the name 
                var asset = bundleToCheck.LoadAsset<GameObject>(assetManifestEntry.AssetName);
                _loadedAssets.Add(assetKey,asset);
            }
            catch (Exception e)
            {
                Debug.LogError($"[AssetSpawnerService] error on loading asset: {e}");
            }
          
        }

        public IEnumerator GetAsset(string assetKey,  Action<GameObject> callback)
        {
            try
            {
                _loadedAssets.TryGetValue(assetKey, out GameObject asset);
                callback?.Invoke(asset);
            }
            catch (Exception e)
            {
               Debug.LogError($"[AssetSpawnerService] Failed to load asset: {e}");
            }
            
            yield return null;
        }
        

        private static bool TryLoadAssetManifest(string basePath, out AssetManifest manifest)
        {
            manifest = null;
            string manifestPath = Path.Combine(basePath, AssetManifest.FileName);

            if (!File.Exists(manifestPath))
            {
                return false;
            }

            try
            {
                string json = File.ReadAllText(manifestPath);
                manifest = JsonUtility.FromJson<AssetManifest>(json);
                return manifest?.Entries != null && manifest.Entries.Length > 0;
            }
            catch (Exception e)
            {
                Debug.LogError($"[AssetSpawnerService] Failed to read asset manifest: {e}");
                return false;
            }
        }

        private void SetLoadFailed(string message)
        {
            Debug.LogError($"[AssetSpawnerService] {message}");
            LoadState = IAssetProviderService.AssetLoadState.Failed;
        }
    }
}
