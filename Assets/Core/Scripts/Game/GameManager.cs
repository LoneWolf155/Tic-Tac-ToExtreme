using System.Collections;
using System.Linq;
using UnityEngine;
using TutorialProject.Framework.Runtime;
using TutorialProject.Framework.Events;
using TutorialProject.Framework.Utility;

#if UNITY_EDITOR
using TutorialProject.Framework.Utility.Editor;
#endif

namespace TutorialProject.Game {
    /// <summary>
    /// Manages the tic-tac-toe game.
    /// </summary>
    /// <seealso cref="UnityEngine.MonoBehaviour" />
    [RequireComponent(typeof(GameBoard))]
    public sealed class GameManager : MonoBehaviour, IGameEventListener {
        #region Declarations
        public enum GameState {
            GameSetup = 0,
            GameRunning = 1,
            PlayerPickResult = 2,
            GameOver = 3
        }

        [System.Serializable]
        public class ListenedGameEvents {
            public GameEvent onBoardBlockClick;
            public GameEvent onBoardChange;
            public GameEvent startNewGame;
        }

        [System.Serializable]
        public class TriggeredGameEvents {
            public GameEvent onGameStart;
            public GameEvent onGameEnd;
        }

        [Header("Project Assets")]
        [SerializeField] private GlobalGameData _gameData = null;
        [SerializeField] private TriggeredGameEvents _triggeredEvents = new TriggeredGameEvents();
        [SerializeField] private ListenedGameEvents _listenedEvents = new ListenedGameEvents();

        [Header("Scene References")]
        [SerializeField] private GameBoard _gameBoard = null;

        public GameBoard GameBoard { get => _gameBoard; }
        public GameState CurrentState { get; private set; }

        private bool _assignedEvents;
        #endregion

        #region MonoBehavior Overrides
        /// <summary>
        /// Starts this instance.
        /// </summary>
        private void Start() {
            // Start up a new game
            StartNewGame();
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
            _gameBoard = GetComponent<GameBoard>();
            _gameData = EditorUtility.GetScriptableObjectAsset<GlobalGameData>(); ;

            EditorUtility.AutoFillEvents<TriggeredGameEvents>(ref _triggeredEvents);
            EditorUtility.AutoFillEvents<ListenedGameEvents>(ref _listenedEvents);
        }
#endif
        #endregion

        #region Game State Methods
        /// <summary>
        /// Starts a new game.
        /// </summary>
        private void StartNewGame() {
            // Reset back to a base state
            CurrentState = GameState.GameSetup;
            GameBoard.ResetBoard();
            _triggeredEvents.onGameStart?.Raise(this);
            CurrentState = GameState.GameRunning;

            // Start the next player's turn
            NextPlayerTurn();
        }

        /// <summary>
        /// Handles starting the next player's turn.
        /// </summary>
        private void NextPlayerTurn() {
            CurrentState = GameState.GameRunning;
            _gameData.ShiftToNextPlayer();
        }

        /// <summary>
        /// Ends the game.
        /// </summary>
        /// <param name="winner">The winner.</param>
        private void EndGame(PlayerData winner = null) {
            CurrentState = GameState.GameOver;
            if (winner != null) {
                winner.Score.SetValue(winner.Score.Value + 1);
                GameBoard.DrawWinLine();
            }

            _triggeredEvents.onGameEnd?.Raise(this, winner);
        }

        /// <summary>
        /// Checks to see if the game is over and, if so, ends it.
        /// </summary>
        /// <returns></returns>
        private bool CheckForGameEnd() {
            var result = GameBoard.CheckGameOver();
            if (result == GameBoard.GameOverResult.StillRunning) {
                return false;
            }
            else {
                PlayerData winner = null;

                if (result == GameBoard.GameOverResult.GameWon) {
                    // Determine our winner (on the event one isn't matched up, we will consider it a tie)
                    for (int x = 0; x < _gameData.PlayersData.Length; x++) {
                        if (GameBoard.LastWinnerText == _gameData.PlayersData[x].Symbol.Value) {
                            winner = _gameData.PlayersData[x];
                            break;
                        }
                    }
                }

                // End our game
                EndGame(winner);
                return true;
            }
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
            if (gameEvent == _listenedEvents.onBoardChange) {
                if (CurrentState != GameState.GameRunning) { return; }

                // Check to see if the board change has resulted in an end game state
                CheckForGameEnd();
            }
            else if (gameEvent == _listenedEvents.startNewGame) {
                StartNewGame();
            }
            else if (gameEvent == _listenedEvents.onBoardBlockClick) {
                if (CurrentState != GameState.GameRunning) { return; }

                // Player has selected a block so trigger our selection event
                StartCoroutine(HandlePlayerSelection(sender as BoardBlock));
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

        #region CoRoutines
        /// <summary>
        /// Handles the player selection.
        /// </summary>
        /// <param name="selectedBlock">The selected block.</param>
        /// <returns></returns>
        private IEnumerator HandlePlayerSelection(BoardBlock selectedBlock) {
            PlayerData currentPlayer = _gameData.CurrentPlayer;

            // Set our state to that of the player selection being handled
            CurrentState = GameState.PlayerPickResult;

            // Trigger and wait for the animation to complete
            selectedBlock.TriggerSelection(currentPlayer);
            while (selectedBlock.IsAnimating) {
                yield return null;
            }

            // Check to see if the game is over and, if not, start the next player's turn
            if (!CheckForGameEnd()) {
                NextPlayerTurn();
            }
        }
        #endregion
    }
}