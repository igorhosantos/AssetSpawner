using AssetSpawner.Service;
using UnityEngine;

namespace Utils
{
    public class ServiceLocatorInitializer: MonoBehaviour
    {
        private void Awake()
        {
            IAssetSpawnerService assetSpawnerService = new AssetSpawnerService();
            StartCoroutine(assetSpawnerService.LoadAllBundlesRoutine());
            
            ServiceLocator.Register(assetSpawnerService);
        }
    }
}
