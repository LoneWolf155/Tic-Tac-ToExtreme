using UnityEngine;
using System.Linq;
using TutorialProject.Framework.Events;

#if UNITY_EDITOR
using TutorialProject.Framework.Utility.Editor;
#endif

namespace TutorialProject.Game {
    /// <summary>
    /// Contains all shared data relevant to the game.
    /// </summary>
    /// <seealso cref="UnityEngine.ScriptableObject" />
    public class GlobalGameData : ScriptableObject {
        #region Declarations
        [System.Serializable]
        public class TriggeredGameEvents {
            public GameEvent onPlayerTurnStart;
            public GameEvent onPlayerTurnEnd;
        }

        [Header("Project Assets")]
        [SerializeField] private PlayerData[] _playersData = new PlayerData[] { };
        [SerializeField] private GamePowerup[] _powerups = new GamePowerup[] { };
        [SerializeField] private TriggeredGameEvents _triggeredEvents = new TriggeredGameEvents();

        public PlayerData[] PlayersData { get => _playersData; }
        public PlayerData CurrentPlayer { get; private set; }
        public GamePowerup[] AllPowerups { get => _powerups; }

        private int _nextPlayerIndex;
        #endregion

        #region ScriptableObject Overrides
#if UNITY_EDITOR
        /// <summary>
        /// Resets this instance.
        /// </summary>
        private void Reset() {
            _playersData = EditorUtility.GetScriptableObjectAssets<PlayerData>().OrderBy(p => p.PlayerNumber).ToArray();
            _powerups = EditorUtility.GetScriptableObjectAssets<GamePowerup>();

            EditorUtility.AutoFillEvents<TriggeredGameEvents>(ref _triggeredEvents);
        }
#endif
        #endregion

        #region Public Methods
        /// <summary>
        /// Starts the next player's turn.
        /// </summary>
        [ContextMenu("Shift Next Player")]
        public void ShiftToNextPlayer() {
            if (CurrentPlayer == null) {
                _nextPlayerIndex = 0;   // Start at index of 0
            }
            else if (CurrentPlayer != null) {
                // Trigger the event for ending the current player's turn
                _triggeredEvents.onPlayerTurnEnd?.Raise(this, CurrentPlayer);

                // Set for the next player and trigger the start
                _nextPlayerIndex += 1;
                if (_nextPlayerIndex >= _playersData.Length) {
                    _nextPlayerIndex = 0;
                }
            }

            CurrentPlayer = PlayersData[_nextPlayerIndex];
            _triggeredEvents.onPlayerTurnStart?.Raise(this, CurrentPlayer);
        }
        #endregion

        #region Context Menu
#if UNITY_EDITOR
        /// <summary>
        /// Creates an asset file for this class.
        /// </summary>
        [UnityEditor.MenuItem("Assets/Create/Custom/GlobalGameData", false, 0)]
        public static void CreateAsset() {
            CustomAssetUtil.CreateAsset<GlobalGameData>("GlobalGameData");
        }
#endif
        #endregion
    }
}
