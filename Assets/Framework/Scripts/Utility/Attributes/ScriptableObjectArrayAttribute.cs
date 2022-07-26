using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TutorialProject.Framework.Utility {
    /// <summary>
    /// Used on a ScriptableObject array field to be able to add or remove embedded scriptable objects that are derived class types of the array field type.
    /// </summary>
    /// <seealso cref="UnityEngine.PropertyAttribute" />
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class ScriptableObjectArrayAttribute : PropertyAttribute {
        public string addButtonText = "";
        public string removeButtonText = "";

        public ScriptableObjectArrayAttribute() {
            addButtonText = "";
            removeButtonText = "";
        }
        public ScriptableObjectArrayAttribute(string addText, string removeText) {
            addButtonText = addText;
            removeButtonText = removeText;
        }
    }

    namespace Editor {
#if UNITY_EDITOR
        [CustomPropertyDrawer(typeof(ScriptableObjectArrayAttribute), false)]
        public class ScriptableObjectArrayDrawer : PropertyDrawer {
            #region Declarations
            private class AddItemParams {
                public SerializedProperty Property { get; set; }
                public Type AddType { get; set; }

                public AddItemParams(SerializedProperty property, Type addType) {
                    Property = property;
                    AddType = addType;
                }
            }

            private Dictionary<Type, string> _scriptCache;
            #endregion

            #region Constructor
            /// <summary>
            /// Initializes the <see cref="ScriptableObjectArrayDrawer"/> class.
            /// </summary>
            static ScriptableObjectArrayDrawer() { }
            #endregion

            #region GUI Override
            /// <summary>
            /// Override this method to make your own IMGUI based GUI for the property.
            /// </summary>
            /// <param name="position">Rectangle on the screen to use for the property GUI.</param>
            /// <param name="property">The SerializedProperty to make the custom GUI for.</param>
            /// <param name="label">The label of this property.</param>
            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
                var attr = (ScriptableObjectArrayAttribute)attribute;
                string buttonText = "";

                if (!fieldInfo.FieldType.GetElementType().IsSubclassOf(typeof(ScriptableObject))) {
                    GUI.Label(position, "The ScriptableObjectArrayAttribute attribute can only be used on ScriptableObject objects");
                    return;
                }
                
                EditorGUI.BeginProperty(position, label, property);

                // Determine the text that will be on the button
                if (property.objectReferenceValue == null) {
                    // ADDING
                    buttonText = attr.addButtonText;
                    if (string.IsNullOrEmpty(buttonText)) {
                        buttonText = "Add";
                    }
                }
                else {
                    // REMOVING
                    buttonText = attr.removeButtonText;
                    if (string.IsNullOrEmpty(buttonText)) {
                        buttonText = "Remove";
                    }
                }

                // Calculate the sizing of everything
                var totalWidth = position.width;
                var buttonWidth = GUI.skin.button.CalcSize(new GUIContent(buttonText)).x;
                var propRect = new Rect(position.x, position.y, totalWidth - (buttonWidth + 5), position.height);
                var buttonRect = new Rect(position.x + totalWidth - buttonWidth, position.y, buttonWidth, position.height);

                // Now show the actual content
                EditorGUI.PropertyField(propRect, property, false);
                if (property.objectReferenceValue == null) {
                    // ADDING
                    if (GUI.Button(buttonRect, buttonText)) {
                        // Check to see if we need to build our cache or not
                        if (_scriptCache == null) {
                            BuildCache(fieldInfo.FieldType.GetElementType());
                        }

                        GenericMenu context = new GenericMenu();
                        foreach (var kvp in _scriptCache) {
                            context.AddItem(new GUIContent(kvp.Value), false, OnContextItemSelected, new AddItemParams(property, kvp.Key));
                        }
                        context.ShowAsContext();
                    }
                }
                else {
                    // REMOVING
                    if (GUI.Button(buttonRect, buttonText)) {
                        var asset = (ScriptableObject)property.objectReferenceValue;
                        Undo.RecordObject(property.serializedObject.targetObject, $"Deleted {fieldInfo.FieldType} Asset");
                        property.objectReferenceValue = null;
                        Undo.DestroyObjectImmediate(asset);
                        AssetDatabase.SaveAssets();
                    }
                }
                EditorGUI.EndProperty();
            }
            #endregion

            #region Private Methods
            /// <summary>
            /// Called when [context item selected].
            /// </summary>
            /// <param name="obj">The type.</param>
            private void OnContextItemSelected(object obj) {
                AddItemParams itemParams = obj as AddItemParams;

                var asset = ScriptableObject.CreateInstance(itemParams.AddType);
                asset.name = itemParams.AddType.Name;

                AssetDatabase.AddObjectToAsset(asset, itemParams.Property.serializedObject.targetObject);
                AssetDatabase.SaveAssets();
                itemParams.Property.objectReferenceValue = asset;
                itemParams.Property.serializedObject.ApplyModifiedProperties();
            }

            /// <summary>
            /// Builds the cache.
            /// </summary>
            /// <param name="type">The type.</param>
            private void BuildCache(Type type) {
                _scriptCache = new Dictionary<Type, string>();

                var types = TypeCache.GetTypesDerivedFrom(type).OrderBy(t => t.Name).ToArray();
                for (int x = 0; x < types.Length; x++) {
                    _scriptCache.Add(types[x], types[x].Name);
                }
            }
            #endregion
        }
#endif
    }
}
