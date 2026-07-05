using UnityEngine;
using Godus.Core;

namespace Godus.Player
{
    /// <summary>
    /// FSM-driven player controller. Milestone 1 — Knight gray-box.
    /// All state logic delegates to components (Dash, Attack).
    /// Uses explicit states — no boolean flags per the design doc.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(CapsuleCollider2D))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private ClassStats stats;

        [Header("Components")]
        [SerializeField] private PlayerDash dash;
        [SerializeField] private PlayerAttack attack;

        // Internal
        private Rigidbody2D _rb;
        private StateMachine<PlayerState> _fsm;
        private Vector2 _moveInput;
        private float _hurtTimer;
        private bool _isFacingRight = true;

        // Public state
        public PlayerState CurrentState => _fsm.CurrentState;
        public Vector2 Velocity => _rb != null ? _rb.linearVelocity : Vector2.zero;
        public bool IsFacingRight => _isFacingRight;

        // --- Unity Lifecycle ---

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.gravityScale = 3f;

            // Find components if not assigned
            if (dash == null) dash = GetComponent<PlayerDash>();
            if (attack == null) attack = GetComponent<PlayerAttack>();

            // Init FSM
            _fsm = new StateMachine<PlayerState>();
            RegisterStates();
        }

        private void Start()
        {
            dash.Initialize(this, stats);
            attack.Initialize(this, stats);

            _fsm.ChangeState(PlayerState.Idle);
        }

        private void Update()
        {
            ReadInput();
            _fsm.Update();
        }

        private void FixedUpdate()
        {
            // Physics-based movement runs in FixedUpdate, driven by state
            ApplyMovement();
        }

        // --- FSM Registration ---

        private void RegisterStates()
        {
            _fsm.RegisterState(PlayerState.Idle,
                onEnter: OnEnterIdle,
                onUpdate: OnUpdateIdle);

            _fsm.RegisterState(PlayerState.Run,
                onEnter: OnEnterRun,
                onUpdate: OnUpdateRun);

            _fsm.RegisterState(PlayerState.Attack,
                onEnter: OnEnterAttack);

            _fsm.RegisterState(PlayerState.Dash,
                onEnter: OnEnterDash);

            _fsm.RegisterState(PlayerState.Hurt,
                onEnter: OnEnterHurt,
                onUpdate: OnUpdateHurt);

            _fsm.RegisterState(PlayerState.Dead,
                onEnter: OnEnterDead);
        }

        // --- State: Idle ---

        private void OnEnterIdle() { }

        private void OnUpdateIdle()
        {
            if (_moveInput.sqrMagnitude > 0.01f)
            {
                _fsm.ChangeState(PlayerState.Run);
            }
        }

        // --- State: Run ---

        private void OnEnterRun() { }

        private void OnUpdateRun()
        {
            if (_moveInput.sqrMagnitude < 0.01f)
            {
                _fsm.ChangeState(PlayerState.Idle);
            }
        }

        // --- State: Attack ---

        private void OnEnterAttack()
        {
            attack.ExecuteAttack(() =>
            {
                // Return to idle/run after attack completes
                _fsm.ChangeState(_moveInput.sqrMagnitude > 0.01f
                    ? PlayerState.Run
                    : PlayerState.Idle);
            });
        }

        // --- State: Dash ---

        private void OnEnterDash()
        {
            dash.ExecuteDash(_moveInput, () =>
            {
                // Return to idle/run after dash completes
                _fsm.ChangeState(_moveInput.sqrMagnitude > 0.01f
                    ? PlayerState.Run
                    : PlayerState.Idle);
            });
        }

        // --- State: Hurt ---

        private void OnEnterHurt()
        {
            _hurtTimer = stats.hurtIFrames;
            EventBus.EmitPlayerHealthChanged(0, stats.maxHP); // placeholder
        }

        private void OnUpdateHurt()
        {
            _hurtTimer -= Time.deltaTime;
            if (_hurtTimer <= 0f)
            {
                _fsm.ChangeState(PlayerState.Idle);
            }
        }

        // --- State: Dead ---

        private void OnEnterDead()
        {
            _rb.linearVelocity = Vector2.zero;
            _rb.simulated = false;
            EventBus.EmitPlayerDied();
        }

        // --- Public Methods ---

        /// <summary>
        /// Take damage from an external source. Called by hurtbox or combat system.
        /// </summary>
        public void TakeDamage(int damage, Vector2 knockbackDirection, float knockbackForce)
        {
            if (_fsm.CurrentState == PlayerState.Dead) return;

            // Dash poise reduces damage (Knight feature)
            if (_fsm.CurrentState == PlayerState.Dash && stats.dashGrantsPoise)
            {
                damage = Mathf.RoundToInt(damage * (1f - stats.dashPoiseReduction));
                knockbackForce *= (1f - stats.dashPoiseKnockbackResist);
            }

            // Apply knockback
            _rb.linearVelocity = knockbackDirection.normalized * knockbackForce;

            // TODO: subtract from actual HP (health component in Milestone 2)
            EventBus.EmitPlayerDamaged(gameObject, damage);

            _fsm.ChangeState(PlayerState.Hurt);
        }

        /// <summary>
        /// Die immediately. Called when HP reaches 0.
        /// </summary>
        public void Die()
        {
            _fsm.ChangeState(PlayerState.Dead);
        }

        // --- Input ---

        /// <summary>
        /// Reads Unity Input System values. Called every frame in Update().
        /// </summary>
        public void SetMoveInput(Vector2 input)
        {
            _moveInput = input;

            // Flip sprite based on movement direction
            if (input.x > 0.1f && !_isFacingRight) Flip();
            else if (input.x < -0.1f && _isFacingRight) Flip();
        }

        /// <summary>
        /// Trigger an attack. Only works in Idle or Run.
        /// </summary>
        public void TryAttack()
        {
            var state = _fsm.CurrentState;
            if (state == PlayerState.Idle || state == PlayerState.Run)
            {
                _fsm.ChangeState(PlayerState.Attack);
            }
        }

        /// <summary>
        /// Trigger a dash. Only works in Idle or Run. Respects cooldown.
        /// </summary>
        public void TryDash()
        {
            var state = _fsm.CurrentState;
            if ((state == PlayerState.Idle || state == PlayerState.Run) && dash.CanDash())
            {
                _fsm.ChangeState(PlayerState.Dash);
            }
        }

        // --- Movement ---

        private void ApplyMovement()
        {
            var state = _fsm.CurrentState;

            // Only apply ground movement in Idle and Run
            if (state != PlayerState.Idle && state != PlayerState.Run)
                return;

            float targetSpeed = _moveInput.x * stats.moveSpeed;
            float currentSpeed = _rb.linearVelocity.x;

            if (Mathf.Abs(_moveInput.x) > 0.01f)
            {
                // Accelerate toward target
                float newSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed,
                    stats.acceleration * Time.fixedDeltaTime);
                _rb.linearVelocity = new Vector2(newSpeed, _rb.linearVelocity.y);
            }
            else
            {
                // Decelerate to stop
                float newSpeed = Mathf.MoveTowards(currentSpeed, 0f,
                    stats.deceleration * Time.fixedDeltaTime);
                _rb.linearVelocity = new Vector2(newSpeed, _rb.linearVelocity.y);
            }
        }

        private void Flip()
        {
            _isFacingRight = !_isFacingRight;
            var scale = transform.localScale;
            scale.x *= -1f;
            transform.localScale = scale;
        }

        // --- Input Handling (called by PlayerInput or directly) ---

        private void ReadInput()
        {
            // Input is set externally via SetMoveInput/TryAttack/TryDash
            // This is a hook for future input-driven state transitions
        }
    }
}
