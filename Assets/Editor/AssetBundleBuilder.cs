#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using AssetSpawner.Model;
using UnityEditor;
using UnityEngine;


namespace AssetSpawner.Editor
{
    public static class AssetBundleBuilder
    {
        private const string GenericGroupBundleName = "generic_group_bundle";
        
        public static AssetSpawnerInfo ExtractData(GameObject prefab)
        {
            //get the current asset path of the prefab
            string assetPath = AssetDatabase.GetAssetPath(prefab);
        
            //convert path to persistent GUID
            string guid = AssetDatabase.AssetPathToGUID(assetPath);
            
            // Access the AssetBundle property
            string assetBundleName = AssetDatabase.GetAssetPath(prefab);
            
            // Or alternatively, use AssetImporter to check the assigned bundle name directly
            AssetImporter importer = AssetImporter.GetAtPath(assetBundleName);

            //if it's not set, set a new one dynamically
            if (string.IsNullOrEmpty(importer.assetBundleName))
            {
                importer.SetAssetBundleNameAndVariant(GenericGroupBundleName, "");
            }
            
            Debug.Log($"AssetBundle Name: {importer.assetBundleName}");
            
            return new AssetSpawnerInfo(guid, prefab.name, importer.assetBundleName);
        }
        
        [MenuItem("AssetSpawner/Build AssetBundles")]
        public static void BuildAllAssetBundles()
        {
            Debug.Log("AssetBundleBuilder BuildAllAssetBundles");
            
            string assetBundleDirectory =  $"{Application.streamingAssetsPath}/AssetBundles";
            try
            {
                //ensure the directory exists
                if (!Directory.Exists(assetBundleDirectory))
                {
                    Directory.CreateDirectory(assetBundleDirectory);
                }

                // Build all AssetBundles and place them in the specified directory.
                BuildPipeline.BuildAssetBundles(assetBundleDirectory,
                    BuildAssetBundleOptions.None,
                    EditorUserBuildSettings.activeBuildTarget);

                WriteAssetManifest(assetBundleDirectory);
                AssetDatabase.Refresh();
            }
            catch (Exception e)
            {
                Debug.LogError($"Error during building asset bundles: {e}" );
            }
        }

        private static void WriteAssetManifest(string assetBundleDirectory)
        {
            var entries = new List<AssetManifestEntry>();

            foreach (string bundleName in AssetDatabase.GetAllAssetBundleNames())
            {
                foreach (string assetPath in AssetDatabase.GetAssetPathsFromAssetBundle(bundleName))
                {
                    if (!assetPath.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                    if (prefab == null)
                    {
                        continue;
                    }

                    entries.Add(new AssetManifestEntry
                    {
                        AssetKey = AssetDatabase.AssetPathToGUID(assetPath),
                        AssetName = prefab.name,
                        AssetBundleName = bundleName
                    });
                }
            }

            var manifest = new AssetManifest { Entries = entries.ToArray() };
            string manifestPath = Path.Combine(assetBundleDirectory, AssetManifest.FileName);
            File.WriteAllText(manifestPath, JsonUtility.ToJson(manifest, true));
            Debug.Log($"Wrote asset manifest with {entries.Count} entries to {manifestPath}");
        }
    }
}

#endif
