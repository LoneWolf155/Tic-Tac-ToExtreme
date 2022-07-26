#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

public static class CustomAssetUtil {
    /// <summary>
    /// Creates a custom asset file based on the specified type.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static T CreateAsset<T>(string assetName = "", bool autoFocus = true, string path = "") where T : ScriptableObject {
        // Create an instance for the asset
        T asset = ScriptableObject.CreateInstance<T>();

        if (string.IsNullOrEmpty(path)) {
            // Get the path of the currently selected asset
            path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (path == "") { path = "Assets"; }
            else if (Path.GetExtension(path) != "") {
                path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
            }
        }
        
        // Build the full path we will be saving the asset to
        if (string.IsNullOrEmpty(assetName)) {
            assetName = AssetDatabase.GenerateUniqueAssetPath(path + "/New " + typeof(T).ToString() + ".asset");
        }
        else {
            assetName = AssetDatabase.GenerateUniqueAssetPath(path + "/" + assetName + ".asset");
        }

        // Generate and save the asset
        AssetDatabase.CreateAsset(asset, assetName);
        AssetDatabase.SaveAssets();

        if (autoFocus) {
            // Focus on the newly created asset
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
        }
        return asset;
    }
}
#endif