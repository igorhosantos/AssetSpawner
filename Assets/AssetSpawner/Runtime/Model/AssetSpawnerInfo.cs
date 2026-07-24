using System;

namespace AssetSpawner.Runtime.Model
{
    [Serializable]
    public class AssetSpawnerInfo
    {
        public string AssetGUI;
        public string AssetBundleName;
        public string AssetName;

        public AssetSpawnerInfo(string assetGUI,string assetName, string assetBundleName)
        {
            this.AssetGUI = assetGUI;
            this.AssetName = assetName;
            this.AssetBundleName = assetBundleName;
        }
    }
}
