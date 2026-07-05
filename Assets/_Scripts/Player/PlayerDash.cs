using UnityEngine;
using System;

namespace Godus.Player
{
    /// <summary>
    /// Dash component. Knight variant: short heavy tackle with poise/armor
    /// instead of full i-frames. Cooldown-based per the design doc.
    /// </summary>
    public class PlayerDash : MonoBehaviour
    {
        private PlayerController _player;
        private ClassStats _stats;
        private Rigidbody2D _rb;

        private float _dashTimer;
        private float _cooldownTimer;
        private Vector2 _dashDirection;
        private Action _onComplete;
        private bool _isDashing;

        public bool IsDashing => _isDashing;
        public float CooldownRemaining => _cooldownTimer;

        public void Initialize(PlayerController player, ClassStats stats)
        {
            _player = player;
            _stats = stats;
            _rb = GetComponent<Rigidbody2D>();
        }

        public bool CanDash()
        {
            return _cooldownTimer <= 0f && !_isDashing;
        }

        /// <summary>
        /// Execute the dash. Called by PlayerController's FSM.
        /// </summary>
        public void ExecuteDash(Vector2 inputDirection, Action onComplete)
        {
            _isDashing = true;
            _dashTimer = _stats.dashDuration;
            _onComplete = onComplete;

            // Dash direction: input direction, or face direction if no input
            if (inputDirection.sqrMagnitude > 0.01f)
            {
                _dashDirection = inputDirection.normalized;
            }
            else
            {
                _dashDirection = _player.IsFacingRight ? Vector2.right : Vector2.left;
            }

            // Set dash velocity (overrides normal movement)
            float dashSpeed = _stats.dashDistance / _stats.dashDuration;
            _rb.linearVelocity = _dashDirection * dashSpeed;

            // Knight dash-tackle has brief armor/poise — managed in PlayerController.TakeDamage()
        }

        private void Update()
        {
            // Dash countdown
            if (_isDashing)
            {
                _dashTimer -= Time.deltaTime;

                // Maintain dash velocity
                float dashSpeed = _stats.dashDistance / _stats.dashDuration;
                _rb.linearVelocity = _dashDirection * dashSpeed;

                if (_dashTimer <= 0f)
                {
                    _isDashing = false;
                    _cooldownTimer = _stats.dashCooldown;
                    _rb.linearVelocity = new Vector2(0f, _rb.linearVelocity.y); // stop horizontal
                    _onComplete?.Invoke();
                }
            }

            // Cooldown countdown
            if (_cooldownTimer > 0f)
            {
                _cooldownTimer -= Time.deltaTime;
            }
        }
    }
}
