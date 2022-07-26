using UnityEngine;
using TMPro;
using TutorialProject.Framework.Events;
using TutorialProject.Framework.Utility;

#if UNITY_EDITOR
using TutorialProject.Framework.Utility.Editor;
#endif

namespace TutorialProject.Game {
    /// <summary>
    /// Represents a single block on the tic-tac-toe board.
    /// </summary>
    /// <seealso cref="UnityEngine.MonoBehaviour" />
    public class BoardBlock : MonoBehaviour, IGameEventListener {
        #region Declarations
        [System.Serializable]
        public class TriggeredGameEvents {
            public GameEvent onBoardBlockClick;
        }

        [System.Serializable]
        public class ListenedGameEvents {
            public GameEvent onGameStart;
            public GameEvent onGameEnd;
            public GameEvent onPlayerColorChanged;
            public GameEvent onPlayerSymbolChanged;
        }

        [Header("Project Assets")]
        [SerializeField] private GlobalGameData _gameData = null;
        [SerializeField] private TriggeredGameEvents _triggeredEvents = new TriggeredGameEvents();
        [SerializeField] private ListenedGameEvents _listenedEvents = new ListenedGameEvents();

        [Header("Scene References")]
        [SerializeField] private Animator _animator;
        [SerializeField] private TMP_Text _text;

        [Header("Animator Settings")]
        [SerializeField] private string _hoverBoolParam = "isHovering";
        [SerializeField] private string _resetTriggerParam = "resetStates";
        [SerializeField] private AnimationPlayData _clickedAnimData;

        public GamePowerup AvailablePowerup { get; set; }
        public bool Selected { get; private set; }
        public string CurrentValue { get; private set; }
        public bool IsAnimating { get { return _animWaitInfo != null && !_animWaitInfo.HasEnded; } }

        private AnimationWaitInfo _animWaitInfo;
        private bool _assignedEvents;
        private bool _gameRunning;
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
            _gameData = EditorUtility.GetScriptableObjectAsset<GlobalGameData>();

            EditorUtility.AutoFillEvents<TriggeredGameEvents>(ref _triggeredEvents);
            EditorUtility.AutoFillEvents<ListenedGameEvents>(ref _listenedEvents);
        }
#endif

        /// <summary>
        /// Called when [mouse enter].
        /// </summary>
        private void OnMouseEnter() {
            if (Selected || !_gameRunning) { return; }
            _animator.SetBool(_hoverBoolParam, true);
        }

        /// <summary>
        /// Called when [mouse exit].
        /// </summary>
        private void OnMouseExit() {
            if (Selected || !_gameRunning) { return; }
            _animator.SetBool(_hoverBoolParam, false);
        }

        /// <summary>
        /// Called when [mouse up].
        /// </summary>
        private void OnMouseUp() {
            if (Selected || !_gameRunning) { return; }

            // Raise the event and let outside handlers decide whether we can be selected or not
            _triggeredEvents.onBoardBlockClick?.Raise(this);
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
                _gameRunning = true;
            }
            else if (gameEvent == _listenedEvents.onGameEnd) {
                _gameRunning = false;
            }
            else if (gameEvent == _listenedEvents.onPlayerColorChanged || gameEvent == _listenedEvents.onPlayerSymbolChanged) {
                PlayerData player = null;

                // Seek out the player that matches our value and set our color to match
                for (int x=0; x < _gameData.PlayersData.Length; x++) {
                    if (CurrentValue == _gameData.PlayersData[x].Symbol.Value) {
                        player = _gameData.PlayersData[x];
                    }
                }

                if (player != null) {
                    _text.color = player.Color.Value;
                }
                else {
                    // No player meaning we are a meaningless block
                    _text.color = Color.white;
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

        #region Public Methods
        /// <summary>
        /// Triggers the selection for this block.
        /// </summary>
        /// <param name="playerData">The data for the player selecting this.</param>
        /// <param name="animationEndCallback">The animation end callback.</param>
        public void TriggerSelection(PlayerData playerData, System.Action<AnimationWaitInfo> animationEndCallback = null) {
            // Set the current value to that of the current player
            Selected = true;
            CurrentValue = playerData.Symbol.Value;

            if (AvailablePowerup != null) {
                playerData.GivePowerup(AvailablePowerup);
            }

            _text.text = CurrentValue;
            _text.color = playerData.Color.Value;

            _animator.SetBool(_hoverBoolParam, false);  // Ensure this is set to false
            _animator.ResetTrigger(_resetTriggerParam); // Clear otherwise it could still be waiting and cause problems

            // Play our animation
            _animWaitInfo = new AnimationWaitInfo(animationEndCallback);
            StartCoroutine(AnimationUtility.PlayAnimation(_animator, _clickedAnimData, _animWaitInfo));
        }

        /// <summary>
        /// Resets this block back to its initial state.
        /// </summary>
        public void ResetBlock() {
            // Reset our data
            Selected = false;
            CurrentValue = "";
            AvailablePowerup = null;
            _text.text = "";

            // Reset our animation
            _animator.SetBool(_hoverBoolParam, false);
            _animator.SetTrigger(_resetTriggerParam);
        }
        #endregion
    }
}
