using System.Collections.Generic;
using UnityEngine;

namespace CharacterControlSample {
    /// <inheritdoc/>
    partial class Player {
        /// <summary>
        /// 状態タイプ
        /// </summary>
        private enum StateType {
            Locomotion,
            JumpAction,
            AttackAction,
            LandingAction,
        }

        /// <summary>
        /// 状態の基底クラス
        /// </summary>
        private abstract class StateBase {
            /// <summary>自身のStateType</summary>
            public abstract StateType Type { get; }
            /// <summary>オーナーのPlayer</summary>
            protected Player Owner { get; }

            /// <summary>
            /// コンストラクタ
            /// </summary>
            protected StateBase(Player owner) {
                Owner = owner;
            }

            /// <summary>
            /// 開始処理
            /// </summary>
            public virtual void OnEnter() {
            }

            /// <summary>
            /// 更新処理
            /// </summary>
            public virtual void OnUpdate(float deltaTime) {
            }

            /// <summary>
            /// 終了処理
            /// </summary>
            public virtual void OnExit() {
            }

            /// <summary>
            /// 移動入力
            /// </summary>
            public virtual void InputMoveDirection(Vector2 moveDirection) {
            }

            /// <summary>
            /// ジャンプ入力
            /// </summary>
            public virtual void InputJump() {
            }

            /// <summary>
            /// 攻撃入力
            /// </summary>
            public virtual void InputAttack() {
            }

            /// <summary>
            /// Stateの変更
            /// </summary>
            protected void ChangeState(StateType stateType) {
                Owner.ChangeState(stateType);
            }

            /// <summary>
            /// Animatorの現在のStateについているTagをチェック
            /// </summary>
            protected bool CheckAnimatorStateTag(string tag) {
                return Owner._animator.GetNextAnimatorStateInfo(0).IsTag(tag) ||
                       Owner._animator.GetCurrentAnimatorStateInfo(0).IsTag(tag);
            }
        }

        private readonly Dictionary<StateType, StateBase> _states = new();

        private StateBase _currentState;
        private LocomotionState _locomotionState;
        private JumpActionState _jumpActionState;
        private AttackActionState _attackActionState;
        private LandingActionState _landingActionState;

        /// <summary>
        /// ステートのセットアップ
        /// </summary>
        private void SetupStates(StateType initStateType) {
            // ステートインスタンスの生成
            _states[StateType.Locomotion] = _locomotionState = new LocomotionState(this);
            _states[StateType.JumpAction] = _jumpActionState = new JumpActionState(this);
            _states[StateType.AttackAction] = _attackActionState = new AttackActionState(this);
            _states[StateType.LandingAction] = _landingActionState = new LandingActionState(this);

            // ステート初期化
            ChangeState(initStateType);
        }

        /// <summary>
        /// ステートのクリーンアップ
        /// </summary>
        private void CleanupStates() {
            if (_currentState != null) {
                _currentState.OnExit();
                _currentState = null;
            }

            _states.Clear();
        }

        /// <summary>
        /// ステートの更新
        /// </summary>
        private void UpdateState(float deltaTime) {
            if (_currentState != null) {
                _currentState.OnUpdate(deltaTime);
            }
        }

        /// <summary>
        /// ステートの変更
        /// </summary>
        private void ChangeState(StateType stateType) {
            if (_currentState != null && _currentState.Type == stateType) {
                return;
            }

            _currentState?.OnExit();
            _currentState = null;

            if (!_states.TryGetValue(stateType, out var state)) {
                return;
            }

            _currentState = state;
            state.OnEnter();
        }
    }
}