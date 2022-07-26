using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TutorialProject.Framework.Utility {
    /// <summary>
    /// Used on a ScriptableObject field to add a button that adds/removes an embedded instance of the object on the asset.
    /// </summary>
    /// <seealso cref="UnityEngine.PropertyAttribute" />
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class ScriptableObjectToggleAttribute : PropertyAttribute {
        public string addButtonText = "";
        public string removeButtonText = "";
        public string assetName = "";

        public ScriptableObjectToggleAttribute() {
            addButtonText = "";
            removeButtonText = "";
            assetName = "";
        }
        public ScriptableObjectToggleAttribute(string addText, string removeText) {
            addButtonText = addText;
            removeButtonText = removeText;
            assetName = "";
        }
        public ScriptableObjectToggleAttribute(string addText, string removeText, string assetName) {
            this.addButtonText = addText;
            this.removeButtonText = removeText;
            this.assetName = assetName;
        }
    }

    namespace Editor {
#if UNITY_EDITOR
        [CustomPropertyDrawer(typeof(ScriptableObjectToggleAttribute), false)]
        public class ScriptableObjectToggleDrawer : PropertyDrawer {
            #region Declarations
            #endregion

            #region Constructor
            /// <summary>
            /// Initializes the <see cref="ScriptableObjectToggleDrawer"/> class.
            /// </summary>
            static ScriptableObjectToggleDrawer() { }
            #endregion

            #region GUI Override
            /// <summary>
            /// Override this method to make your own IMGUI based GUI for the property.
            /// </summary>
            /// <param name="position">Rectangle on the screen to use for the property GUI.</param>
            /// <param name="property">The SerializedProperty to make the custom GUI for.</param>
            /// <param name="label">The label of this property.</param>
            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
                var attr = (ScriptableObjectToggleAttribute)attribute;
                
                if (fieldInfo.FieldType.IsSubclassOf(typeof(ScriptableObject))) {
                    string buttonText;

                    // Allow the creation or deletion of an instance
                    if (property.objectReferenceValue == null) {
                        if (string.IsNullOrEmpty(attr.addButtonText)) { buttonText = $"Add {fieldInfo.FieldType}"; }
                        else { buttonText = attr.addButtonText; }

                        if (GUI.Button(position, buttonText)) {
                            var asset = ScriptableObject.CreateInstance(fieldInfo.FieldType);
                            if (string.IsNullOrEmpty(attr.assetName)) {
                                asset.name = asset.GetType().Name;
                            }
                            else {
                                asset.name = attr.assetName;
                            }

                            AssetDatabase.AddObjectToAsset(asset, property.serializedObject.targetObject);
                            AssetDatabase.SaveAssets();
                            property.objectReferenceValue = asset;
                        }
                    }
                    else {
                        if (string.IsNullOrEmpty(attr.removeButtonText)) { buttonText = $"Remove {fieldInfo.FieldType}"; }
                        else { buttonText = attr.removeButtonText; }

                        if (GUI.Button(position, buttonText)) {
                            var asset = (ScriptableObject)property.objectReferenceValue;
                            Undo.RecordObject(property.serializedObject.targetObject, $"Deleted {fieldInfo.FieldType} Asset");
                            property.objectReferenceValue = null;
                            Undo.DestroyObjectImmediate(asset);
                            AssetDatabase.SaveAssets();
                        }
                    }
                }
                else {
                    GUI.Label(position, "The ToggleEmbeddedAsset attribute can only be used on ScriptableObject fields");
                }
            }
            #endregion
        }
#endif
    }
}
