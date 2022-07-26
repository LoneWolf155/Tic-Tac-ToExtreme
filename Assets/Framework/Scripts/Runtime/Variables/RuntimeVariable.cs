using UnityEngine;
using TutorialProject.Framework.Events;
using TutorialProject.Framework.Utility;

namespace TutorialProject.Framework.Runtime {
    /// <summary>
    /// Base class for runtime variables that provides base implementation and allows for easy grouping of multiple types.
    /// </summary>
    /// <seealso cref="UnityEngine.ScriptableObject" />
    public abstract class RuntimeVariable : ScriptableObject {
        #region Declarations
        [Header("General")]
        [SerializeField, StringIDValidation, Tooltip("An ID that can be used to identify this variable")] private string _id = "";
        [SerializeField, TextArea, Tooltip("A description on what this variable is for")] private string _description = "";
        [SerializeField, Tooltip("If true, value is reset back to the base value on start")] private bool _clearOnAwake = true;

        public string ID { get => _id; }
        public string Description { get => _description; }
        public virtual GameEvent ValueChangedGameEvent { get; protected set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="RuntimeVariable"/> class.
        /// </summary>
        public RuntimeVariable() { }
        #endregion

        #region ScriptableObject Overrides
        /// <summary>
        /// Awakes this instance.
        /// </summary>
        protected virtual void Awake() {
            if (Application.isEditor) { return; }

            if (_clearOnAwake) {
                ResetValue();
            }
        }
        #endregion

        #region Abstract Methods
        /// <summary>
        /// Gets the value of this runtime variable as an object.
        /// </summary>
        /// <returns></returns>
        public abstract object GetObjectValue();

        /// <summary>
        /// Resets the value back to the base value.
        /// </summary>
        protected abstract void ResetValue();

        /// <summary>
        /// Gets the type of this variable's value.
        /// </summary>
        /// <returns></returns>
        public abstract System.Type GetValueType();
        #endregion

        #region Context Menu
#if UNITY_EDITOR
        /// <summary>
        /// Creates a changed game event for this (editor only).
        /// </summary>
        [ContextMenu("Create Changed Game Event")]
        protected virtual void CreateChangedGameEvent() {
            ValueChangedGameEvent = CustomAssetUtil.CreateAsset<GameEvent>($"On{this.name}Changed");
            ValueChangedGameEvent._args = new string[] { $"<{{{GetValueType().Name}}} value>" };
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
        #endregion
    }

    /// <summary>
    /// A runtime variable of a specified type that can be referenced as an asset within the project.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="UnityEngine.ScriptableObject" />
    public abstract class RuntimeVariable<T> : RuntimeVariable {
        #region Declarations
        [SerializeField] private T _baseValue;
        [SerializeField, VisibleRuntimeOnly] private T _value;

        [Header("Game Event")]
        [SerializeField, Tooltip("Game event that fires when this value is changed")] protected GameEvent _valueChangedGameEvent;
        [SerializeField, Tooltip("If true, the event is triggered even if the value being set is the same")] private bool _triggerEventIfSame = false;
        [SerializeField, Tooltip("If true, the event is triggered when the value is changed through the editor")] private bool _triggerEventOnEditorChange = true;

        public T Value { get => _value; protected set => _value = value; }
        public abstract T DefaultValue { get; }
        public override GameEvent ValueChangedGameEvent { get { return _valueChangedGameEvent; } protected set { _valueChangedGameEvent = value; } }
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="RuntimeVariable{T}"/> class.
        /// </summary>
        public RuntimeVariable() {
            _baseValue = DefaultValue;
            _value = DefaultValue;
        }
        #endregion

        #region ScriptableObject Overrides
#if UNITY_EDITOR
        /// <summary>
        /// Called when [validate].
        /// </summary>
        private void OnValidate() {
            if (!Application.isPlaying) {
                _value = _baseValue;
                return; 
            }

            if (_triggerEventOnEditorChange) {
                _valueChangedGameEvent?.Raise(this, _value);
            }
        }
#endif
        #endregion

        #region Shared Methods
        /// <summary>
        /// Gets the variable's value. This method exists to help with visual scripting.
        /// </summary>
        /// <param name="varAsset">The variable asset.</param>
        /// <returns></returns>
        public static T GetVarValue(RuntimeVariable<T> varAsset) {
            return varAsset.Value;
        }

        /// <summary>
        /// Sets the variable's value. This method exists to help with visual scripting.
        /// </summary>
        /// <param name="varAsset">The variable asset.</param>
        /// <param name="value">The value.</param>
        public static void SetVarValue(RuntimeVariable<T> varAsset, T value) {
            varAsset.SetValue(value);
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Gets the value of this runtime variable as an object. Should only be used if the type of this variable is unknown.
        /// </summary>
        /// <returns></returns>
        public override object GetObjectValue() {
            return _value;
        }

        /// <summary>
        /// Sets this value.
        /// </summary>
        /// <param name="newValue">The new value.</param>
        public virtual void SetValue(T newValue) {
            bool trigger = (_triggerEventIfSame || !IsEqual(newValue));

            _value = newValue;

            if (trigger) { _valueChangedGameEvent?.Raise(this, _value); }
        }

        /// <summary>
        /// Gets the type of this variable's value.
        /// </summary>
        /// <returns></returns>
        public override System.Type GetValueType() {
            return typeof(T);
        }

        /// <summary>
        /// Determines whether the specified value is equal to the active value or not.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        ///   <c>true</c> if the specified value is equal; otherwise, <c>false</c>.
        /// </returns>
        public abstract bool IsEqual(T value);
        #endregion

        #region Protected Methods
        /// <summary>
        /// Resets the value back to the base value.
        /// </summary>
        protected override void ResetValue() {
            _value = _baseValue;
        }
        #endregion
    }
}
