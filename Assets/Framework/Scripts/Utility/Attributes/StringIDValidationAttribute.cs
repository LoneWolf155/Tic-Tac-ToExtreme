using UnityEngine;
using System.Reflection;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TutorialProject.Framework.Utility {
    /// <summary>
    /// Can be used on a string field to help enforce rules on a string field that is to be used for ID purposes.
    /// </summary>
    /// <seealso cref="UnityEngine.PropertyAttribute" />
    public class StringIDValidationAttribute : PropertyAttribute {
        public bool AllowSpaces { get; set; }
        public bool AllowMixedCasing { get; set; }
        public bool AllowEmpty { get; set; }

        public StringIDValidationAttribute() : this(false, true, false) { }
        public StringIDValidationAttribute(bool allowSpaces, bool allowMixedCasing, bool allowEmpty) {
            AllowSpaces = allowSpaces;
            AllowMixedCasing = allowMixedCasing;
            AllowEmpty = allowEmpty;
        }
    }

    namespace Editor {
#if UNITY_EDITOR
        [CustomPropertyDrawer(typeof(StringIDValidationAttribute))]
        public class StringIDValidationDrawer : PropertyDrawer {
            #region Declarations
            public const string UNIQUE_BUTTON_TEXT = "Is Unique?";

            private bool _uniqueChecked = false;
            private bool _isUnique = false;
            private string _uniqueCheckValue = "";
            #endregion

            #region Overrides
            /// <summary>
            /// Override this method to specify how tall the GUI for this field is in pixels.
            /// </summary>
            /// <param name="property">The SerializedProperty to make the custom GUI for.</param>
            /// <param name="label">The label of this property.</param>
            /// <returns>
            /// The height in pixels.
            /// </returns>
            public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
                var attr = (StringIDValidationAttribute)attribute;
                string message;

                if (property.propertyType == SerializedPropertyType.String && (_uniqueChecked || !IsValid(property, attr, out message))) {
                    return (EditorGUI.GetPropertyHeight(property, label, true) * 2) + 25;
                }
                else {
                    return EditorGUI.GetPropertyHeight(property, label, true);
                }
            }

            /// <summary>
            /// Override this method to make your own IMGUI based GUI for the property.
            /// </summary>
            /// <param name="position">Rectangle on the screen to use for the property GUI.</param>
            /// <param name="property">The SerializedProperty to make the custom GUI for.</param>
            /// <param name="label">The label of this property.</param>
            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
                var attr = (StringIDValidationAttribute)attribute;
                if (property.propertyType != SerializedPropertyType.String) {
                    GUI.Label(position, "The StringIDValidationAttribute attribute can only be used on string fields");
                    return;
                }

                // As a quick pre-cursor, if unique was checked but the value has changed then clear it out
                if (_uniqueChecked && property.stringValue != _uniqueCheckValue) {
                    _uniqueChecked = false;
                    _isUnique = false;
                    _uniqueCheckValue = "";
                }

                // Perform our checks
                string message;
                if (!IsValid(property, attr, out message)) {
                    ShowHelpBox(position, property, label, message, MessageType.Error);
                }
                else if (property.serializedObject.targetObject.GetType().IsSubclassOf(typeof(ScriptableObject))) {
                    // ---- For scriptable objects, allow us to verify it is unique
                    if (_uniqueChecked) {
                        if (_isUnique) {
                            ShowHelpBox(position, property, label, $"Value of '{_uniqueCheckValue}' is unique!", MessageType.Info);
                        }
                        else {
                            ShowHelpBox(position, property, label, $"Value of '{_uniqueCheckValue}' is not unique!", MessageType.Error);
                        }
                    }
                    else {
                        // Show the button for unique validation
                        var totalWidth = position.width;
                        var buttonWidth = GUI.skin.button.CalcSize(new GUIContent(UNIQUE_BUTTON_TEXT)).x;
                        var propRect = new Rect(position.x, position.y, totalWidth - (buttonWidth + 5), position.height);
                        var buttonRect = new Rect(position.x + totalWidth - buttonWidth, position.y, buttonWidth, position.height);

                        EditorGUI.PropertyField(propRect, property, label, true);
                        if (GUI.Button(buttonRect, UNIQUE_BUTTON_TEXT)) {
                            _uniqueChecked = true;
                            _uniqueCheckValue = property.stringValue;

                            if (IsUnique(property)) {
                                _isUnique = true;
                            }
                            else {
                                _isUnique = false;
                            }
                        }
                    }
                }
                else {
                    EditorGUI.PropertyField(position, property, label, true);
                }
            }
            #endregion

            #region Private Methods
            /// <summary>
            /// Shows a help box.
            /// </summary>
            /// <param name="position">The position.</param>
            /// <param name="property">The property.</param>
            /// <param name="label">The label.</param>
            /// <param name="message">The message.</param>
            /// <param name="type">The type.</param>
            private void ShowHelpBox(Rect position, SerializedProperty property, GUIContent label, string message, MessageType type) {
                position.height /= 2;
                EditorGUI.PropertyField(position, property, label, true);

                position.height += 5;
                position.y += EditorGUI.GetPropertyHeight(property, label, true) + 5;
                EditorGUI.HelpBox(position, message, type);
            }

            /// <summary>
            /// Returns whether or not the property is valid based on our attribute settings.
            /// </summary>
            /// <param name="property">The property.</param>
            /// <param name="attribute">The attribute.</param>
            /// <param name="reason">The reason.</param>
            /// <returns>
            ///   <c>true</c> if the specified property is valid; otherwise, <c>false</c>.
            /// </returns>
            private bool IsValid(SerializedProperty property, StringIDValidationAttribute attribute, out string reason) {
                if (!attribute.AllowEmpty && string.IsNullOrEmpty(property.stringValue)) {
                    reason = $"Field '{property.displayName}' must contain a value";
                    return false;
                }
                else if (!attribute.AllowSpaces && (property.stringValue.Contains(" ") || property.stringValue.Contains("\t") || property.stringValue.Contains("\r"))) {
                    reason = $"Field '{property.displayName}' must not contain whitespace";
                    return false;
                }
                else if (!attribute.AllowMixedCasing && property.stringValue != property.stringValue.ToLower()) {
                    reason = $"Field '{property.displayName}' must not contain mixed casing";
                    return false;
                }
                reason = "";
                return true;
            }

            /// <summary>
            /// Determines whether the specified property value is unique.
            /// </summary>
            /// <param name="property">The property.</param>
            /// <returns>
            ///   <c>true</c> if the specified property is unique; otherwise, <c>false</c>.
            /// </returns>
            private bool IsUnique(SerializedProperty property) {
                var assets = EditorUtility.GetScriptableObjectAssets(property.serializedObject.targetObject.GetType());

                for (int x=0; x < assets.Length; x++) {
                    var tmpAsset = assets[x];
                    if (tmpAsset == property.serializedObject.targetObject) { continue; }

                    string tmpValue = fieldInfo.GetValue(assets[x]) as string;
                    if (tmpValue == property.stringValue) {
                        return false;
                    }
                }
                return true;
            }
            #endregion
        }
#endif
    }
}