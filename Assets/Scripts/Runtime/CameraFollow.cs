using UnityEngine;
using UnityEngine.InputSystem;

namespace CharacterControlSample {
    /// <summary>
    /// カメラフォロートランスフォーム操作用クラス
    /// </summary>
    public class CameraFollow : MonoBehaviour {
        [SerializeField, Tooltip("プレイヤー入力")]
        private PlayerInput playerInput;

        [SerializeField, Tooltip("追従対象のトランスフォーム")]
        private Transform targetTransform;

        [SerializeField, Tooltip("回転速度")]
        private float angularSpeed = 360.0f;

        private InputAction _lookAction;

        private void Awake() {
            _lookAction = playerInput.actions["Look"];
        }

        private void LateUpdate() {
            var deltaTime = Time.deltaTime;

            // 入力に合わせて回転
            var lookDirection = _lookAction.ReadValue<Vector2>();
            transform.Rotate(0, lookDirection.x * angularSpeed * deltaTime, 0);

            // 座標の追従
            transform.position = targetTransform.position;
        }
    }
}