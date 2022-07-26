using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TutorialProject.Framework.Events;
using TutorialProject.Framework.Runtime;

namespace TutorialProject.Framework.UI {
    /// <summary>
    /// Adjusts a specific text instance to always reflect that of a specific runtime variable.
    /// </summary>
    /// <seealso cref="UnityEngine.MonoBehaviour" />
    public class RuntimeVariableUI : MonoBehaviour, IGameEventListener {
        #region Declarations
        [Header("Project Assets")]
        [SerializeField] private RuntimeVariable _textVar = null;
        [SerializeField] private ColorVariable _colorVar = null;

        [Header("Scene References")]
        [SerializeField, Tooltip("Has both text and color applied")] private TMP_Text[] _text = new TMP_Text[] { };
        [SerializeField, Tooltip("Has only the color applied")] private Graphic[] _graphics = new Graphic[] { };

        [Header("Settings")]
        [SerializeField, Tooltip("If enabled, this will subscribe to the runtime variable events to refresh when their value changes")] private bool _autoRefresh = true;

        private bool _registered;
        #endregion

        #region MonoBehavior Overrides
        /// <summary>
        /// Resets this instance.
        /// </summary>
        private void Reset() {
            _text = GetComponents<TextMeshProUGUI>();
            _graphics = GetComponents<Image>();
        }

        /// <summary>
        /// Called when [enable].
        /// </summary>
        private void OnEnable() {
            Refresh();

            // Check to see if we should register to the events so we refresh when they change
            if (_autoRefresh && !_registered) {
                _registered = true;

                if (_textVar != null && _textVar.ValueChangedGameEvent != null) { _textVar.ValueChangedGameEvent.RegisterListener(this); }
                if (_colorVar != null && _colorVar.ValueChangedGameEvent != null) { _colorVar.ValueChangedGameEvent.RegisterListener(this); }
            }
        }

        /// <summary>
        /// Called when [destroy].
        /// </summary>
        private void OnDisable() {
            if (!Application.isPlaying) { return; }

            // Check to see if we should unregister to the variable events
            if (_autoRefresh && _registered) {
                _registered = false;

                if (_textVar != null && _textVar.ValueChangedGameEvent != null) { _textVar.ValueChangedGameEvent.UnregisterListener(this); }
                if (_colorVar != null && _colorVar.ValueChangedGameEvent != null) { _colorVar.ValueChangedGameEvent.UnregisterListener(this); }
            }
        }
        #endregion

        #region IGameEventListener Implements
        /// <summary>
        /// Handles the raising of the linked event.
        /// </summary>
        /// <param name="gameEvent">The game event being raised.</param>
        /// <param name="sender">The sender that is triggering the event (can be NULL).</param>
        /// <param name="args">Any arguments being passed along with the event.</param>
        public void OnEventRaised(GameEvent gameEvent, object sender, params object[] args) {
            Refresh();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Refreshes the text to match the value of the runtime variable.
        /// </summary>
        public void Refresh() {
            // Update all of the configured text and images
            for (int x = 0; x < _text.Length; x++) {
                var text = _text[x];

                if (_textVar != null) { text.text = _textVar.GetObjectValue().ToString(); }
                if (_colorVar != null) { text.color = _colorVar.Value; }
            }

            for (int x = 0; x < _graphics.Length; x++) {
                var graphic = _graphics[x];

                if (_colorVar != null) { graphic.color = _colorVar.Value; }
            }
        }
        #endregion
    }
}
