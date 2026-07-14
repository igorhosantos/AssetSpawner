using System.Collections;
using System.Collections.Generic;
using System.IO;
using AssetSpawner.Model;
using UnityEngine;


namespace AssetSpawner.Service
{
    public class AssetSpawnerService: IAssetSpawnerService
    {
        private Dictionary<string, AssetSpawnerInfo> _assetSpawnerInfos = new  Dictionary<string, AssetSpawnerInfo>();
        private List<AssetBundle> _loadedBundles = new List<AssetBundle>();
        private Dictionary<string,GameObject> _allGameObjects = new Dictionary<string, GameObject>();
        
        private bool _assetsAreReady = false;
        
        public IEnumerator SpawnAsset(string assetKey, Transform container)
        {
            yield return new WaitUntil(() => _assetsAreReady);
            
            Debug.Log($"[AssetSpawnerService] try spawn: {assetKey}"); 
            
            var instance = Object.Instantiate(_allGameObjects[assetKey], container);
            
            Debug.Log($"[AssetSpawnerService] {assetKey} spawn Successfully");  
            yield break;
        }
        
       public IEnumerator LoadAllBundlesRoutine()
       {
            // 1. Determine the correct path according to the running platform
            string basePath = $"{Application.streamingAssetsPath}/AssetBundles";
            
            // 2. Load the main manifest bundle first
            string manifestBundlePath = Path.Combine(basePath, "AssetBundles");
            AssetBundleCreateRequest manifestLoadRequest = AssetBundle.LoadFromFileAsync(manifestBundlePath);
            yield return manifestLoadRequest;

            AssetBundle manifestBundle = manifestLoadRequest.assetBundle;
            if (manifestBundle == null)
            {
                Debug.LogError($"Failed to load main manifest bundle at: {manifestBundlePath}");
                yield break;
            }

            // 3. Extract the AssetBundleManifest object from the bundle
            AssetBundleManifest manifest = manifestBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
            if (manifest == null)
            {
                Debug.LogError("Failed to extract AssetBundleManifest object.");
                manifestBundle.Unload(false);
                yield break;
            }

            // 4. Get the names of all built bundles listed inside the manifest
            string[] bundleNames = manifest.GetAllAssetBundles();
            Debug.Log($"Found {bundleNames.Length} bundles to load.");

            // 5. Iterate and load each individual AssetBundle
            foreach (string bundleName in bundleNames)
            {
                string individualBundlePath = Path.Combine(basePath, bundleName);
                AssetBundleCreateRequest bundleLoadRequest = AssetBundle.LoadFromFileAsync(individualBundlePath);
                yield return bundleLoadRequest;

                AssetBundle loadedBundle = bundleLoadRequest.assetBundle;
                if (loadedBundle != null)
                {
                    _loadedBundles.Add(loadedBundle);
                    Debug.Log($"Successfully loaded bundle: {bundleName}");
                }
                else
                {
                    Debug.LogError($"Failed to load bundle: {bundleName} at path {individualBundlePath}");
                }
            }

            // 6. Unload the manifest container bundle (keep loaded assets intact) to save memory
            manifestBundle.Unload(false);
            
            yield return OnAllBundlesLoaded();
        }

        private IEnumerator OnAllBundlesLoaded()
        {
            IEnumerable<AssetBundle> loadedBundles = AssetBundle.GetAllLoadedAssetBundles();

            Debug.Log("--- Currently Loaded AssetBundles ---");
            foreach (AssetBundle loadedBundle in loadedBundles)
            {
                Debug.Log(loadedBundle.name);
                AssetBundleRequest request = loadedBundle.LoadAllAssetsAsync<GameObject>();
                yield return request; // or await request;
                foreach (Object asset in request.allAssets)
                {
                    if (asset is GameObject prefab)
                    {
                        Debug.Log($"--- Found Prefab: {prefab.name}");
                        _allGameObjects.Add(prefab.name, prefab);
                    }
                }
            }
            
            Debug.Log("All AssetBundles have been successfully cached!!");
            _assetsAreReady = true;
        }
    }
}
