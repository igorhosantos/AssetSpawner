#if UNITY_EDITOR
using System;
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
                
                AssetDatabase.Refresh();
            }
            catch (Exception e)
            {
                Debug.LogError($"Error during building asset bundles: {e}" );
            }
        }
    }
}

#endif
