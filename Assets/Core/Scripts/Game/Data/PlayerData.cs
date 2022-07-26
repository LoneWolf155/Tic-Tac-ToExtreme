using TutorialProject.Framework.Events;
using TutorialProject.Framework.Runtime;
using UnityEngine;

namespace TutorialProject.Game {
    /// <summary>
    /// Directs or contains to all data regarding a particular player.
    /// </summary>
    /// <seealso cref="UnityEngine.ScriptableObject" />
    public class PlayerData : ScriptableObject {
        #region Declarations
        [System.Serializable]
        public class TriggeredGameEvents {
            public GameEvent onPlayerPowerupChanged;
        }

        [Header("Project Asseets")]
        [SerializeField] private StringVariable _symbol;
        [SerializeField] private IntVariable _score;
        [SerializeField] private ColorVariable _color;
        [SerializeField] private TriggeredGameEvents _triggeredGameEvents;

        [Header("Settings")]
        [SerializeField] private int _playerNumber = 0;


        public int PlayerNumber { get => _playerNumber; }
        public StringVariable Symbol { get => _symbol; }
        public IntVariable Score { get => _score; }
        public ColorVariable Color { get => _color; }

        public GamePowerup CurrentPowerup { get; private set; }
        #endregion

        #region ScriptableObject Overrides
        #endregion

        #region Public Methods
        public void GivePowerup(GamePowerup powerup) {
            CurrentPowerup = powerup;
            _triggeredGameEvents.onPlayerPowerupChanged?.Raise(this, CurrentPowerup);
        }

        public void RemovePowerup() {
            CurrentPowerup = null;
            _triggeredGameEvents.onPlayerPowerupChanged?.Raise(this, null);
        }
        #endregion

        #region Context Menu
#if UNITY_EDITOR
        /// <summary>
        /// Creates an asset file for this class.
        /// </summary>
        [UnityEditor.MenuItem("Assets/Create/Custom/PlayerData", false, 0)]
        public static void CreateAsset() {
            CustomAssetUtil.CreateAsset<PlayerData>("playerData");
        }
#endif
        #endregion
    }
}
