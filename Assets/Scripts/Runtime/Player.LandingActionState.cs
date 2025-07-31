using UnityEngine;

namespace CharacterControlSample {
    /// <inheritdoc/>
    partial class Player {
        /// <summary>
        /// LandingActionState
        /// </summary>
        private class LandingActionState : ActionState {
            private int _actionIndex;
            
            /// <inheritdoc/>
            public override StateType Type => StateType.LandingAction;

            /// <inheritdoc/>
            protected override ActionInfo ActionInfo => Owner.landingActionInfos[_actionIndex];
            /// <inheritdoc/>
            protected override string ActionTagName => "Landing";

            /// <summary>
            /// コンストラクタ
            /// </summary>
            public LandingActionState(Player owner) : base(owner) {
            }

            /// <summary>
            /// セットアップ
            /// </summary>
            public void Setup(int actionIndex) {
                _actionIndex = Mathf.Clamp(actionIndex, 0, Owner.landingActionInfos.Length);
            }
        }
    }
}