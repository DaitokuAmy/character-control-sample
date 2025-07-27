using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace CharacterControlSample {
    /// <summary>
    /// プレイヤー操作用クラス
    /// </summary>
    public class Player : MonoBehaviour {
        // Animatorパラメータ用のId
        private static readonly int SpeedPropId = Animator.StringToHash("speed");
        private static readonly int SpeedXPropId = Animator.StringToHash("speed.x");
        private static readonly int SpeedZPropId = Animator.StringToHash("speed.z");
        private static readonly int InverseGravityScalePropId = Animator.StringToHash("inverse_gravity_scale");
        private static readonly int DisableActionPropId = Animator.StringToHash("disable_action");

        /// <summary>
        /// アクション情報
        /// </summary>
        [Serializable]
        private class ActionInfo {
            [Tooltip("ステート名")]
            public string stateName;

            [Tooltip("ブレンド時間")]
            public float normalizedBlendTime;
        }

        [SerializeField, Tooltip("プレイヤー入力")]
        private PlayerInput playerInput;

        [SerializeField, Tooltip("最大速度")]
        private float maxSpeed = 4.0f;

        [SerializeField, Tooltip("移動速度補間割合")]
        private float speedInterpRate = 1.0f;

        [SerializeField, Tooltip("回転速度補間割合")]
        private float rotationInterpRate = 1.0f;

        [SerializeField, Tooltip("見ている方向に使うTransform")]
        private Transform lookTransform;

        [SerializeField, Tooltip("ジャンプアクション情報")]
        private ActionInfo jumpActionInfo;

        [SerializeField, Tooltip("攻撃アクション情報リスト")]
        private ActionInfo[] attackActionInfos;

        [Header("可視化用")]
        [SerializeField, Tooltip("ルート移動量スケール")]
        private Vector3 _rootPositionScale = Vector3.one;

        [FormerlySerializedAs("_rootRotationYScale")]
        [SerializeField, Tooltip("ルート回転量(Y)スケール")]
        private float rootRotationYYScale = 1.0f;

        [SerializeField, Tooltip("重力スケール")]
        private float _gravityScale = 1.0f;

        [SerializeField, Tooltip("アクション実行不可能フラグ")]
        private bool _disableAction;

        private Animator _animator;
        private CharacterController _characterController;
        private Vector3 _velocity;
        private float _gravitySpeed;
        private InputAction _moveAction;
        private InputAction _jumpAction;
        private InputAction _attackAction;

        /// <summary>重力スケール</summary>
        public float GravityScale {
            get => _gravityScale;
            set => _gravityScale = value;
        }

        /// <summary>ルート移動スケール</summary>
        public Vector3 RootPositionScale {
            get => _rootPositionScale;
            set => _rootPositionScale = value;
        }

        /// <summary>ルート回転スケール(Y軸)</summary>
        public float RootRotationYScale {
            get => rootRotationYYScale;
            set => rootRotationYYScale = value;
        }

        private void Awake() {
            _animator = GetComponent<Animator>();
            _characterController = GetComponent<CharacterController>();
            _moveAction = playerInput.actions["Move"];
            _jumpAction = playerInput.actions["Jump"];
            _attackAction = playerInput.actions["Attack"];
        }

        private void OnEnable() {
            _jumpAction.performed += OnJumpAction;
            _attackAction.performed += OnAttackAction;
        }

        private void OnDisable() {
            _jumpAction.performed -= OnJumpAction;
            _attackAction.performed -= OnAttackAction;
        }

        private void Update() {
            var deltaTime = Time.deltaTime;

            // 速度の更新
            UpdateVelocity(deltaTime);

            // 向きの更新
            UpdateRotation(deltaTime);

            // 重力の更新
            UpdateGravity(deltaTime);

            // アニメーターのパラメータに反映
            UpdateAnimatorParameters(deltaTime);

            // アニメーターのパラメータから情報を反映
            UpdateFromAnimatorParameters(deltaTime);
        }

        private void OnAnimatorMove() {
            // ローカル移動量に変換
            var deltaPosition = _animator.deltaPosition;
            var localDeltaPosition = transform.InverseTransformDirection(deltaPosition);

            // 軸スケールを考慮
            localDeltaPosition = Vector3.Scale(localDeltaPosition, RootPositionScale);
            deltaPosition = transform.TransformDirection(localDeltaPosition);

            // 回転はワールドで考慮
            var deltaEulerAngles = _animator.deltaRotation.eulerAngles;
            deltaEulerAngles.y *= RootRotationYScale;

            // 値の反映
            Move(deltaPosition);
            transform.eulerAngles += deltaEulerAngles;
        }

        /// <summary>
        /// JumpAction発生時
        /// </summary>
        private void OnJumpAction(InputAction.CallbackContext context) {
            if (_disableAction) {
                return;
            }
            
            var actionInfo = jumpActionInfo;

            // ステートに遷移
            _animator.CrossFade(actionInfo.stateName, actionInfo.normalizedBlendTime);
        }

        /// <summary>
        /// AttackAction発生時
        /// </summary>
        private void OnAttackAction(InputAction.CallbackContext context) {
            if (_disableAction) {
                return;
            }
            
            // 攻撃の抽選
            var index = Random.Range(0, attackActionInfos.Length);
            var actionInfo = attackActionInfos[index];

            // ステートに遷移
            _animator.CrossFade(actionInfo.stateName, actionInfo.normalizedBlendTime);
        }

        /// <summary>
        /// 速度の更新
        /// </summary>
        private void UpdateVelocity(float deltaTime) {
            // 移動入力に合わせて速度を更新
            var inputDirection = _moveAction.ReadValue<Vector2>();
            var targetVelocity = Quaternion.Euler(0, lookTransform.eulerAngles.y, 0) * new Vector3(inputDirection.x, 0, inputDirection.y) * maxSpeed;

            // 実際の速度は補間して更新
            var t = 1.0f - Mathf.Exp(-speedInterpRate * deltaTime);
            _velocity = Vector3.Lerp(_velocity, targetVelocity, t);
        }

        /// <summary>
        /// 向きの更新
        /// </summary>
        private void UpdateRotation(float deltaTime) {
            // 移動速度方向に徐々に回転させる
            if (_velocity.sqrMagnitude > 0.01f) {
                var targetDir = lookTransform.forward;
                targetDir.y = 0;
                targetDir.Normalize();
                var currentDir = transform.forward;
                var t = 1.0f - Mathf.Exp(-rotationInterpRate * deltaTime);
                currentDir = Vector3.Lerp(currentDir, targetDir, t);
                transform.rotation = Quaternion.LookRotation(currentDir);
            }
        }

        /// <summary>
        /// 重力制御
        /// </summary>
        private void UpdateGravity(float deltaTime) {
            var grounded = _characterController.isGrounded;
            if (grounded) {
                _gravitySpeed = 0.0f;
                return;
            }

            var gravity = Physics.gravity * GravityScale;
            _gravitySpeed += gravity.y * deltaTime;
            Move(Vector3.up * _gravitySpeed * deltaTime);
        }

        /// <summary>
        /// Locomotion制御用のパラメータ更新
        /// </summary>
        private void UpdateAnimatorParameters(float deltaTime) {
            // 移動速度
            var speed = _velocity.magnitude;
            _animator.SetFloat(SpeedPropId, speed);

            // 正面から見た相対速度
            var relativeVelocity = Quaternion.Euler(0, -transform.eulerAngles.y, 0) * _velocity;
            _animator.SetFloat(SpeedXPropId, relativeVelocity.x);
            _animator.SetFloat(SpeedZPropId, relativeVelocity.z);
        }

        /// <summary>
        /// Animatorのパラメータから情報を更新
        /// </summary>
        private void UpdateFromAnimatorParameters(float deltaTime) {
            GravityScale = Mathf.Clamp01(1.0f - _animator.GetFloat(InverseGravityScalePropId));
            _disableAction = _animator.GetFloat(DisableActionPropId) >= 0.1f;
        }

        /// <summary>
        /// 移動
        /// </summary>
        private void Move(Vector3 deltaPosition) {
            _characterController.Move(deltaPosition);
        }

        /// <summary>
        /// 座標設定
        /// </summary>
        private void SetPosition(Vector3 newPosition) {
            var deltaPosition = newPosition - transform.position;
            Move(deltaPosition);
        }
    }
}