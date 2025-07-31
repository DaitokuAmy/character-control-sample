namespace CharacterControlSample {
    /// <inheritdoc/>
    partial class Player {
        /// <summary>
        /// ActionState基底
        /// </summary>
        private abstract class ActionState : StateBase {
            private bool _inState;

            /// <summary>再生するActionInfo</summary>
            protected abstract ActionInfo ActionInfo { get; }
            /// <summary>再生中のアクションタグ</summary>
            protected abstract string ActionTagName { get; }

            /// <summary>
            /// コンストラクタ
            /// </summary>
            protected ActionState(Player owner) : base(owner) {
            }

            /// <inheritdoc/>
            public override void OnEnter() {
                _inState = false;

                if (ActionInfo != null) {
                    // ステートに遷移
                    Owner._animator.CrossFade(ActionInfo.stateName, ActionInfo.normalizedBlendTime);
                }
            }

            /// <inheritdoc/>
            public override void OnUpdate(float deltaTime) {
                // 終了判定
                if (ActionInfo == null ||
                    string.IsNullOrEmpty(ActionTagName) ||
                    (_inState && !CheckAnimatorStateTag(ActionTagName))) {
                    ChangeState(StateType.Locomotion);
                }

                // ステートに入ったかチェック
                if (!_inState) {
                    _inState = CheckAnimatorStateTag(ActionTagName);
                }
            }
        }
    }
}