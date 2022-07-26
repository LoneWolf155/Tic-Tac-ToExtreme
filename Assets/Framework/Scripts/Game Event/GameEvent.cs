using System.Collections.Generic;
using UnityEngine;
using TutorialProject.Framework.Utility;

namespace TutorialProject.Framework.Events {
    /// <summary>
    /// A simple event to which different listeners and callers can access. Created as a <see cref="ScriptableObject"/>
    /// allowing for references tied to other <see cref="ScriptableObject"/> instances.
    /// </summary>
    /// <seealso cref="UnityEngine.ScriptableObject" />
    [System.Serializable]
    public class GameEvent : ScriptableObject {
        #region Declarations
        [SerializeField, TextArea, Tooltip("Brief description of this GameEvent")] private string _description = "";
        [SerializeField, MonoScript, Tooltip("The expected type for the sender of this GameEvent. If not specified, should be the GameObject raising the event")] private string _senderType = null;
        [SerializeField, Tooltip("If true, the game event will not execute unless the arguments passed are properly validated")] private bool _validateArguments = true;
        [SerializeField, Tooltip("The read-only expected arguments of this GameEvent, formatted like <{type} name> or [{type} name] for optional")] public string[] _args = new string[] { };

        public static bool LogAllCalls { get; set; }
        public bool HasListeners { get { return _listeners != null && _listeners.Count > 0; } }

        public string Description { get => _description; }
        public string SenderType { get => _senderType; }
        public string[] Args { get => _args; }

        private List<IGameEventListener> _listeners;
        #endregion

        #region MonoBehavior Overrides
        /// <summary>
        /// Awakes this instance.
        /// </summary>
        private void Awake() {
            if (_listeners == null) { _listeners = new List<IGameEventListener>(); }
        }

        /// <summary>
        /// Called when [enable].
        /// </summary>
        private void OnEnable() {
            if (_listeners == null) { _listeners = new List<IGameEventListener>(); }
        }

#if UNITY_EDITOR
        /// <summary>
        /// Called when [validate].
        /// </summary>
        private void OnValidate() {
            // If any empty arguments exist then fill it in with the expected format
            for (int x=0; x < _args.Length; x++) {
                if (string.IsNullOrEmpty(_args[x])) {
                    _args[x] = "<{type} name>";
                }
            }
        }
#endif
        #endregion

        #region Event Methods
        /// <summary>
        /// Raises this event instance.
        /// </summary>
        public void Raise(object sender, params object[] eventArgs) {
            if (LogAllCalls) {
                if (sender != null) {
                    Debug.Log($"GameEvent '{name}' called by sender '{sender}'", this);
                }
                else {
                    Debug.Log($"GameEvent '{name}' called by method '{new System.Diagnostics.StackFrame(1).GetMethod().Name}'", this);
                }
            }

#if UNITY_EDITOR
            // Check to see if the sender matches what we expect
            if (!string.IsNullOrEmpty(SenderType)) {
                if (sender == null) {
                    Debug.Log($"GameEvent '{name}' expects a sender but did not receive one!", this);
                }
                else {
                    var tmpType = System.Type.GetType(SenderType);
                    if (tmpType == null) {
                        Debug.Log($"GameEvent '{name}' is set to expect a type of '{SenderType}' but that type could not be found!", this);
                    }
                    else if (sender.GetType() != tmpType) {
                        Debug.Log($"GameEvent '{name}' expects a sender of type '{tmpType.Name}' but received type of '{sender.GetType().Name}'!", this);
                    }
                }
            }
#endif

            if (_validateArguments && eventArgs != null) {
                int optionalCount = 0;

                // Check how many optional arguments exist (should ALWAYS be at the end)
                for (int x = Args.Length - 1; x >= 0; x--) {
                    if (Args[x].StartsWith("[") && Args[x].EndsWith("]")) { optionalCount += 1; }
                    else { break; }
                }

                // Check to see if the argument count meets what we have expected
                if (optionalCount > 0) {
                    if (eventArgs.Length < Args.Length - optionalCount) {
                        Debug.LogWarning($"GameEvent '{name}' was passed {eventArgs.Length} arguments but is expecting at least {Args.Length - optionalCount} with {optionalCount} optional! Stopping execution!", this);
                        return;
                    }
                    else if (eventArgs.Length > Args.Length) {
                        Debug.LogWarning($"GameEvent '{name}' was passed {eventArgs.Length} arguments but is expecting no more than {Args.Length}! Stopping execution!", this);
                        return;
                    }
                }
                else {
                    if (eventArgs.Length != Args.Length) {
                        Debug.LogWarning($"GameEvent '{name}' was passed {eventArgs.Length} arguments but is expecting {Args.Length}! Stopping execution!", this);
                        return;
                    }
                }
            }

            // Raise the event to each listener
            if (_listeners == null || _listeners.Count == 0) { return; }
            for (int x = _listeners.Count - 1; x >= 0; x--) {
                _listeners[x].OnEventRaised(this, sender, eventArgs);
            }
        }

        /// <summary>
        /// Registers the specified listener to this event.
        /// </summary>
        /// <param name="listener">The listener.</param>
        public void RegisterListener(IGameEventListener listener) {
#if UNITY_EDITOR
            if (_listeners.Contains(listener)) {
                if (listener is Object) {
                    Debug.LogWarning($"{this.GetType().Name} with name '{this.name}' has had a requested register from listener '{(listener as Object).name}' that is already registered, bug?", this);
                }
                else {
                    Debug.LogWarning($"{this.GetType().Name} with name '{this.name}' has had a requested register from a listener that is already registered, bug?", this);
                }
            }
#endif
            _listeners.Add(listener);
        }

        /// <summary>
        /// Unregisters the specified listener to this event.
        /// </summary>
        /// <param name="listener">The listener.</param>
        public void UnregisterListener(IGameEventListener listener) {
            _listeners.Remove(listener);
        }
        #endregion

        #region Context Menu
#if UNITY_EDITOR
        /// <summary>
        /// Creates an asset file for this class.
        /// </summary>
        [UnityEditor.MenuItem("Assets/Create/Custom/GameEvent", false, 0)]
        public static void CreateAsset() {
            CustomAssetUtil.CreateAsset<GameEvent>("OnGameEvent");
        }
#endif
        #endregion
    }
}
