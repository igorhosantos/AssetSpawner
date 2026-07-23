
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AssetSpawner.Providers
{
    public interface IAssetProviderService
    {
        IEnumerator PrefetchAssets();
        IEnumerator FetchAllAssets(Action<Dictionary<string, GameObject>> callback);
        IEnumerator FetchAsset(string assetKey);
        IEnumerator GetAsset(string assetKey, Action<GameObject> callback);

        AssetLoadState LoadState { get; set; }

        public enum AssetLoadState
        {
            NotInitialized = 0,
            Loading = 1,
            Ready = 2,
            Failed = 3,
        }
    }
}
