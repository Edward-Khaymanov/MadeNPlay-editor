#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEngine;

namespace MadeNPlayShared
{
    public class MadeNPlayInstaller
    {
        [MenuItem("Window/MadeNPlay/Setup")]
        public static void Setup()
        {
            SetLayers();
            Debug.Log($"{nameof(MadeNPlayInstaller)}: Layers are initialized");

            ExternalResourceLoader.EditorPlugin.Init();
            var addressableSettings = AddressableAssetSettingsDefaultObject.GetSettings(true);
            addressableSettings.AddLabel(CONSTANTS.NETWORK_PREFAB_LABEL);
            addressableSettings.AddLabel(CONSTANTS.GAME_HANDLER_LABEL);
            Debug.Log($"{nameof(MadeNPlayInstaller)}: Addressable is initialized");

            NetworkIdGenerator.Validate();
            Debug.Log($"{nameof(MadeNPlayInstaller)}: Network ids added");
        }

        private static void SetLayers()
        {
            var tagManager = new SerializedObject(AssetDatabase.LoadAssetAtPath<Object>("ProjectSettings/TagManager.asset"));

            var layers = tagManager.FindProperty("layers");
            if (layers == null || !layers.isArray)
            {
                Debug.LogWarning("Can't set up the layers.  It's possible the format of the layers and tags data has changed in this version of Unity.");
                return;
            }

            var startLayerIndex = 10;
            var maximumLayers = 31;
            for (int i = startLayerIndex; i < maximumLayers; i++)
            {
                var layerProperty = layers.GetArrayElementAtIndex(i);
                layerProperty.stringValue = CONSTANTS.Layers[i - startLayerIndex];
            }

            tagManager.ApplyModifiedProperties();
        }
    }
}
#endif