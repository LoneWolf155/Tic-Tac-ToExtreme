using UnityEngine;
using UnityEngine.Events;
using TutorialProject.Framework.Events;
using TutorialProject.Framework.Utility;

namespace TutorialProject.Framework.UI {
    /// <summary>
    /// A script that triggers an animation when an event is fired.
    /// </summary>
    /// <seealso cref="UnityEngine.MonoBehaviour" />
    public class AnimateOnGameEvent : MonoBehaviour, IGameEventListener {
        #region Declarations
        [System.Serializable]
        public class TriggeredGameEvents {
            public GameEvent onAnimationEnding;
        }
        [System.Serializable]
        public class ListenedGameEvents {
            public GameEvent onAnimationStart;
        }

        [Header("Animation Settings")]
        [SerializeField, Tooltip("The animator that is going to be affected")] protected Animator _animator = null;
        [SerializeField, Tooltip("The animation state we will be playing and waiting for")] protected AnimationPlayData _animData = new AnimationPlayData();
        [SerializeField, Tooltip("If false, any triggers of the starting event will be ignored if the animation is still playing")] protected bool _allowInterrupt = false;

        [Header("Event Settings")]
        [SerializeField, Tooltip("Event that will trigger this animation")] protected GameEvent _onAnimationStart;
        [SerializeField] protected UnityEvent _startEvents;
        [SerializeField, Tooltip("Event that is triggered by this script when the animation ends")] protected GameEvent _onAnimationEnding;
        [SerializeField] protected UnityEvent _endEvents;

        private bool _assignedEvents;
        private bool _isAnimating;
        #endregion

        #region MonoBehavior Overrides
        /// <summary>
        /// Called when [enable].
        /// </summary>
        private void OnEnable() {
            if (!Application.isPlaying) { return; }
            RegisterEventListener();
        }

        /// <summary>
        /// Called when [disable].
        /// </summary>
        private void OnDisable() {
            if (!Application.isPlaying) { return; }
            UnregisterEventListener();
        }
        #endregion

        #region Event Methods
        /// <summary>
        /// Handles the raising of the linked event.
        /// </summary>
        /// <param name="sender">The sender that is triggering the event (can be NULL).</param>
        /// <param name="gameEvent">The game event being raised.</param>
        public void OnEventRaised(GameEvent gameEvent, object sender, params object[] args) {
            // Determine the event and call the appropriate method to handle it
            if (_onAnimationStart == gameEvent) {
                StartAnimation();
            }
            else {
                Debug.LogWarning("Registered to unrecognized event or an event triggered with unexpected sender/arguments!\nEvent: " + gameEvent.name, this);
            }
        }

        /// <summary>
        /// Registers this to all tied events so we can listen in on them.
        /// </summary>
        private void RegisterEventListener() {
            if (_assignedEvents) { return; }
            _assignedEvents = true;

            _onAnimationStart?.RegisterListener(this);
        }

        /// <summary>
        /// Unregisters this from all tied events.
        /// </summary>
        private void UnregisterEventListener() {
            if (!_assignedEvents) { return; }
            _assignedEvents = false;

            _onAnimationStart?.UnregisterListener(this);
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Starts the animation.
        /// </summary>
        private void StartAnimation() {
            if (_isAnimating && !_allowInterrupt) { return; }
            _isAnimating = true;

            AnimationWaitInfo waitInfo = new AnimationWaitInfo((info) => {
                _isAnimating = false;
                _endEvents?.Invoke();
                _onAnimationEnding?.Raise(this);
            });
            _startEvents?.Invoke();
            StartCoroutine(AnimationUtility.PlayAnimation(_animator, _animData, waitInfo));
        }
        #endregion
    }
}
