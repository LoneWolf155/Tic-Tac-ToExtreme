using UnityEngine;

namespace TutorialProject.Framework.Runtime {
    /// <summary>
    /// A runtime variable that is a int value.
    /// </summary>
    /// <seealso cref="TutorialProject.Framework.Runtime.RuntimeVariable{System.Single}" />
    public class IntVariable : RuntimeVariable<int> {
        #region Declarations
        public override int DefaultValue { get { return 0; } }
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="IntVariable"/> class.
        /// </summary>
        public IntVariable() : base() { }
        #endregion

        #region Overrides
        /// <summary>
        /// Determines whether the specified value is equal to the active value or not.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// <c>true</c> if the specified value is equal; otherwise, <c>false</c>.
        /// </returns>
        public override bool IsEqual(int value) {
            return (Value == value);
        }
        #endregion

        #region Context Menu
#if UNITY_EDITOR
        /// <summary>
        /// Creates an asset file for this class.
        /// </summary>
        [UnityEditor.MenuItem("Assets/Create/Custom/Runtime/IntVariable", false, 0)]
        public static void CreateAsset() {
            CustomAssetUtil.CreateAsset<IntVariable>("intVariable");
        }
#endif
        #endregion
    }
}
