using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TutorialProject.Framework.Utility;

#if UNITY_EDITOR
using TutorialProject.Framework.Utility.Editor;
#endif

namespace TutorialProject.Game {
    [CreateAssetMenu(fileName = "ChangePlayerDataPowerup", menuName = "Custom/Powerups/ChangePlayerDataPowerup")]
    public class ChangePlayerDataPowerup : GamePowerup {
        #region Declarations
        public enum TargetPlayer { 
            Current = 0, Others = 1
        }
        public enum TargetData {
            Score = 0, Color = 1, Symbol = 2
        }

        [Header("Project Assets")]
        [SerializeField] private GlobalGameData _gameData;

        [Header("Settings")]
        [SerializeField] private TargetPlayer _targetPlayer = TargetPlayer.Current;
        [SerializeField] private TargetData _targetData = TargetData.Score;
        [SerializeField, ShowWhen("_targetData", TargetData.Score)] private int _scoreAdjust = 0;
        [SerializeField, ShowWhen("_targetData", TargetData.Color)] private Color _targetColor = Color.white;
        [SerializeField, ShowWhen("_targetData", TargetData.Symbol)] private string _targetSymbol = "";
        #endregion

        #region ScriptableObject Overrides
#if UNITY_EDITOR
        private void Reset() {
            _gameData = EditorUtility.GetScriptableObjectAsset<GlobalGameData>();
        }
#endif
        #endregion

        #region GamePowerup Overrides
        public override void Execute() {
            List<PlayerData> players = new List<PlayerData>();

            if (_targetPlayer == TargetPlayer.Current) {
                if (_gameData.CurrentPlayer != null) {
                    players.Add(_gameData.CurrentPlayer);
                }
            }
            else {
                foreach (var player in _gameData.PlayersData) {
                    if (player != _gameData.CurrentPlayer) {
                        players.Add(player);
                    }
                }
            }

            switch (_targetData) {
                case TargetData.Score:
                    foreach (var player in players) {
                        player.Score.SetValue(player.Score.Value + _scoreAdjust);
                    }
                    break;
                case TargetData.Color:
                    foreach (var player in players) {
                        player.Color.SetValue(_targetColor);
                    }
                    break;
                case TargetData.Symbol:
                    foreach (var player in players) {
                        player.Symbol.SetValue(_targetSymbol);
                    }
                    break;
            }
        }
        #endregion
    }
}
