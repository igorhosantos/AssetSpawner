
using System.Collections;
using UnityEngine;

namespace AssetSpawner.Service
{
    public interface IAssetSpawnerService
    {
        IEnumerator LoadAllBundlesRoutine();
        IEnumerator SpawnAsset(string assetKey, Transform container);
    }
}
