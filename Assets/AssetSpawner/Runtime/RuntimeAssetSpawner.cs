using System.Collections;
using AssetSpawner.Runtime.Model;
using AssetSpawner.Service;
using UnityEngine;
using Utils;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AssetSpawner
{

    public class RuntimeAssetSpawner : MonoBehaviour
    {
        private const string GenericGroupBundleName = "generic_group_bundle";
        
        [Header("Drag N Drop Here the prefab you want to spawn")]
        public GameObject prefabRefToSpawn;
        
        //injected by using service locator
        private IAssetSpawnerService _assetSpawnerService;

        [Header("After Drop the prefab, all the properties will refresh")]
        public string AssetGUI;
        public string AssetName;
        public string AssetBundleName;
        
        private void Start()
        {
            _assetSpawnerService = ServiceLocator.Get<IAssetSpawnerService>();
            StartCoroutine(ProcessSpawn());
        }

        private IEnumerator ProcessSpawn()
        {
            if (string.IsNullOrEmpty(AssetGUI))
            {
                Debug.LogError($"[RuntimeAssetSpawner] AssetKey is empty on '{name}'. Assign a prefab in the inspector.", this);
                yield break;
            }

            yield return _assetSpawnerService.SpawnAsset(AssetGUI, transform);
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

            AssetSpawnerInfo info = ExtractData(prefabRefToSpawn);
            AssetGUI = info.AssetGUI;
            AssetName = info.AssetName;
            AssetBundleName = info.AssetBundleName;
            prefabRefToSpawn = null;
        }
        
        public static AssetSpawnerInfo ExtractData(GameObject prefab)
        {
            //get the current asset path of the prefab
            string assetPath = AssetDatabase.GetAssetPath(prefab);
        
            //convert path to persistent GUID
            string guid = AssetDatabase.AssetPathToGUID(assetPath);
            
            // Access the AssetBundle property
            string assetBundleName = AssetDatabase.GetAssetPath(prefab);
            
            // Or alternatively, use AssetImporter to check the assigned bundle name directly
            AssetImporter importer = AssetImporter.GetAtPath(assetBundleName);

            //if it's not set, set a new one dynamically
            if (string.IsNullOrEmpty(importer.assetBundleName))
            {
                importer.SetAssetBundleNameAndVariant(GenericGroupBundleName, "");
            }
            
            Debug.Log($"AssetBundle Name: {importer.assetBundleName}");
            
            return new AssetSpawnerInfo(guid, prefab.name, importer.assetBundleName);
        }
#endif
        
    }
}
