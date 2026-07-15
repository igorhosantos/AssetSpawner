using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using AssetSpawner.Model;
using UnityEngine;
using Object = UnityEngine.Object;


namespace AssetSpawner.Service
{
    public class AssetSpawnerService : IAssetSpawnerService
    {
        private readonly List<AssetBundle> _loadedBundles = new List<AssetBundle>();
        private readonly Dictionary<string, GameObject> _allGameObjects = new Dictionary<string, GameObject>();
        private IAssetSpawnerService.LoadState _loadState = IAssetSpawnerService.LoadState.Loading;

        public IEnumerator SpawnAsset(string assetKey, Transform container)
        {
            yield return new WaitUntil(() => _loadState != IAssetSpawnerService.LoadState.Loading);

            if (_loadState == IAssetSpawnerService.LoadState.Failed)
            {
                Debug.LogError($"[AssetSpawnerService] Cannot spawn '{assetKey}': asset loading failed.");
                yield break;
            }

            try
            {
                if (!_allGameObjects.TryGetValue(assetKey, out GameObject gameObject))
                {
                    Debug.LogError($"[AssetSpawnerService] '{assetKey}' not found.");
                    yield break;
                }

                //TODO implement object pooling
                Object.Instantiate(gameObject, container);
                Debug.Log($"[AssetSpawnerService] '{assetKey}' spawned successfully.");
            }
            catch (Exception e)
            {
                Debug.LogError($"[AssetSpawnerService] Error during spawn asset: {e}");
            }
        }

        public IEnumerator LoadAllBundlesRoutine()
        {
            string basePath = $"{Application.streamingAssetsPath}/AssetBundles";
            string manifestBundlePath = Path.Combine(basePath, "AssetBundles");
            
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

            //optional layer to load all the game object elements and keep each type in cache
            yield return LoadAndCachePerType(basePath);
        }

        private IEnumerator LoadAndCachePerType(string basePath)
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

                if (!_allGameObjects.TryAdd(entry.AssetKey, prefab))
                {
                    Debug.LogWarning(
                        $"[AssetSpawnerService] Duplicate AssetKey '{entry.AssetKey}' found in manifest.");
                }
            }

            if (_allGameObjects.Count == 0)
            {
                SetLoadFailed("No spawnable assets were cached from the asset manifest.");
                yield break;
            }

            Debug.Log($"All AssetBundles have been successfully cached ({_allGameObjects.Count} assets).");
            _loadState = IAssetSpawnerService.LoadState.Ready;
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
            _loadState = IAssetSpawnerService.LoadState.Failed;
        }
    }
}
