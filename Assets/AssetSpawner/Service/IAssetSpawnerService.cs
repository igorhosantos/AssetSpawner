
using AssetSpawner.Model;
using UnityEngine;

namespace AssetSpawner.Service
{
    public interface IAssetSpawnerService
    {
        void RegisterAsset(AssetSpawnerInfo assetSpawnerInfos);
        void SpawnAsset(string assetKey, Transform container);
    }
}
