using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TutorialProject.Framework.Utility {
    /// <summary>
    /// Only shows the specified field during runtime.
    /// </summary>
    /// <seealso cref="UnityEngine.PropertyAttribute" />
    public class VisibleRuntimeOnlyAttribute : PropertyAttribute {
        /// <summary>
        /// Attribute used to show or hide the Field depending on certain conditions
        /// </summary>
        /// <param name="conditionFieldName">Name of the bool condition Field</param>
        public VisibleRuntimeOnlyAttribute() { }
    }

    namespace Editor {
        [CustomPropertyDrawer(typeof(VisibleRuntimeOnlyAttribute))]
        public class VisibleRuntimeOnlyDrawer : PropertyDrawer {
            /// <summary>
            /// Gets the height of the property.
            /// </summary>
            /// <param name="property">The property.</param>
            /// <param name="label">The label.</param>
            /// <returns></returns>
            public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
                if (Application.isPlaying)
                    return EditorGUI.GetPropertyHeight(property, label, true);
                else
                    return -EditorGUIUtility.standardVerticalSpacing;
            }

            /// <summary>
            /// Called when [GUI].
            /// </summary>
            /// <param name="position">The position.</param>
            /// <param name="property">The property.</param>
            /// <param name="label">The label.</param>
            /// <returns></returns>
            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
                if (Application.isPlaying)
                    EditorGUI.PropertyField(position, property, label, true);
            }
        }
    }
}
