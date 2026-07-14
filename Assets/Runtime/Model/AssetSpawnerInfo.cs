using System;

namespace AssetSpawner.Model
{
    [Serializable]
    public class AssetSpawnerInfo
    {
        public string AssetKey;
        public string AssetBundleName;
        public string AssetName;

        public AssetSpawnerInfo(string assetKey,string assetName, string assetBundleName)
        {
            this.AssetKey = assetKey;
            this.AssetName = assetName;
            this.AssetBundleName = assetBundleName;
        }
    }
}
