using UnityEngine;

namespace CharacterControlSample {
    /// <inheritdoc/>
    partial class Player {
        /// <summary>
        /// LocomotionState
        /// </summary>
        private class LocomotionState : StateBase {
            /// <inheritdoc/>
            public override StateType Type => StateType.Locomotion;

            /// <summary>
            /// コンストラクタ
            /// </summary>
            public LocomotionState(Player owner) : base(owner) {
            }

            /// <inheritdoc/>
            public override void OnEnter() {
                // RootMotionを使用せずに移動する場合、Rootの移動Scaleを0にする
                if (!Owner._useRootLocomotion) {
                    Owner.RootPositionScale = Vector3.zero;
                }
            }

            /// <inheritdoc/>
            public override void OnUpdate(float deltaTime) {
                // 空中状態は移動をプログラムで反映させる
                if (Owner._air) {
                    Owner.Move(Owner._moveVelocity * (Owner._airMoveSpeedMultiplier * deltaTime));
                }
                // プログラム制御による移動
                else if (!Owner._useRootLocomotion) {
                    Owner.Move(Owner._moveVelocity * deltaTime);
                }
                
                // 移動速度があれば、カメラ方向に徐々に向きを直していく
                if (Owner.IsInputMoving) {
                    Owner._targetLookDirection = Owner.lookTransform.forward;
                }
                else {
                    Owner._targetLookDirection = null;
                }
            }

            /// <inheritdoc/>
            public override void OnExit() {
                if (!Owner._useRootLocomotion) {
                    Owner.RootPositionScale = Vector3.one;
                }
            }

            /// <inheritdoc/>
            public override void InputJump() {
                // ジャンプに遷移
                Owner._jumpActionState.Setup();
                ChangeState(StateType.JumpAction);
            }

            /// <inheritdoc/>
            public override void InputAttack() {
                // アクションに遷移
                Owner._attackActionState.Setup();
                ChangeState(StateType.AttackAction);
            }
        }
    }
}