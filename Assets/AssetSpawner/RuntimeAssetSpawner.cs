using AssetSpawner.Model;
using AssetSpawner.Service;
using UnityEngine;

namespace AssetSpawner
{

    public class RuntimeAssetSpawner : MonoBehaviour
    {
        public GameObject prefabRefToSpawn;
        
        //TODO inject using service locator
        private IAssetSpawnerService _assetSpawnerService;
        public string AssetKey;
        
        void Awake()
        {
           
          
        }
        
        private void Start()
        {
            _assetSpawnerService ??= new AssetSpawnerService();

            //instantiate
            _assetSpawnerService.SpawnAsset(AssetKey, this.transform);
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
            Debug.Log("RuntimeAssetSpawner UpdateAssetReference");

            _assetSpawnerService ??= new AssetSpawnerService();
            
            AssetSpawnerInfo assetSpawnerInfo = new AssetSpawnerInfo();
            assetSpawnerInfo.SetData(prefabRefToSpawn);
            _assetSpawnerService.RegisterAsset(assetSpawnerInfo);
            AssetKey = assetSpawnerInfo.Key;
            prefabRefToSpawn = null;

        }
    }
}
