using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TutorialProject.Framework.Utility {
    /// <summary>
    /// Attribute that makes a field read-only when looking at it within the editor.
    /// </summary>
    /// <seealso cref="UnityEngine.PropertyAttribute" />
    public class EditorReadOnlyAttribute : PropertyAttribute {
        public bool VisibleRuntimeOnly { get; set; }
        public bool DisabledRuntimeOnly { get; set; }

        public EditorReadOnlyAttribute() {
            VisibleRuntimeOnly = false;
            DisabledRuntimeOnly = false;
        }
        public EditorReadOnlyAttribute(bool visibleRuntimeOnly) {
            VisibleRuntimeOnly = visibleRuntimeOnly;
            DisabledRuntimeOnly = false;
        }
        public EditorReadOnlyAttribute(bool visibleRuntimeOnly, bool disabledRuntimeOnly) {
            VisibleRuntimeOnly = visibleRuntimeOnly;
            DisabledRuntimeOnly = disabledRuntimeOnly;
        }
    }

    namespace Editor {
#if UNITY_EDITOR
        [CustomPropertyDrawer(typeof(EditorReadOnlyAttribute))]
        public class ReadOnlyDrawer : PropertyDrawer {
            /// <summary>
            /// Override this method to specify how tall the GUI for this field is in pixels.
            /// </summary>
            /// <param name="property">The SerializedProperty to make the custom GUI for.</param>
            /// <param name="label">The label of this property.</param>
            /// <returns>
            /// The height in pixels.
            /// </returns>
            public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
                if (((EditorReadOnlyAttribute)attribute).VisibleRuntimeOnly && !Application.isPlaying) {
                    //The property is not being drawn
                    //We want to undo the spacing added before and after the property
                    return -EditorGUIUtility.standardVerticalSpacing;
                }
                return EditorGUI.GetPropertyHeight(property, label, true);
            }

            /// <summary>
            /// Override this method to make your own IMGUI based GUI for the property.
            /// </summary>
            /// <param name="position">Rectangle on the screen to use for the property GUI.</param>
            /// <param name="property">The SerializedProperty to make the custom GUI for.</param>
            /// <param name="label">The label of this property.</param>
            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
                var attr = (EditorReadOnlyAttribute)attribute;
                if (attr.VisibleRuntimeOnly && !Application.isPlaying) {
                    return;
                }
                else if (attr.DisabledRuntimeOnly && !Application.isPlaying) {
                    EditorGUI.PropertyField(position, property, label, true);
                    return;
                }

                bool previousEnableState = GUI.enabled;
                GUI.enabled = false;
                EditorGUI.PropertyField(position, property, label, true);
                GUI.enabled = previousEnableState;
            }
        }
#endif
    }
}
