using TMPro;
using UnityEngine;

namespace TutorialProject.Effect {
    /// <summary>
    /// A simple script that creates a laser-like effect.
    /// </summary>
    /// <seealso cref="UnityEngine.MonoBehaviour" />
    public class LaserEffect : MonoBehaviour {
        #region Declarations
        public enum TargetAxis {
            X = 0, Y = 1, Z = 2
        }

        [Header("Line Renderer")]
        [SerializeField] private LineRenderer _lineRenderer = null;
        [SerializeField] private int _targetIndex = 1;

        [Header("Animation")]
        [SerializeField] private TargetAxis _animationAxis = TargetAxis.X;
        [SerializeField] private float _speed = 0.8f;
        [SerializeField] private float _minAxis = -15f;
        [SerializeField] private float _maxAxis = 15f;
        [SerializeField, Tooltip("Curve that defines the animation, must return a value between 0 and 1")] private AnimationCurve _curve = new AnimationCurve();

        private TextMeshProUGUI _text;
        private Vector3 _defaultPos;
        private float _time;
        private float _direction;
        #endregion

        #region MonoBehavior Overrides
        /// <summary>
        /// Awakes this instance.
        /// </summary>
        private void Awake() {
            _time = Random.Range(0f, 1f);
            _direction = Mathf.Abs(Random.Range(-1f, 1f));
            _defaultPos = _lineRenderer.GetPosition(_targetIndex);
        }

#if UNITY_EDITOR
        /// <summary>
        /// Resets this instance.
        /// </summary>
        private void Reset() {
            _lineRenderer = GetComponent<LineRenderer>();
        }
#endif

        /// <summary>
        /// Updates this instance.
        /// </summary>
        private void Update() {
            // Continue our timer
            _time += _direction * _speed * Time.deltaTime;
            if (_time > 1f) {
                _direction = -1;
            }
            else if (_time < 0) {
                _direction = 1;
            }

            // Evaluate the position value from the curve, determine our position between the max/min, and then apply it
            float pos = _curve.Evaluate(_time);
            pos = Mathf.Lerp(_minAxis, _maxAxis, pos);
            switch (_animationAxis) {
                case TargetAxis.X:
                    _lineRenderer.SetPosition(_targetIndex, new Vector3(pos, _defaultPos.y, _defaultPos.z));
                    break;
                case TargetAxis.Y:
                    _lineRenderer.SetPosition(_targetIndex, new Vector3(_defaultPos.x, pos, _defaultPos.z));
                    break;
                case TargetAxis.Z:
                    _lineRenderer.SetPosition(_targetIndex, new Vector3(_defaultPos.x, _defaultPos.y, pos));
                    break;
            }
        }
        #endregion
    }
}