using System.Collections;
using UnityEngine;

namespace TutorialProject.Framework.Utility {
    /// <summary>
    /// Different tweening methods available when moving.
    /// </summary>
    public enum TweeningSpaceMode {
        WorldSpace = 0, Local = 1
    }

    /// <summary>
    /// Serialized class that can be used to hold animation data that can be used
    /// to play an animation.
    /// </summary>
    [System.Serializable]
    public class AnimationPlayData {
        [SerializeField, Tooltip("The animation state name")] private string _stateName = "";
        [SerializeField, Tooltip("The layer that the state is on")] private int _layer = 0;
        [SerializeField, Range(0f, 1f), Tooltip("The normalized time transition duration if this animation should crossfade when played")] private float _transitionDuration = 0f;
        [SerializeField, Range(0f, 1f), Tooltip("The normalized time of the state from which to start the animation")] private float _startTime = 0f;
        [SerializeField, Tooltip("The max time (in seconds) to wait before the animation is to be considered a failure")] private float _maxTime = 10f;
        [SerializeField, Range(0f, 1.1f), Tooltip("The normalized time to wait for before the animation is considered complete (if set above 1 than that means the animation needs to actively transition into another state to end)")] private float _endNormalizedTime = 0.95f;

        public string StateName { get => _stateName; }
        public int StateHash { get; private set; } = 0;
        public bool IsValid { get { return StateHash != 0 || !string.IsNullOrEmpty(_stateName); } }
        public int Layer { get => _layer; }
        public float TransitionDuration { get => _transitionDuration; }
        public float StartTime { get => _startTime; }
        public bool CheckTransition { get => _transitionDuration > 0; }
        public float MaxTime { get => _maxTime; }
        public float EndNormalizedTime { get => _endNormalizedTime; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnimationPlayData"/> class.
        /// </summary>
        public AnimationPlayData() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnimationPlayData"/> class.
        /// </summary>
        /// <param name="stateName">Name of the state.</param>
        /// <param name="stateHash">The state hash.</param>
        /// <param name="layer">The layer.</param>
        public AnimationPlayData(string stateName, int layer, int stateHash, float endNormalizedTime, float maxWaitTime) {
            _stateName = stateName;
            _layer = layer;
            _endNormalizedTime = endNormalizedTime;
            _maxTime = maxWaitTime;

            StateHash = stateHash;
        }

        /// <summary>
        /// Calculates the state hash.
        /// </summary>
        public void CalculateStateHash() {
            StateHash = Animator.StringToHash(_stateName);
        }

        /// <summary>
        /// Checks whether or not this animation exists in the specified animator.
        /// </summary>
        /// <param name="animator">The animator.</param>
        /// <returns></returns>
        public bool CheckExists(Animator animator) {
            if (StateHash == 0) { CalculateStateHash(); }
            return animator.HasState(_layer, StateHash);
        }
    }

    /// <summary>
    /// Provides information about an animation that was/is being waited on.
    /// </summary>
    public class AnimationWaitInfo {
        public float Timer { get; set; } = 0f;
        public float NormalizedTime { get; set; } = 0f;
        public bool Success {
            get { return _success; }
            set {
                if (value) { _hasEnded = true; }
                _success = value;
            }
        }
        public bool HasEnded { get => _hasEnded; set => _hasEnded = value; }
        public System.Action<AnimationWaitInfo> EndCallback { get; private set; }

        private bool _success;
        private bool _hasEnded;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnimationWaitInfo"/> class.
        /// </summary>
        public AnimationWaitInfo() {
            EndCallback = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnimationWaitInfo"/> class.
        /// </summary>
        /// <param name="endCallback">The end callback.</param>
        public AnimationWaitInfo(System.Action<AnimationWaitInfo> endCallback) {
            EndCallback = endCallback;
        }

        /// <summary>
        /// Resets this instance.
        /// </summary>
        public void Reset() {
            Timer = 0;
            NormalizedTime = 0;

            _success = false;
            _hasEnded = false;
        }
    }


    /// <summary>
    /// Provides utility animation methods.
    /// </summary>
    public static class AnimationUtility {
        #region Animator Methods
        /// <summary>
        /// Plays an animation and then waits for an animation state to either no longer be playing or for it to reach a specified normalized time.
        /// </summary>
        /// <param name="animator">The animator.</param>
        /// <param name="playData">The play data.</param>
        /// <param name="info">The information.</param>
        /// <returns></returns>
        public static IEnumerator PlayAnimation(Animator animator, AnimationPlayData playData, AnimationWaitInfo info) {
            if (playData.StateHash == 0) {
                playData.CalculateStateHash();
            }
            return PlayAnimation(animator, playData.Layer, playData.StateHash, playData.TransitionDuration, playData.StartTime, playData.MaxTime, playData.EndNormalizedTime, info);
        }

        /// <summary>
        /// Plays an animation and then waits for an animation state to either no longer be playing or for it to reach a specified normalized time.
        /// </summary>
        /// <param name="animator">The animator.</param>
        /// <param name="layer">The layer.</param>
        /// <param name="stateHash">The state hash.</param>
        /// <param name="transitionDuration">Duration of the transition.</param>
        /// <param name="startTime">The start time.</param>
        /// <param name="maxTime">The maximum time.</param>
        /// <param name="endNormalizedTime">The end normalized time.</param>
        /// <param name="info">The information.</param>
        /// <returns></returns>
        public static IEnumerator PlayAnimation(Animator animator, int layer, int stateHash, float transitionDuration, float startTime, float maxTime, float endNormalizedTime, AnimationWaitInfo info) {
            if (transitionDuration > 0) {
                animator.CrossFade(stateHash, transitionDuration, layer, startTime);
            }
            else {
                animator.Play(stateHash, layer, startTime);
            }
            return WaitForAnimation(animator, layer, stateHash, transitionDuration > 0, maxTime, endNormalizedTime, info);
        }


        /// <summary>
        /// Waits for an animation state to either no longer be playing or for it to reach a specified normalized time.
        /// </summary>
        /// <param name="animator">The animator.</param>
        /// <param name="playData">The play data.</param>
        /// <param name="info">The information.</param>
        /// <returns></returns>
        public static IEnumerator WaitForAnimation(Animator animator, AnimationPlayData playData, AnimationWaitInfo info) {
            if (playData.StateHash == 0) {
                playData.CalculateStateHash();
            }
            return WaitForAnimation(animator, playData.Layer, playData.StateHash, playData.CheckTransition, playData.MaxTime, playData.EndNormalizedTime, info);
        }

        /// <summary>
        /// Waits for an animation state to either no longer be playing or for it to reach a specified normalized time.
        /// </summary>
        /// <param name="animator">The animator.</param>
        /// <param name="layer">The layer.</param>
        /// <param name="stateHash">The state hash.</param>
        /// <param name="checkTransition">if set to <c>true</c> [check transition].</param>
        /// <param name="maxTime">The maximum time.</param>
        /// <param name="endNormalizedTime">The end normalized time.</param>
        /// <param name="success">if set to <c>true</c> [success].</param>
        /// <returns></returns>
        public static IEnumerator WaitForAnimation(Animator animator, int layer, int stateHash, bool checkTransition, float maxTime, float endNormalizedTime, AnimationWaitInfo info) {
            info.Reset();

            while (!info.Success && info.Timer <= maxTime && animator.enabled) {
                yield return null;
                info.Timer += Time.deltaTime;

                if (checkTransition) {
                    // Once the transition has ended, allow us to move on
                    if (!animator.IsInTransition(layer)) { checkTransition = false; }
                }
                else {
                    // Check to see if the animation has ended or not
                    var animStateInfo = animator.GetCurrentAnimatorStateInfo(layer);
                    info.NormalizedTime = animStateInfo.normalizedTime;
                    if (animStateInfo.shortNameHash != stateHash || (animStateInfo.normalizedTime >= endNormalizedTime && endNormalizedTime <= 1f)) {
                        info.Success = true;
                    }
                }
            }

            info.HasEnded = true;
            info.EndCallback?.Invoke(info);
        }


        /// <summary>
        /// Waits for a specific animation to start and then waits for it to either no longer be
        /// playing or for it to reach a specified normalized time.
        /// </summary>
        /// <param name="animator">The animator.</param>
        /// <param name="playData">The play data.</param>
        /// <param name="info">The information.</param>
        /// <returns></returns>
        public static IEnumerator WaitForDelayedAnimation(Animator animator, AnimationPlayData playData, AnimationWaitInfo info) {
            if (playData.StateHash == 0) {
                playData.CalculateStateHash();
            }
            return WaitForDelayedAnimation(animator, playData.Layer, playData.StateHash, playData.CheckTransition, playData.MaxTime, playData.EndNormalizedTime, info);
        }

        /// <summary>
        /// Waits for a specific animation to start and then waits for it to either no longer be 
        /// playing or for it to reach a specified normalized time.
        /// </summary>
        /// <param name="animator">The animator.</param>
        /// <param name="layer">The layer.</param>
        /// <param name="stateHash">The state hash.</param>
        /// <param name="checkTransition">if set to <c>true</c> [check transition].</param>
        /// <param name="maxTime">The maximum time.</param>
        /// <param name="endNormalizedTime">The end normalized time.</param>
        /// <param name="info">The information.</param>
        /// <returns></returns>
        public static IEnumerator WaitForDelayedAnimation(Animator animator, int layer, int stateHash, bool checkTransition, float maxTime, float endNormalizedTime, AnimationWaitInfo info) {
            bool isPlaying = false;
            info.Reset();

            if (checkTransition) {
                // If checking transition, verify that we are not already playing the animation and just skip that process if so
                // otherwise we could get stuck waiting a long time
                var animStateInfo = animator.GetCurrentAnimatorStateInfo(layer);
                if (animStateInfo.shortNameHash == stateHash) {
                    checkTransition = false;
                }
            }

            while (!info.Success && info.Timer <= maxTime && animator.enabled) {
                yield return null;
                info.Timer += Time.deltaTime;

                if (checkTransition) {
                    // Once the transition has ended, allow us to move on
                    if (!animator.IsInTransition(layer)) { checkTransition = false; }
                }
                else {
                    var animStateInfo = animator.GetCurrentAnimatorStateInfo(layer);

                    // Check to see if we have reached the animation we want to wait for or not
                    if (!isPlaying) {
                        isPlaying = animStateInfo.shortNameHash == stateHash;
                    }
                    else {
                        info.NormalizedTime = animStateInfo.normalizedTime;
                        if (animStateInfo.shortNameHash != stateHash || (animStateInfo.normalizedTime >= endNormalizedTime && endNormalizedTime <= 1f)) {
                            info.Success = true;
                        }
                    }
                }
            }

            info.HasEnded = true;
            info.EndCallback?.Invoke(info);
        }


        /// <summary>
        /// Waits for the current animation to reach a specified normalized time.
        /// </summary>
        /// <param name="animator">The animator.</param>
        /// <param name="layer">The layer.</param>
        /// <param name="checkTransition">if set to <c>true</c> [check transition].</param>
        /// <param name="maxTime">The maximum time.</param>
        /// <param name="endNormalizedTime">The end normalized time.</param>
        /// <param name="info">The information.</param>
        /// <returns></returns>
        public static IEnumerator WaitForCurrentAnimation(Animator animator, int layer, bool checkTransition, float maxTime, float endNormalizedTime, AnimationWaitInfo info) {
            int stateHash = 0;
            bool hashObtained = false;
            info.Reset();

            while (!info.Success && info.Timer <= maxTime && animator.enabled) {
                yield return null;
                info.Timer += Time.deltaTime;

                if (checkTransition) {
                    // Once the transition has ended, allow us to move on
                    if (!animator.IsInTransition(layer)) { checkTransition = false; }
                }
                else {
                    // Check to see if the animation has ended or not
                    var animStateInfo = animator.GetCurrentAnimatorStateInfo(layer);
                    info.NormalizedTime = animStateInfo.normalizedTime;
                    if (!hashObtained) {
                        stateHash = animStateInfo.shortNameHash;
                        hashObtained = true;
                    }
                    if (stateHash != animStateInfo.shortNameHash || (endNormalizedTime <= 1f && animStateInfo.normalizedTime >= endNormalizedTime)) {
                        info.Success = true;
                    }
                }
            }

            info.HasEnded = true;
            info.EndCallback?.Invoke(info);
        }
        #endregion
    }
}
