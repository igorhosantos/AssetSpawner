using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace AssetSpawner.Editor
{

    [CustomEditor(typeof(RuntimeAssetSpawner))]
    public class RuntimeAssetSpawnerDrawer: UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            VisualElement myInspector = new VisualElement();

            var title = new Label("Runtime Asset Spawner")
            {
                style =
                {
                    color = Color.red,
                    fontSize = 20,
                    unityTextAlign =  TextAnchor.MiddleCenter,
                }
            };
            
            myInspector.Add(title);

            InspectorElement.FillDefaultInspector(myInspector, serializedObject, this);
            
            var buildAssetBundleButton = new Button
            {
                text = $"Update AssetBundles"
            };
            buildAssetBundleButton.clickable.clicked += AssetBundleBuilder.BuildAllAssetBundles;
            myInspector.Add(buildAssetBundleButton);

            // Return the finished Inspector UI.
            return myInspector;
        }
    }
}
