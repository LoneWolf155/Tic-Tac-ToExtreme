using UnityEngine;
using UnityEditor;

namespace TutorialProject.Framework.Events.Editor {
    [CustomEditor(typeof(GameEvent)), CanEditMultipleObjects()]
    public class GameEventEditor : UnityEditor.Editor {
        /// <summary>
        /// Called when [enable].
        /// </summary>
        void OnEnable() { }

        /// <summary>
        /// Implement this function to make a custom inspector.
        /// </summary>
        public override void OnInspectorGUI() {
            DrawDefaultInspector();

            if (GUILayout.Button("Trigger Event")) {
                ((GameEvent)serializedObject.targetObject).Raise(null);
            }
        }
    }
}
