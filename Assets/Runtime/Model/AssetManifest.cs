using System;

namespace AssetSpawner.Model
{
    [Serializable]
    public class AssetManifestEntry
    {
        public string AssetKey;
        public string AssetName;
        public string AssetBundleName;
    }

    [Serializable]
    public class AssetManifest
    {
        public const string FileName = "asset_manifest.json";

        public AssetManifestEntry[] Entries;
    }
}
