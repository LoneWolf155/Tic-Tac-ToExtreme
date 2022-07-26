using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TutorialProject.Framework.Events;
using TutorialProject.Framework.Utility;

#if UNITY_EDITOR
using TutorialProject.Framework.Utility.Editor;
#endif

namespace TutorialProject.Game {
    /// <summary>
    /// Deals with the player panel.
    /// </summary>
    /// <seealso cref="UnityEngine.MonoBehaviour" />
    /// <seealso cref="TutorialProject.Framework.Events.IGameEventListener" />
    public class PlayerPanel : MonoBehaviour, IGameEventListener {
        #region Declarations
        [System.Serializable]
        public class ListenedGameEvents {
            public GameEvent onPlayerTurnStart;
            public GameEvent onPlayerTurnEnd;
            public GameEvent onPlayerPowerupChanged;
            public GameEvent onGameEnd;
        }

        [Header("Project Assets")]
        [SerializeField] private PlayerData _playersData = null;
        [SerializeField] private ListenedGameEvents _listenedEvents = new ListenedGameEvents();

        [Header("Scene References")]
        [SerializeField] private Animator _animator;
        [SerializeField] private Button _powerupButton;
        [SerializeField] private TMP_Text _powerupText;

        [Header("Animation Settings")]
        private string _turnActiveBoolParam = "isTurn";

        private bool _assignedEvents;
        #endregion

        #region MonoBehavior Overrides
        /// <summary>
        /// Starts this instance.
        /// </summary>
        private void Start() {
            RefreshPowerupDisplay();
        }

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

            EditorUtility.AutoFillEvents<ListenedGameEvents>(ref _listenedEvents);
        }
#endif
        #endregion

        #region Powerup Methods
        /// <summary>
        /// Activates the powerup.
        /// </summary>
        public void UsePowerupClick() {
            if (_playersData.CurrentPowerup != null) {
                _playersData.CurrentPowerup.Execute();
                _playersData.RemovePowerup();
            }
        }

        /// <summary>
        /// Refreshes the powerup display.
        /// </summary>
        private void RefreshPowerupDisplay() {
            if (_playersData.CurrentPowerup != null) {
                _powerupButton.interactable = true;
                _powerupText.text = _playersData.CurrentPowerup.name;
            }
            else {
                _powerupButton.interactable = false;
                _powerupText.text = "[No Powerup]";
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
            if (gameEvent == _listenedEvents.onPlayerPowerupChanged) {
                if (sender != null && sender is PlayerData && (sender as PlayerData) == _playersData) {
                    RefreshPowerupDisplay();
                }
            }
            else if (gameEvent == _listenedEvents.onGameEnd) {
                // Toggle the animator off
                _animator.SetBool(_turnActiveBoolParam, false);
            }
            else if (gameEvent == _listenedEvents.onPlayerTurnStart) {
                // If this player then start their turn, otherwise ensure we are toggled off
                if (args.Length > 0 && args[0] is PlayerData && (args[0] as PlayerData) == _playersData) {
                    _animator.SetBool(_turnActiveBoolParam, true);
                }
                else {
                    _animator.SetBool(_turnActiveBoolParam, false);
                }
            }
            else if (gameEvent == _listenedEvents.onPlayerTurnEnd) {
                // If it is this player ending their turn than toggle off
                if (args.Length > 0 && args[0] is PlayerData && (args[0] as PlayerData) == _playersData) {
                    _animator.SetBool(_turnActiveBoolParam, false);
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
