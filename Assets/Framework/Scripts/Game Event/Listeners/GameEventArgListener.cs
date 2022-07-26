using UnityEngine;
using UnityEngine.Events;

namespace TutorialProject.Framework.Events {
    /// <summary>
    /// A listener for a <see cref="GameEvent"/> which can trigger a response in a scene and passes along arguments.
    /// </summary>
    /// <seealso cref="UnityEngine.MonoBehaviour" />
    public class GameEventArgListener : MonoBehaviour, IGameEventListener {
        #region Declarations
        [System.Serializable]
        public class CustomResponse<T> {
            [Tooltip("Whether or not this response should be processed")] public bool processEvent = false;
            [Tooltip("Whether or not an argument is required to be processed")] public bool requireArgument = true;
            [Tooltip("If not argument is passed and an argument is not required, then this gets passed as the argument instead")] public T defaultValue;
            public UnityEvent<T> Response;
        }

        [Tooltip("The GameEvent that we are listening to")] public GameEvent Event;
        [TextArea, Tooltip("A simple description of what this listener is doing")] public string description = "";

        public CustomResponse<string> ResponseStringArg;    // Response that passed any received argument as a string
        public CustomResponse<int> ResponseIntArg;          // Response that passed any received argument as an integer
        #endregion

        #region MonoBehavior Overrides
        /// <summary>
        /// Awakes this instance.
        /// </summary>
        private void Awake() {
            if (Event == null) {
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
            if (ResponseStringArg.processEvent) {
                if (args.Length > 0) { ResponseStringArg.Response?.Invoke(args[0].ToString()); }
                else if (!ResponseStringArg.requireArgument) { ResponseStringArg.Response?.Invoke(ResponseStringArg.defaultValue); }
            }

            if (ResponseIntArg.processEvent) {
                if (args.Length > 0) {
                    int arg;

                    if (args[0] is int) {
                        ResponseIntArg.Response?.Invoke((int)args[0]);
                    }
                    else if (int.TryParse(args[0].ToString(), out arg)) {
                        ResponseIntArg.Response?.Invoke(arg);
                    }
                }
                else if (!ResponseIntArg.requireArgument) { ResponseIntArg.Response?.Invoke(ResponseIntArg.defaultValue); }
            }
        }
        #endregion
    }
}
