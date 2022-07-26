using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TutorialProject.Game {
    public abstract class GamePowerup : ScriptableObject {
        #region Declarations
        #endregion

        #region ScriptableObject Overrides
        public abstract void Execute();
        #endregion
    }
}
