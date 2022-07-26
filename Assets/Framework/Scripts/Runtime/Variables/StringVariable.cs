using UnityEngine;

namespace TutorialProject.Framework.Runtime {
    /// <summary>
    /// A runtime variable that is a string value.
    /// </summary>
    public class StringVariable : RuntimeVariable<string> {
        #region Declarations
        public override string DefaultValue { get { return ""; } }
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="StringVariable"/> class.
        /// </summary>
        public StringVariable() : base() { }
        #endregion

        #region Overrides
        /// <summary>
        /// Determines whether the specified value is equal to the active value or not.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// <c>true</c> if the specified value is equal; otherwise, <c>false</c>.
        /// </returns>
        public override bool IsEqual(string value) {
            return (Value == value);
        }
        #endregion

        #region Context Menu
#if UNITY_EDITOR
        /// <summary>
        /// Creates an asset file for this class.
        /// </summary>
        [UnityEditor.MenuItem("Assets/Create/Custom/Runtime/StringVariable", false, 0)]
        public static void CreateAsset() {
            CustomAssetUtil.CreateAsset<StringVariable>("stringVariable");
        }
#endif
        #endregion
    }
}
