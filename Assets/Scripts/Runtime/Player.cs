using System;
using UnityEngine;

namespace CharacterControlSample {
    /// <summary>
    /// プレイヤー操作用クラス
    /// </summary>
    public partial class Player : MonoBehaviour {
        // Animatorパラメータ用のId
        private static readonly int SpeedPropId = Animator.StringToHash("speed");
        private static readonly int SpeedXPropId = Animator.StringToHash("speed.x");
        private static readonly int SpeedZPropId = Animator.StringToHash("speed.z");
        private static readonly int SpeedScalePropId = Animator.StringToHash("speed_scale");
        private static readonly int IsAirPropId = Animator.StringToHash("is_air");
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

        [SerializeField, Tooltip("最大速度")]
        private float maxSpeed = 4.0f;

        [SerializeField, Tooltip("Root移動による最大速度")]
        private float maxRootSpeed = 4.0f;

        [SerializeField, Tooltip("加速度")]
        private float acceleration = 1.0f;

        [SerializeField, Tooltip("角加速度")]
        private float angularAcceleration = 1.0f;

        [SerializeField, Tooltip("見ている方向に使うTransform")]
        private Transform lookTransform;

        [SerializeField, Tooltip("ジャンプアクション情報")]
        private ActionInfo jumpActionInfo;

        [SerializeField, Tooltip("着地アクション情報")]
        private ActionInfo[] landingActionInfos;

        [SerializeField, Tooltip("攻撃アクション情報リスト")]
        private ActionInfo[] attackActionInfos;

        [SerializeField, Tooltip("基本移動モーションでRoot更新を行うか")]
        private bool _useRootLocomotion = true;

        [SerializeField, Tooltip("速度スケールを用いて移動速度をルート移動モーションより増加させるか")]
        private bool _useSpeedScale = true;

        [SerializeField, Tooltip("空中状態での移動速度にかけるスケール")]
        private float _airMoveSpeedMultiplier = 0.5f;

        [SerializeField, Tooltip("地面判定用のSphereCast半径")]
        private float _groundCastRadius = 0.2f;

        [SerializeField, Tooltip("地面判定用のSphereCast距離")]
        private float _groundCastDistance = 0.5f;

        [SerializeField, Tooltip("地面チェック用のレイヤーマスク")]
        private LayerMask _groundCastLayerMask = -1;

        [SerializeField, Tooltip("着地アクションを流す速度閾値")]
        private float _landingSpeedThreshold = 2.0f;

        [Header("可視化用")]
        [SerializeField, Tooltip("ルート移動量スケール")]
        private Vector3 _rootPositionScale = Vector3.one;

        [SerializeField, Tooltip("ルート回転量(Y)スケール")]
        private float rootRotationYYScale = 1.0f;

        [SerializeField, Tooltip("重力スケール")]
        private float _gravityScale = 1.0f;

        [SerializeField, Tooltip("アクション実行不可能フラグ")]
        private bool _disableAction;

        [SerializeField, Tooltip("移動入力速度")]
        private Vector3 _moveVelocity;

        private readonly RaycastHit[] _raycastHits = new RaycastHit[8];

        private Animator _animator;
        private CharacterController _characterController;
        private Vector2 _inputDirection;
        private Vector3? _targetLookDirection;
        private float _gravitySpeed;
        private bool _air;

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

        /// <summary>移動入力中か</summary>
        public bool IsInputMoving => _moveVelocity.sqrMagnitude > 0.01f;

        /// <summary>
        /// 移動値入力
        /// </summary>
        public void InputMoveDirection(Vector2 inputDirection) {
            _inputDirection = inputDirection;

            // 現在のステートに通知
            _currentState?.InputMoveDirection(inputDirection);
        }

        /// <summary>
        /// ジャンプ入力
        /// </summary>
        public void InputJump() {
            if (_disableAction) {
                return;
            }

            // 現在のステートに通知
            _currentState?.InputJump();
        }

        /// <summary>
        /// 攻撃入力
        /// </summary>
        public void InputAttack() {
            if (_disableAction) {
                return;
            }

            // 現在のステートに通知
            _currentState?.InputAttack();
        }

        private void Awake() {
            _animator = GetComponent<Animator>();
            _characterController = GetComponent<CharacterController>();
        }

        private void OnEnable() {
            _inputDirection = Vector2.zero;
            SetupStates(StateType.Locomotion);
        }

        private void OnDisable() {
            CleanupStates();
        }

        private void Update() {
            var deltaTime = Time.deltaTime;

            // ステートの更新
            UpdateState(deltaTime);

            // 速度の更新
            UpdateVelocity(deltaTime);

            // 向きの更新
            UpdateRotation(deltaTime);

            // 重力の更新
            UpdateGravity(deltaTime);

            // 空中状態の更新
            UpdateAirStatus(deltaTime);

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
        /// LandingAction発生時
        /// </summary>
        private void OnLandingAction() {
            if (_disableAction) {
                return;
            }

            _landingActionState.Setup(0);
            ChangeState(StateType.LandingAction);
        }

        /// <summary>
        /// 速度の更新
        /// </summary>
        private void UpdateVelocity(float deltaTime) {
            // 移動入力に合わせて速度を更新
            var targetVelocity = Quaternion.Euler(0, lookTransform.eulerAngles.y, 0) * new Vector3(_inputDirection.x, 0, _inputDirection.y) * maxSpeed;

            // 実際の速度は補間して更新
            var delta = targetVelocity - _moveVelocity;
            var deltaMagnitude = delta.magnitude;
            if (deltaMagnitude <= acceleration * deltaTime) {
                _moveVelocity = targetVelocity;
            }
            else {
                _moveVelocity += delta * (acceleration * deltaTime / deltaMagnitude);
            }
        }

        /// <summary>
        /// 向きの更新
        /// </summary>
        private void UpdateRotation(float deltaTime) {
            // ターゲット向きがあればそこに向かって向きを合わせる
            if (_targetLookDirection.HasValue) {
                var targetDir = _targetLookDirection.Value;
                targetDir.y = 0;
                targetDir.Normalize();
                var targetAngleY = Mathf.Atan2(targetDir.x, targetDir.z) * Mathf.Rad2Deg;
                var currentAngles = transform.eulerAngles;
                var deltaAngle = Mathf.DeltaAngle(currentAngles.y, targetAngleY);
                if (Mathf.Abs(deltaAngle) <= angularAcceleration * deltaTime) {
                    currentAngles.y = targetAngleY;
                    transform.eulerAngles = currentAngles;
                }
                else {
                    if (deltaAngle >= 0.0f) {
                        currentAngles.y += angularAcceleration * deltaTime;
                    }
                    else {
                        currentAngles.y -= angularAcceleration * deltaTime;
                    }

                    transform.eulerAngles = currentAngles;
                }
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
            Move(Vector3.up * (_gravitySpeed * deltaTime));
        }

        /// <summary>
        /// 空中状態の更新
        /// </summary>
        private void UpdateAirStatus(float deltaTime) {
            var checkRay = new Ray(transform.position, Vector3.down);
            var hitCount = Physics.SphereCastNonAlloc(checkRay, _groundCastRadius, _raycastHits, _groundCastDistance, _groundCastLayerMask);

            // 上方向を向いている板に衝突していたら空中ではないとみなす
            var prevAir = _air;
            _air = true;
            for (var i = 0; i < hitCount; i++) {
                var hit = _raycastHits[i];
                if (Vector3.Dot(hit.normal, Vector3.up) <= Mathf.Cos(Mathf.Deg2Rad * 20.0f)) {
                    continue;
                }

                _air = false;
            }

            // 着地判定
            if (prevAir && !_air) {
                var velocityY = _characterController.velocity.y;
                if (velocityY <= -_landingSpeedThreshold) {
                    OnLandingAction();
                }
            }
        }

        /// <summary>
        /// Locomotion制御用のパラメータ更新
        /// </summary>
        private void UpdateAnimatorParameters(float deltaTime) {
            // 移動速度
            var speed = _moveVelocity.magnitude;
            _animator.SetFloat(SpeedPropId, speed);

            // 速度スケール
            var speedScale = _useSpeedScale ? Mathf.Max(1.0f, speed / maxRootSpeed) : 1.0f;
            _animator.SetFloat(SpeedScalePropId, speedScale);

            // 正面から見た相対速度
            var relativeVelocity = Quaternion.Euler(0, -transform.eulerAngles.y, 0) * _moveVelocity;
            _animator.SetFloat(SpeedXPropId, relativeVelocity.x);
            _animator.SetFloat(SpeedZPropId, relativeVelocity.z);

            // 空中状態の反映
            _animator.SetBool(IsAirPropId, _air);
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