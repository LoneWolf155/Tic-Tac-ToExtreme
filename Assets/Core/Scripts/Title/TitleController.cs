using UnityEngine;
using TutorialProject.Framework.Utility;

namespace TutorialProject.Title {
    /// <summary>
    /// Handles the title screen.
    /// </summary>
    /// <seealso cref="UnityEngine.MonoBehaviour" />
    public class TitleController : MonoBehaviour {
        #region Declarations
        [Header("Scene References")]
        [SerializeField] private Animator _titleAnimator;

        [Header("Settings")]
        [SerializeField] private string _gameSceneName = "GameScene";
        [SerializeField] private AnimationPlayData _startingAnimation;
        [SerializeField] private AnimationPlayData _endingAnimation;

        private bool _canStart;
        #endregion

        #region MonoBehavior Overrides
        /// <summary>
        /// Awakes this instance.
        /// </summary>
        private void Start() {
            // Play our initial animation and, when it ends, allow the game to start
            AnimationWaitInfo waitInfo = new AnimationWaitInfo((info) => {
                _canStart = true;
            });
            StartCoroutine(AnimationUtility.PlayAnimation(_titleAnimator, _startingAnimation, waitInfo));
        }

        /// <summary>
        /// Updates this instance.
        /// </summary>
        private void Update() {
            if (_canStart && Input.GetMouseButtonUp(0)) {
                _canStart = false;

                // Play the animation and load the game scene when it is done
                AnimationWaitInfo waitInfo = new AnimationWaitInfo((info) => {
                    UnityEngine.SceneManagement.SceneManager.LoadScene(_gameSceneName, UnityEngine.SceneManagement.LoadSceneMode.Single);
                });
                StartCoroutine(AnimationUtility.PlayAnimation(_titleAnimator, _endingAnimation, waitInfo));
            }
        }
        #endregion
    }
}
