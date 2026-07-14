#if UNITY_EDITOR
using AssetSpawner.Editor;
#endif
using System.Collections;
using AssetSpawner.Model;
using AssetSpawner.Service;
using UnityEngine;
using Utils;


namespace AssetSpawner
{

    public class RuntimeAssetSpawner : MonoBehaviour
    {
        public GameObject prefabRefToSpawn;
        
        //TODO inject using service locator
        private IAssetSpawnerService _assetSpawnerService;
        
#if UNITY_EDITOR
        private AssetBundleBuilder _assetBundleBuilder;
#endif
        public string AssetKey;
        public string AssetName;
        public string AssetBundleName;
        
        private void Start()
        {
            _assetSpawnerService = ServiceLocator.Get<IAssetSpawnerService>();
            StartCoroutine(ProcessSpawn());
        }

        private IEnumerator ProcessSpawn()
        {
            yield return _assetSpawnerService.SpawnAsset(AssetName, this.transform);
        }

        private void OnValidate()
        {
            if(prefabRefToSpawn)
            {
                UpdateAssetReference();
            }
        }

        private void UpdateAssetReference()
        {
#if UNITY_EDITOR
            Debug.Log("RuntimeAssetSpawner UpdateAssetReference");
            
            _assetBundleBuilder ??= new AssetBundleBuilder();
            AssetSpawnerInfo info = _assetBundleBuilder.ExtractData(prefabRefToSpawn);
            AssetKey = info.AssetKey;
            AssetName = info.AssetName;
            AssetBundleName = info.AssetBundleName;
            prefabRefToSpawn = null;
#endif

        }
    }
}
