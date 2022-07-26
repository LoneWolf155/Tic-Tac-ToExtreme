using UnityEngine;

namespace TutorialProject.Framework.Runtime {
    /// <summary>
    /// A runtime variable that is a float value.
    /// </summary>
    public class FloatVariable : RuntimeVariable<float> {
        #region Declarations
        public override float DefaultValue { get { return 0f; } }
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="FloatVariable"/> class.
        /// </summary>
        public FloatVariable() : base() { }
        #endregion

        #region Overrides
        /// <summary>
        /// Determines whether the specified value is equal to the active value or not.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// <c>true</c> if the specified value is equal; otherwise, <c>false</c>.
        /// </returns>
        public override bool IsEqual(float value) {
            return (Value == value);
        }
        #endregion

        #region Context Menu
#if UNITY_EDITOR
        /// <summary>
        /// Creates an asset file for this class.
        /// </summary>
        [UnityEditor.MenuItem("Assets/Create/Custom/Runtime/FloatVariable", false, 0)]
        public static void CreateAsset() {
            CustomAssetUtil.CreateAsset<FloatVariable>("floatVariable");
        }
#endif
        #endregion
    }
}
