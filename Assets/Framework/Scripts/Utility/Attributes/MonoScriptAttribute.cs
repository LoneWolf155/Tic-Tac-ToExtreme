using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TutorialProject.Framework.Utility {
    /// <summary>
    /// Can be attached to a string to allow the user to choose a MonoBheavior script
    /// to be referenced in the project.
    /// </summary>
    /// <seealso cref="UnityEngine.PropertyAttribute" />
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class MonoScriptAttribute : PropertyAttribute {
        public Type type;
    }

    namespace Editor {
#if UNITY_EDITOR
        [CustomPropertyDrawer(typeof(MonoScriptAttribute), false)]
        public class MonoScriptPropertyDrawer : PropertyDrawer {
            #region Declarations
            private static Dictionary<string, MonoScript> _scriptCache;
            private bool _viewString = false;
            #endregion

            #region Constructor
            /// <summary>
            /// Initializes the <see cref="MonoScriptPropertyDrawer"/> class.
            /// </summary>
            static MonoScriptPropertyDrawer() {
                _scriptCache = new Dictionary<string, MonoScript>();
                var scripts = Resources.FindObjectsOfTypeAll<MonoScript>();
                for (int i = 0; i < scripts.Length; i++) {
                    var type = scripts[i].GetClass();
                    if (type != null && !_scriptCache.ContainsKey(type.FullName)) {
                        _scriptCache.Add(type.FullName, scripts[i]);
                    }
                }
            }
            #endregion

            #region GUI Override
            /// <summary>
            /// Override this method to make your own IMGUI based GUI for the property.
            /// </summary>
            /// <param name="position">Rectangle on the screen to use for the property GUI.</param>
            /// <param name="property">The SerializedProperty to make the custom GUI for.</param>
            /// <param name="label">The label of this property.</param>
            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
                if (property.propertyType == SerializedPropertyType.String) {
                    Color defaultColor = GUI.color;
                    Rect r = EditorGUI.PrefixLabel(position, label);
                    Rect labelRect = position;
                    labelRect.xMax = r.xMin;
                    position = r;
                    
                    // If right clicked, show the text value
                    _viewString = GUI.Toggle(labelRect, _viewString, "", "label");
                    if (_viewString) {
                        property.stringValue = EditorGUI.TextField(position, property.stringValue);
                        return;
                    }

                    MonoScript script = null;
                    string typeName = property.stringValue;

                    // Check to see if the serialized text is valid or not
                    if (!string.IsNullOrEmpty(typeName)) {
                        _scriptCache.TryGetValue(typeName, out script);
                        if (script == null) {
                            GUI.color = Color.red;
                        }
                    }

                    // Show an object field for selecting a script
                    script = (MonoScript)EditorGUI.ObjectField(position, script, typeof(MonoScript), false);
                    if (GUI.changed) {
                        if (script != null) {
                            var type = script.GetClass();
                            MonoScriptAttribute attr = (MonoScriptAttribute)attribute;
                            if (attr.type != null && !attr.type.IsAssignableFrom(type))
                                type = null;
                            if (type != null)
                                property.stringValue = script.GetClass().FullName;
                            else
                                Debug.LogWarning("The script file " + script.name + " doesn't contain an assignable class");
                        }
                        else
                            property.stringValue = "";
                    }
                    GUI.color = defaultColor;
                }
                else {
                    GUI.Label(position, "The MonoScript attribute can only be used on string variables");
                }
            }
            #endregion
        }
#endif
    }
}