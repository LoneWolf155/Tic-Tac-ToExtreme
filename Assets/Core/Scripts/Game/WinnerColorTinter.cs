using System.Collections;
using UnityEngine;
using TutorialProject.Framework.Events;
using TutorialProject.Framework.Utility;

#if UNITY_EDITOR
using TutorialProject.Framework.Utility.Editor;
#endif

namespace TutorialProject.Game {
    public class WinnerColorTinter : MonoBehaviour, IGameEventListener {
        #region Declarations
        /// <summary>
        /// Different settings for different tinting effects.
        /// </summary>
        public enum TintMode {
            Instant = 0, Animate = 1
        }

        [System.Serializable]
        public class ListenedGameEvents {
            public GameEvent onGameStart;
            public GameEvent onGameEnd;
        }

        [System.Serializable]
        public class ParticleTinter {
            [SerializeField] private ParticleSystem _particle;

            public ParticleSystem Particle { get => _particle; }
            public Color OriginalColor { get; set; }
            public Color ActiveColor { get; set; }
            public Color TargetColor { get; set; }
        }

        [Header("Project Assets")]
        [SerializeField] private ListenedGameEvents _listenedEvents = new ListenedGameEvents();

        [Header("Tinting")]
        [SerializeField] private ParticleTinter[] _particleTinters;
        [SerializeField] private TintMode _tintMode = TintMode.Instant;
        [SerializeField, ShowWhen("_tintMode", TintMode.Animate)] private float _tintSpeed = 1f;

        private Coroutine _tintRoutine;
        private bool _assignedEvents;
        #endregion

        #region MonoBehavior Overrides
        /// <summary>
        /// Awakes this instance.
        /// </summary>
        private void Awake() {
            // Set the original colors
            for (int x = 0; x < _particleTinters.Length; x++) {
                var tinter = _particleTinters[x];
                tinter.OriginalColor = tinter.Particle.main.startColor.color;
            }
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
            EditorUtility.AutoFillEvents<ListenedGameEvents>(ref _listenedEvents);
        }
#endif
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
                // Ensure we tint back to the original color
                for (int x = 0; x < _particleTinters.Length; x++) {
                    var tinter = _particleTinters[x];
                    tinter.TargetColor = tinter.OriginalColor;
                }

                if (_tintRoutine != null) { StopCoroutine(_tintRoutine); }
                _tintRoutine = StartCoroutine(HandleTinting());
            }
            else if (gameEvent == _listenedEvents.onGameEnd) {
                Color color;  

                // If no data was passed then we will not tint
                if (args.Length > 0 && args[0] is PlayerData) {
                    color = (args[0] as PlayerData).Color.Value;
                }
                else {
                    return;
                }

                // Tint to the winning color
                for (int x = 0; x < _particleTinters.Length; x++) {
                    var tinter = _particleTinters[x];
                    tinter.TargetColor = color;
                }

                if (_tintRoutine != null) { StopCoroutine(_tintRoutine); }
                _tintRoutine = StartCoroutine(HandleTinting());
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

        #region Coroutines
        /// <summary>
        /// Handles the tinting.
        /// </summary>
        /// <returns></returns>
        private IEnumerator HandleTinting() {
            // Handle tinting based on our mode
            if (_tintMode == TintMode.Instant) {
                for (int x = 0; x < _particleTinters.Length; x++) {
                    var tinter = _particleTinters[x];
                    var settings = tinter.Particle.main;
                    settings.startColor = tinter.TargetColor;
                }
            }
            else if (_tintMode == TintMode.Animate) {
                float delta = 0f;

                do {
                    delta += Time.deltaTime * _tintSpeed;

                    for (int x = 0; x < _particleTinters.Length; x++) {
                        var tinter = _particleTinters[x];
                        var settings = tinter.Particle.main;
                        settings.startColor = Color.Lerp(tinter.ActiveColor, tinter.TargetColor, delta);
                    }

                    yield return null;
                } while (delta < 1f);
            }

            // Ensure all of the active colors are properly set
            for (int x = 0; x < _particleTinters.Length; x++) {
                var tinter = _particleTinters[x];
                tinter.ActiveColor = tinter.Particle.main.startColor.color;
            }

            _tintRoutine = null;
        }
        #endregion
    }
}
