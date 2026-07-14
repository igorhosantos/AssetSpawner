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
            if (string.IsNullOrEmpty(AssetKey))
            {
                Debug.LogError($"[RuntimeAssetSpawner] AssetKey is empty on '{name}'. Assign a prefab in the inspector.", this);
                yield break;
            }

            yield return _assetSpawnerService.SpawnAsset(AssetKey, transform);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (prefabRefToSpawn != null && !Application.isPlaying)
            {
                UpdateAssetReference();
            }
        }

        private void UpdateAssetReference()
        {
            Debug.Log("RuntimeAssetSpawner UpdateAssetReference");

            AssetSpawnerInfo info = AssetBundleBuilder.ExtractData(prefabRefToSpawn);
            AssetKey = info.AssetKey;
            AssetName = info.AssetName;
            AssetBundleName = info.AssetBundleName;
            prefabRefToSpawn = null;
        }
#endif
        
    }
}
