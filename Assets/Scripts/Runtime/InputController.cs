using UnityEngine;
using UnityEngine.InputSystem;

namespace CharacterControlSample {
    /// <summary>
    /// 入力操作用クラス
    /// </summary>
    public class InputController : MonoBehaviour {
        [SerializeField, Tooltip("制御対象のプレイヤー")]
        private Player _player;
        
        [SerializeField, Tooltip("プレイヤー入力")]
        private PlayerInput playerInput;
        
        private InputAction _moveAction;
        private InputAction _jumpAction;
        private InputAction _attackAction;

        private void Awake() {
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
            // 移動入力
            var moveDirection = _moveAction.ReadValue<Vector2>();
            _player.InputMoveDirection(moveDirection);
        }

        /// <summary>
        /// JumpAction発生時
        /// </summary>
        private void OnJumpAction(InputAction.CallbackContext context) {
            _player.InputJump();
        }

        /// <summary>
        /// AttackAction発生時
        /// </summary>
        private void OnAttackAction(InputAction.CallbackContext context) {
            _player.InputAttack();
        }
    }
}