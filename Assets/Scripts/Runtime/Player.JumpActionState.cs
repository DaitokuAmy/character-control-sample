namespace CharacterControlSample {
    /// <inheritdoc/>
    partial class Player {
        /// <summary>
        /// JumpActionState
        /// </summary>
        private class JumpActionState : ActionState {
            /// <inheritdoc/>
            public override StateType Type => StateType.JumpAction;

            /// <inheritdoc/>
            protected override ActionInfo ActionInfo => Owner.jumpActionInfo;
            /// <inheritdoc/>
            protected override string ActionTagName => "Jump";

            /// <summary>
            /// コンストラクタ
            /// </summary>
            public JumpActionState(Player owner) : base(owner) {
            }

            /// <inheritdoc/>
            public override void OnEnter() {
                base.OnEnter();
                
                // 重力を消す
                Owner._gravitySpeed = 0.0f;
                Owner.GravityScale = 0.0f;
            }

            /// <inheritdoc/>
            public override void OnUpdate(float deltaTime) {
                // 向きを移動方向に揃える
                if (Owner.IsInputMoving) {
                    Owner._targetLookDirection = Owner._moveVelocity;
                }
                else {
                    Owner._targetLookDirection = null;
                }
                
                base.OnUpdate(deltaTime);
            }

            /// <inheritdoc/>
            public override void OnExit() {
                // 重力を戻す
                Owner.GravityScale = 1.0f;
            }

            /// <summary>
            /// セットアップ
            /// </summary>
            public void Setup() {
            }
        }
    }
}