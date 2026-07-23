using AssetSpawner.Model;
using AssetSpawner.Service;
using UnityEngine;

namespace Utils
{
    public class ServiceLocatorInitializer: MonoBehaviour
    {
        [SerializeField] private AssetSpawnerSettingsScriptableObject  assetSpawnerSettings;
        private void Awake()
        {
            IAssetSpawnerService assetSpawnerService = new AssetSpawnerService(assetSpawnerSettings.Settings);
            StartCoroutine(assetSpawnerService.InitializeService());
            
            ServiceLocator.Register(assetSpawnerService);
        }
    }
}
