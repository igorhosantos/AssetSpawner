

using AssetSpawner.Service;
using UnityEngine;

namespace AssetSpawner.Providers
{
    public static class AssetProvidersFactory
    {
        public static IAssetProviderService CreateProvider(AssetSpawnerSettings.Providers providerType)
        {
            switch (providerType)
            {
                case AssetSpawnerSettings.Providers.StandardAssetBundle:
                    return new AssetBundleProviderService();
                default:
                    Debug.LogError($"[AssetProvidersFactory] Unknown provider type: {providerType}");
                   return null;
            }
        }
    }
}
