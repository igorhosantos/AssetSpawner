
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using AssetSpawner.Model;
using AssetSpawner.Service;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AssetSpawner.Providers
{
    public class AssetBundleProviderService : IAssetProviderService
    {
        public IAssetProviderService.AssetLoadState LoadState { get; set; } =
            IAssetProviderService.AssetLoadState.NotInitialized;
        private readonly List<AssetBundle> _loadedBundles = new List<AssetBundle>();
        private Dictionary<string, GameObject> _loadedAssets = new Dictionary<string, GameObject>();
        private string _basePath;
        private string _manifestBundlePath;
        private AssetSpawnerSettings _settings;
        public AssetBundleProviderService(AssetSpawnerSettings settings)
        {
            _settings = settings;
            _basePath = $"{Application.streamingAssetsPath}/{settings.StreamingAssetPath}";
            _manifestBundlePath = Path.Combine(_basePath, settings.StreamingAssetPath);
        }
        public IEnumerator PrefetchAssets()
        {
            LoadState = IAssetProviderService.AssetLoadState.Loading;
            
            AssetBundleCreateRequest manifestLoadRequest = AssetBundle.LoadFromFileAsync(_manifestBundlePath);
            
            yield return manifestLoadRequest;

            AssetBundle manifestBundle = manifestLoadRequest.assetBundle;
            if (manifestBundle == null)
            {
                SetLoadFailed($"[AssetBundleProviderService] Failed to load main manifest bundle at: {_manifestBundlePath}");
                yield break;
            }

            AssetBundleManifest manifest = manifestBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
            if (manifest == null)
            {
                manifestBundle.Unload(false);
                SetLoadFailed("[AssetBundleProviderService] Failed to extract AssetBundleManifest object.");
                yield break;
            }

            string[] bundleNames = manifest.GetAllAssetBundles();
            Debug.Log($"[AssetBundleProviderService] Found {bundleNames.Length} bundles to load.");

            foreach (string bundleName in bundleNames)
            {
                string individualBundlePath = Path.Combine(_basePath, bundleName);
                AssetBundleCreateRequest bundleLoadRequest = AssetBundle.LoadFromFileAsync(individualBundlePath);
                yield return bundleLoadRequest;

                AssetBundle loadedBundle = bundleLoadRequest.assetBundle;
                if (loadedBundle != null)
                {
                    _loadedBundles.Add(loadedBundle);
                    Debug.Log($"[AssetBundleProviderService] Successfully loaded bundle: {bundleName}");
                }
                else
                {
                    Debug.LogError($"[AssetBundleProviderService] Failed to load bundle: {bundleName} at path {individualBundlePath}");
                }
            }

            manifestBundle.Unload(false);
        }
        
        public IEnumerator FetchAllAssets(Action<Dictionary<string, GameObject>> callback)
        {
            if (!TryLoadAssetManifest(_basePath, out AssetManifest assetManifest))
            {
                SetLoadFailed(
                    $"[AssetBundleProviderService] Missing or invalid asset manifest at '{Path.Combine(_basePath, AssetManifest.FileName)}'. Rebuild AssetBundles from the editor.");
                
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
                                $"[AssetBundleProviderService] Duplicate prefab name '{prefab.name}' found while caching bundles.");
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
                        $"[AssetBundleProviderService] Manifest entry '{entry.AssetKey}' references missing prefab '{entry.AssetName}'.");
                    continue;
                }

                if (!_loadedAssets.TryAdd(entry.AssetKey, prefab))
                {
                    Debug.LogWarning(
                        $"[AssetBundleProviderService] Duplicate AssetKey '{entry.AssetKey}' found in manifest.");
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
            if (!TryLoadAssetManifest(_basePath, out AssetManifest assetManifest))
            {
                SetLoadFailed(
                    $"[AssetBundleProviderService] Missing or invalid asset manifest at '{Path.Combine(_basePath, AssetManifest.FileName)}'. Rebuild AssetBundles from the editor.");
                
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
                if (bundleToCheck != null)
                {
                    var asset = bundleToCheck.LoadAsset<GameObject>(assetManifestEntry.AssetName);
                    _loadedAssets.Add(assetKey,asset);
                }
                else
                {
                    Debug.LogError($"[AssetBundleProviderService] invalid bundle");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[AssetBundleProviderService] error on loading asset: {e}");
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
               Debug.LogError($"[AssetBundleProviderService] Failed to load asset: {e}");
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
                Debug.LogError($"[AssetBundleProviderService] Failed to read asset manifest: {e}");
                return false;
            }
        }

        private void SetLoadFailed(string message)
        {
            Debug.LogError($"[AssetBundleProviderService] {message}");
            LoadState = IAssetProviderService.AssetLoadState.Failed;
        }
    }
}
