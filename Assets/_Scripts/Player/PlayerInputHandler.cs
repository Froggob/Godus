using UnityEngine;
using UnityEngine.InputSystem;

namespace Godus.Player
{
    /// <summary>
    /// Bridges Unity Input System to PlayerController.
    /// Attach to the same GameObject as PlayerController.
    /// </summary>
    [RequireComponent(typeof(PlayerController))]
    public class PlayerInputHandler : MonoBehaviour
    {
        [SerializeField] private InputActionAsset inputActions;

        private PlayerController _player;
        private InputAction _moveAction;
        private InputAction _attackAction;
        private InputAction _dashAction;

        private void Awake()
        {
            _player = GetComponent<PlayerController>();

            if (inputActions == null)
            {
                Debug.LogError("[PlayerInputHandler] InputActions asset not assigned!");
                return;
            }

            var playerMap = inputActions.FindActionMap("Player");
            if (playerMap == null)
            {
                Debug.LogError("[PlayerInputHandler] 'Player' action map not found!");
                return;
            }

            _moveAction = playerMap.FindAction("Move");
            _attackAction = playerMap.FindAction("Attack");
            _dashAction = playerMap.FindAction("Dash");

            // Wire attack and dash to PlayerController
            _attackAction.performed += OnAttack;
            _dashAction.performed += OnDash;
        }

        private void OnEnable()
        {
            inputActions?.Enable();
        }

        private void OnDisable()
        {
            inputActions?.Disable();
            if (_attackAction != null) _attackAction.performed -= OnAttack;
            if (_dashAction != null) _dashAction.performed -= OnDash;
        }

        private void Update()
        {
            if (_moveAction != null)
            {
                _player.SetMoveInput(_moveAction.ReadValue<Vector2>());
            }
        }

        private void OnAttack(InputAction.CallbackContext ctx)
        {
            _player.TryAttack();
        }

        private void OnDash(InputAction.CallbackContext ctx)
        {
            _player.TryDash();
        }
    }
}
