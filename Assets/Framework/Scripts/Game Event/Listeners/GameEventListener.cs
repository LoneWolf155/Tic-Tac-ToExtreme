using UnityEngine;
using UnityEngine.Events;

namespace TutorialProject.Framework.Events {
    /// <summary>
    /// A simple listener for a <see cref="GameEvent"/> which can trigger a response in a scene.
    /// </summary>
    /// <seealso cref="UnityEngine.MonoBehaviour" />
    public class GameEventListener : MonoBehaviour, IGameEventListener {
        #region Declarations
        [Tooltip("The GameEvent that we are listening to")] public GameEvent Event;
        [TextArea, Tooltip("A simple description of what this listener is doing")] public string description = "";
        public UnityEvent<object> Response;             // Standard response where the sender is sent as an argument
        #endregion

        #region MonoBehavior Overrides
        /// <summary>
        /// Awakes this instance.
        /// </summary>
        private void Awake() {
            if (Event == null || Response == null) {
                Debug.LogError($"{this.GetType().Name} does not have an Event or a Response set!", this);
                this.enabled = false;
            }
        }

        /// <summary>
        /// Called when [enable].
        /// </summary>
        private void OnEnable() {
            if (Event != null) { Event.RegisterListener(this); }
        }

        /// <summary>
        /// Called when [disable].
        /// </summary>
        private void OnDisable() {
            if (Event != null) { Event.UnregisterListener(this); }
        }
        #endregion

        #region Event Methods
        /// <summary>
        /// Handles the raising of the linked event.
        /// </summary>
        /// <param name="gameEvent">The game event being raised.</param>
        /// <param name="sender">The sender that is triggering the event (can be NULL).</param>
        /// <param name="args">Any arguments being passed along with the event.</param>
        public void OnEventRaised(GameEvent gameEvent, object sender, params object[] args) {
            Response?.Invoke(sender);
        }
        #endregion
    }
}
