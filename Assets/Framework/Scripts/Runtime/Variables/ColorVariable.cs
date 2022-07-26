using UnityEngine;

namespace TutorialProject.Framework.Runtime {
    /// <summary>
    /// A runtime variable that is a color value.
    /// </summary>
    public class ColorVariable : RuntimeVariable<Color> {
        #region Declarations
        public override Color DefaultValue { get { return Color.white; } }
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="FloatVariable"/> class.
        /// </summary>
        public ColorVariable() : base() { }
        #endregion

        #region Overrides
        /// <summary>
        /// Determines whether the specified value is equal to the active value or not.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// <c>true</c> if the specified value is equal; otherwise, <c>false</c>.
        /// </returns>
        public override bool IsEqual(Color value) {
            return (Value == value);
        }
        #endregion

        #region Context Menu
#if UNITY_EDITOR
        /// <summary>
        /// Creates an asset file for this class.
        /// </summary>
        [UnityEditor.MenuItem("Assets/Create/Custom/Runtime/ColorVariable", false, 0)]
        public static void CreateAsset() {
            CustomAssetUtil.CreateAsset<ColorVariable>("colorVariable");
        }
#endif
        #endregion
    }
}
