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

