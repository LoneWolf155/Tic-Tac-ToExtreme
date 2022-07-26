using System.Collections.Generic;
using UnityEngine;
using TutorialProject.Framework.Events;
using TutorialProject.Framework.Utility;

#if UNITY_EDITOR
using TutorialProject.Framework.Utility.Editor;
#endif

namespace TutorialProject.Game {
    /// <summary>
    /// Manages the tic-tac-toe game board.
    /// </summary>
    /// <seealso cref="UnityEngine.MonoBehaviour" />
    public class GameBoard : MonoBehaviour {
        #region Declarations
        public enum GameOverResult {
            StillRunning = 0,
            GameTie = 1,
            GameWon = 2
        }

        [System.Serializable]
        public class TriggeredGameEvents {
            public GameEvent onBoardChange;
        }

        [Header("Project Assets")]
        [SerializeField] private GlobalGameData _gameData;
        [SerializeField] private LineRenderer _dividerPrefab = null;
        [SerializeField] private LineRenderer _winLinePrefab = null;
        [SerializeField] private BoardBlock _boardBlockPrefab = null;
        [SerializeField] private TriggeredGameEvents _triggeredEvents = new TriggeredGameEvents();

        [Header("Scene References")]
        [SerializeField] private Transform _dividerParent;
        [SerializeField] private Transform _blockParent;

        [Header("Board Settings")]
        [SerializeField] private float _size = 9f;
        [SerializeField] private Vector2Int _defaultCount = new Vector2Int(3, 3);
        [SerializeField, VisibleRuntimeOnly] private Vector2Int _count = new Vector2Int(3, 3);
        [SerializeField] private float _blockBaseSize = 2f;
        [SerializeField] private float _blockScaleRatio = 0.666f;
        [SerializeField] private float _winLineZOffset = -2f;

        [Header("Powerup Settings")]
        [SerializeField] private int _minPowerupCount = 1;
        [SerializeField] private int _maxPowerupCount = 2;

        public int BlocksWidth { get { return _blockCounts.x; } }
        public int BlocksHeight { get { return _blockCounts.y; } }

        public string LastWinnerText { get; private set; }
        public BoardBlock[] LastWinnerBlocks { get; private set; }

        private Vector2Int _blockCounts;
        private LineRenderer _winLine;
        private LineRenderer[] _dividers = new LineRenderer[] { };
        private BoardBlock[,] _blockArray = new BoardBlock[,] { };
        #endregion

        #region MonoBehavior Overrides
#if UNITY_EDITOR
        /// <summary>
        /// Resets this instance.
        /// </summary>
        private void Reset() {
            EditorUtility.AutoFillEvents<TriggeredGameEvents>(ref _triggeredEvents);
        }
#endif
        #endregion

        #region Board Methods
        /// <summary>
        /// Resets the board back to its default state.
        /// </summary>
        [ContextMenu("Reset Board")]
        public void ResetBoard() {
            if (!Application.isPlaying) { return; }

            List<LineRenderer> oldDividers = new List<LineRenderer>();
            List<BoardBlock> oldBlocks = new List<BoardBlock>();

            // Collect our existing stuff so we can re-use them
            foreach (var line in _dividers) {
                if (line == null) { continue; }
                oldDividers.Add(line);
            }
            foreach (var block in _blockArray) {
                if (block == null) { continue; }
                block.ResetBlock();
                oldBlocks.Add(block);
            }

            // Reset to our defaults and set up the arrays
            LastWinnerText = "";
            LastWinnerBlocks = null;
            if (_winLine != null) {
                _winLine.gameObject.SetActive(false);
            }
            _count = _defaultCount;

            _dividers = new LineRenderer[_count.x + _count.y - 2];  // Lines are between blocks and will always count 1 less per direction
            _blockArray = new BoardBlock[_count.x, _count.y];

            // Fill in some of the old stuff into our arrays then destroy the rest
            if (oldDividers.Count > 0) {
                for (int x = 0; x < _dividers.Length; x++) {
                    _dividers[x] = oldDividers[0];
                    oldDividers.RemoveAt(0);
                    if (oldDividers.Count == 0) { break; }
                }
                while (oldDividers.Count > 0) {
                    Destroy(oldDividers[0].gameObject);
                    oldDividers.RemoveAt(0);
                }
            }
            if (oldBlocks.Count > 0) {
                for (int x = 0; x < _blockArray.GetLength(0); x++) {
                    for (int y = 0; y < _blockArray.GetLength(1); y++) {
                        _blockArray[x, y] = oldBlocks[0];
                        oldBlocks.RemoveAt(0);
                        if (oldBlocks.Count == 0) { break; }
                    }
                    if (oldBlocks.Count == 0) { break; }
                }
                while (oldBlocks.Count > 0) {
                    Destroy(oldBlocks[0].gameObject);
                    oldBlocks.RemoveAt(0);
                }
            }

            // Now refresh the board which ensures it is all completely built
            RefreshBoard();

            // Determine the powerups we are going to assign
            List<GamePowerup> availPowerups = new List<GamePowerup>();
            int rand = Random.Range(_minPowerupCount, _maxPowerupCount + 1);
            for (int x = 0; x < rand; x++) {
                availPowerups.Add(_gameData.AllPowerups[Random.Range(0, _gameData.AllPowerups.Length)]);
            }

            // Assign powerups to our blocks
            foreach (GamePowerup powerup in availPowerups) {
                _blockArray[Random.Range(0, _blockArray.GetLength(0)), Random.Range(0, _blockArray.GetLength(1))].AvailablePowerup = powerup;
            }
        }

        /// <summary>
        /// Sets a new size for the board.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        public void SetBoardSize(int width, int height) {
            // Create new arrays for the new sizes
            _count = new Vector2Int(width, height);
            var newDividerArr = new LineRenderer[_count.x + _count.y - 2];  // Lines are between blocks and will always count 1 less per direction
            var newBlockArr = new BoardBlock[_count.x, _count.y];

            // Add our existing stuff into the new arrays, if they don't fit than destroy them
            // NOTE: Could use System.Array methods for the copy but I decided to do it myself due to the deletion
            for (int x=0; x < _dividers.Length; x++) {
                if ( x < newDividerArr.Length) {
                    newDividerArr[x] = _dividers[x];
                }
                else {
                    Destroy(_dividers[x].gameObject);
                }
            }
            for (int x = 0; x < _blockArray.GetLength(0); x++) {
                for (int y = 0; y < _blockArray.GetLength(1); y++) {
                    if (x < newBlockArr.GetLength(0) && y < newBlockArr.GetLength(1)) {
                        newBlockArr[x, y] = _blockArray[x, y];
                    }
                    else {
                        Destroy(_blockArray[x, y].gameObject);
                    }
                }
            }

            _dividers = newDividerArr;
            _blockArray = newBlockArr;

            // Now refresh (this will build in any missing slots for us)
            RefreshBoard();
        }

        /// <summary>
        /// Changes the size of the board to the current count, for use in the editor for testing.
        /// </summary>
        [ContextMenu("Update Size To Count")]
        private void ChangeSizeToCount() {
            if (!Application.isPlaying) { return; }

            SetBoardSize(_count.x, _count.y);
        }

        /// <summary>
        /// Refreshes the board by ensuring that each divider and block exists.
        /// </summary>
        [ContextMenu("Refresh Board")]
        private void RefreshBoard() {
            if (!Application.isPlaying) { return; }

            // NOTE: This method is a bunch of math that I worked out on paper so it isn't really documented well

            _blockCounts = new Vector2Int(_blockArray.GetLength(0), _blockArray.GetLength(1));

            // Ensure that dividers and blocks exists in each slot
            for (int x = 0; x < _dividers.Length; x++) {
                LineRenderer divider = _dividers[x];
                if (divider == null) {
                    divider = Instantiate(_dividerPrefab);
                    divider.transform.SetParent(_dividerParent);
                    _dividers[x] = divider;
                }
                divider.transform.SetSiblingIndex(x);
            }
            for (int x = 0; x < _blockArray.GetLength(0); x++) {
                for (int y = 0; y < _blockArray.GetLength(1); y++) {
                    BoardBlock block = _blockArray[x, y];
                    if (block == null) {
                        block = Instantiate(_boardBlockPrefab);
                        block.transform.SetParent(_blockParent);
                        _blockArray[x, y] = block;
                    }
                }
            }

            // Position all of our dividers
            float startPos = -(_size / 2);
            float lineAdjustment = _size / _blockCounts.y;
            float workingPosition = startPos + lineAdjustment;
            for (int x = 0; x < _blockCounts.y - 1; x++) {                    // Horizontal dividers
                LineRenderer divider = _dividers[x];
                divider.SetPosition(0, new Vector3(-_size / 2, 0, 0));
                divider.SetPosition(1, new Vector3(_size / 2, 0, 0));

                divider.transform.position = new Vector3(0, workingPosition, 0);
                workingPosition += lineAdjustment;
            }
            lineAdjustment = _size / _blockCounts.x;
            workingPosition = -(_size / 2) + lineAdjustment;
            for (int x = _count.y - 1; x < _dividers.Length; x++) {     // Vertical dividers
                LineRenderer divider = _dividers[x];
                divider.SetPosition(0, new Vector3(0, -_size / 2, 0));
                divider.SetPosition(1, new Vector3(0, _size / 2, 0));

                divider.transform.position = new Vector3(workingPosition, 0, 0);
                workingPosition += lineAdjustment;
            }

            // Position all of our blocks
            for (int x = 0; x < _blockCounts.x; x++) {
                float xPos = startPos + (_size / _blockCounts.x / 2) + (_size / _blockCounts.x * x);

                for (int y = 0; y < _blockCounts.y; y++) {
                    float yPos = startPos + (_size / _blockCounts.y / 2) + (_size / _blockCounts.y * y);

                    BoardBlock block = _blockArray[x, y];
                    block.transform.position = new Vector3(xPos, yPos, 0f);
                    block.transform.localScale = new Vector3(_blockBaseSize / (_blockCounts.x * _blockScaleRatio), _blockBaseSize / (_blockCounts.y * _blockScaleRatio), 3 * _blockScaleRatio);    // Z can just be a default
                }
            }

            // Since the board has changed, run the event
            _triggeredEvents.onBoardChange?.Raise(this);
        }
        #endregion

        #region Game Over Methods
        /// <summary>
        /// Checks the board to see if the game has ended or not.
        /// </summary>
        /// <returns></returns>
        public GameOverResult CheckGameOver() {
            // NOTE: This method is a bit sloppy but it gets the job done
            List<BoardBlock> winnerBlocks = new List<BoardBlock>();
            bool hasNoEmpty = true;
            string winnerText = "";

            // Check for vertical
            for (int x = 0; x < _blockArray.GetLength(0); x++) {
                if (!string.IsNullOrEmpty(winnerText)) { break; }
                winnerBlocks.Clear();

                for (int y = 0; y < _blockArray.GetLength(1); y++) {
                    var block = _blockArray[x, y];
                    if (!block.Selected) {
                        hasNoEmpty = false;
                        winnerText = "";
                        break;
                    }
                    else if (y == 0) {
                        winnerText = block.CurrentValue;
                    }
                    else if (winnerText != block.CurrentValue) {
                        winnerText = "";
                    }
                    else {
                        winnerBlocks.Add(block);
                    }
                }
            }
            if (!string.IsNullOrEmpty(winnerText)) {
                LastWinnerText = winnerText;
                LastWinnerBlocks = winnerBlocks.ToArray();
                return GameOverResult.GameWon;
            }
            else if (hasNoEmpty) {
                return GameOverResult.GameTie;
            }

            // Check for horizontal
            for (int y = 0; y < _blockArray.GetLength(0); y++) {
                if (!string.IsNullOrEmpty(winnerText)) { break; }
                winnerBlocks.Clear();

                for (int x = 0; x < _blockArray.GetLength(1); x++) {
                    var block = _blockArray[x, y];
                    if (!block.Selected) {
                        winnerText = "";
                        break;
                    }
                    else if (x == 0) {
                        winnerText = block.CurrentValue;
                    }
                    else if (winnerText != block.CurrentValue) {
                        winnerText = "";
                        break;
                    }

                    winnerBlocks.Add(block);
                }
            }
            if (!string.IsNullOrEmpty(winnerText)) {
                LastWinnerText = winnerText;
                LastWinnerBlocks = winnerBlocks.ToArray();
                return GameOverResult.GameWon;
            }

            // Check for cross wins (only possible if counts match)
            if (_blockArray.GetLength(0) == _blockArray.GetLength(1)) {
                winnerBlocks.Clear();
                for (int xy = 0; xy < _blockArray.GetLength(0); xy++) {
                    var block = _blockArray[xy, xy];
                    if (!block.Selected) {
                        winnerText = "";
                        break;
                    }
                    else if (xy == 0) {
                        winnerText = block.CurrentValue;
                    }
                    else if (winnerText != block.CurrentValue) {
                        winnerText = "";
                        break;
                    }
                    winnerBlocks.Add(block);
                }
                if (!string.IsNullOrEmpty(winnerText)) {
                    LastWinnerText = winnerText;
                    LastWinnerBlocks = winnerBlocks.ToArray();
                    return GameOverResult.GameWon;
                }

                winnerBlocks.Clear();
                for (int xy = 0; xy < _blockArray.GetLength(0); xy++) {
                    var block = _blockArray[xy, _blockArray.GetLength(0) - xy - 1];
                    if (!block.Selected) {
                        winnerText = "";
                        break;
                    }
                    else if (xy == 0) {
                        winnerText = block.CurrentValue;
                    }
                    else if (winnerText != block.CurrentValue) {
                        winnerText = "";
                        break;
                    }
                    winnerBlocks.Add(block);
                }
                if (!string.IsNullOrEmpty(winnerText)) {
                    LastWinnerText = winnerText;
                    LastWinnerBlocks = winnerBlocks.ToArray();
                    return GameOverResult.GameWon;
                }
            }
            return GameOverResult.StillRunning;
        }

        /// <summary>
        /// Draws a line on the last winning cubes.
        /// </summary>
        /// <param name="winColor">Color of the line.</param>
        public void DrawWinLine() {
            if (LastWinnerBlocks.Length == 0) { return; }

            // Ensure we have a win line to work with
            if (_winLine == null) {
                _winLine = Instantiate(_winLinePrefab);
                _winLine.transform.SetParent(transform);
                _winLine.transform.localPosition = Vector3.zero;
            }
            else {
                _winLine.gameObject.SetActive(true);
            }

            // Collect positions for us to show the win line on
            Vector3[] positions = new Vector3[LastWinnerBlocks.Length];
            for (int x = 0; x < LastWinnerBlocks.Length; x++) {
                positions[x] = LastWinnerBlocks[x].transform.position;
                positions[x].z = _winLineZOffset;
            }

            _winLine.positionCount = positions.Length;
            _winLine.SetPositions(positions);
        }
        #endregion
    }
}
