using UnityEngine;
using TMPro;
using TutorialProject.Framework.Events;
using TutorialProject.Framework.Utility;

#if UNITY_EDITOR
using TutorialProject.Framework.Utility.Editor;
#endif

namespace TutorialProject.Game {
    /// <summary>
    /// Shows the winner and waits for a click to start a new game.
    /// </summary>
    /// <seealso cref="UnityEngine.MonoBehaviour" />
    public class WinnerTextDisplay : MonoBehaviour, IGameEventListener {
        #region Declarations
        [System.Serializable]
        public class TriggeredGameEvents {
            public GameEvent startNewGame;
        }

        [System.Serializable]
        public class ListenedGameEvents {
            public GameEvent onGameStart;
            public GameEvent onGameEnd;
        }

        [Header("Project Assets")]
        [SerializeField] private TriggeredGameEvents _triggeredEvents = new TriggeredGameEvents();
        [SerializeField] private ListenedGameEvents _listenedEvents = new ListenedGameEvents();

        [Header("Scene References")]
        [SerializeField] private Animator _animator;
        [SerializeField] private TMP_Text _winnerNumText;
        [SerializeField] private TMP_Text[] _winnerText;

        [Header("Animation")]
        [SerializeField] private string _resetTriggerParam = "resetStates";
        [SerializeField] private AnimationPlayData _winnerAnim;
        [SerializeField] private AnimationPlayData _tieAnim;

        private bool _assignedEvents;
        private bool _waitingForClick;
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

#if UNITY_EDITOR
        /// <summary>
        /// Resets this instance.
        /// </summary>
        private void Reset() {
            _animator = GetComponent<Animator>();

            EditorUtility.AutoFillEvents<TriggeredGameEvents>(ref _triggeredEvents);
            EditorUtility.AutoFillEvents<ListenedGameEvents>(ref _listenedEvents);
        }
#endif

        /// <summary>
        /// Called when [mouse up].
        /// </summary>
        private void Update() {
            if (_waitingForClick && Input.GetMouseButtonUp(0)) {
                _waitingForClick = false;
                _triggeredEvents.startNewGame?.Raise(this);
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
            if (gameEvent == _listenedEvents.onGameStart) {
                _animator.SetTrigger(_resetTriggerParam);
            }
            else if (gameEvent == _listenedEvents.onGameEnd) {
                PlayerData winnerData = null;
                if (args.Length > 0 && args[0] is PlayerData) {
                    winnerData = (PlayerData)args[0];
                }

                // Clear the reset trigger to ensure it doesn't cause problems
                _animator.ResetTrigger(_resetTriggerParam);

                // Setup the wait info with an action so when the animation ends we will wait for a mouse click to trigger a new game
                AnimationWaitInfo waitInfo = new AnimationWaitInfo(new System.Action<AnimationWaitInfo>((waitInfo) => {
                    _waitingForClick = true;
                }));
                if (winnerData != null) {
                    for (int x = 0; x < _winnerText.Length; x++) {
                        _winnerText[x].outlineColor = winnerData.Color.Value;

                        // Need to call this to ensure the outline color takes as it effects the material
                        // NOTE: I believe this results in an instanced material which you should generally avoid for performance but for this game it is no big deal
                        _winnerText[x].SetMaterialDirty();
                    }
                    _winnerNumText.text = winnerData.PlayerNumber.ToString();

                    StartCoroutine(AnimationUtility.PlayAnimation(_animator, _winnerAnim, waitInfo));
                }
                else {
                    StartCoroutine(AnimationUtility.PlayAnimation(_animator, _tieAnim, waitInfo));
                }
            }
        }

        /// <summary>
        /// Registers this to all tied events so we can listen in on them.
        /// </summary>
        private void RegisterEventListener() {
            if (_assignedEvents) { return; }
            _assignedEvents = true;

            HelperUtility.AutoRegisterGameEvents<ListenedGameEvents>(_listenedEvents, this);
        }

        /// <summary>
        /// Unregisters this from all tied events.
        /// </summary>
        private void UnregisterEventListener() {
            if (!_assignedEvents) { return; }
            _assignedEvents = false;

            HelperUtility.AutoUnregisterGameEvents<ListenedGameEvents>(_listenedEvents, this);
        }
        #endregion
    }
}
