using UnityEngine;

#if UNITY_EDITOR
using TutorialProject.Framework.Utility.Editor;
#endif

namespace TutorialProject.Game {
    [CreateAssetMenu(menuName = "Custom/Powerups/FlipSymbolsPower")]
    public class FlipSymbolsPowerup : GamePowerup {
        #region Declarations
        [Header("Project Assets")]
        [SerializeField] private GlobalGameData _gameData;
        #endregion

        #region Overrides
#if UNITY_EDITOR
        /// <summary>
        /// Resets this instance.
        /// </summary>
        private void Reset() {
            _gameData = EditorUtility.GetScriptableObjectAsset<GlobalGameData>();
        }
#endif

        /// <summary>
        /// Executes this powerup.
        /// </summary>
        [ContextMenu("Execute")]
        public override void Execute() {
            if (!Application.isPlaying) {
                Debug.LogWarning("Game has to be running to execute!", this);
                return;
            }

            string[] symbols = new string[_gameData.PlayersData.Length];

            // Grab all of our symbols
            for (int x = 0; x < _gameData.PlayersData.Length; x++) {
                symbols[x] = _gameData.PlayersData[x].Symbol.Value;
            }

            // Set each player's symbol to that of the next player's
            for (int x = 0; x < _gameData.PlayersData.Length; x++) {
                if (x >= _gameData.PlayersData.Length - 1) {
                    _gameData.PlayersData[x].Symbol.SetValue(symbols[0]);
                }
                else {
                    _gameData.PlayersData[x].Symbol.SetValue(symbols[x + 1]);
                }
            }
        }
        #endregion
    }
}
