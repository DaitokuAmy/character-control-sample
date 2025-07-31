using UnityEngine;

namespace CharacterControlSample {
    /// <inheritdoc/>
    partial class Player {
        /// <summary>
        /// AttackActionState
        /// </summary>
        private class AttackActionState : ActionState {
            private int _comboIndex;

            /// <inheritdoc/>
            public override StateType Type => StateType.AttackAction;

            /// <inheritdoc/>
            protected override ActionInfo ActionInfo => Owner.attackActionInfos[_comboIndex];
            /// <inheritdoc/>
            protected override string ActionTagName => "Attack";

            /// <summary>
            /// コンストラクタ
            /// </summary>
            public AttackActionState(Player owner) : base(owner) {
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

            /// <summary>
            /// 攻撃入力
            /// </summary>
            public override void InputAttack() {
                if (_comboIndex >= Owner.attackActionInfos.Length - 1) {
                    return;
                }

                _comboIndex++;
                OnEnter();
            }

            /// <summary>
            /// セットアップ
            /// </summary>
            public void Setup() {
                _comboIndex = 0;
            }
        }
    }
}