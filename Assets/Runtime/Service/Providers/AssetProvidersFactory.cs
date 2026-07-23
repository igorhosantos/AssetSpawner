

using AssetSpawner.Service;
using UnityEngine;

namespace AssetSpawner.Providers
{
    public static class AssetProvidersFactory
    {
        public static IAssetProviderService CreateProvider(AssetSpawnerSettings settings)
        {
            AssetSpawnerSettings.Providers providerType = settings.ProviderType;
            
            switch (providerType)
            {
                case AssetSpawnerSettings.Providers.StandardAssetBundle:
                    return new AssetBundleProviderService(settings);
                default:
                    Debug.LogError($"[AssetProvidersFactory] Unknown provider type: {providerType}");
                   return null;
            }
        }
    }
}
