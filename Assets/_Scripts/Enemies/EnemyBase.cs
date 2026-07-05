using UnityEngine;
using Godus.Core;
using Godus.Combat;

namespace Godus.Enemies
{
    /// <summary>
    /// Base enemy class. FSM-driven, uses HealthComponent + Hitbox/Hurtbox.
    /// All enemy types inherit from this or use it as a template.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class EnemyBase : MonoBehaviour
    {
        [Header("Stats")]
        [SerializeField] protected int maxHP = 50;
        [SerializeField] protected float moveSpeed = 2f;
        [SerializeField] protected int contactDamage = 10;
        [SerializeField] protected float chaseRange = 6f;
        [SerializeField] protected float attackRange = 1.5f;
        [SerializeField] protected float attackCooldown = 1f;
        [SerializeField] protected int currencyDrop = 5;

        [Header("Combat")]
        [SerializeField] protected HitboxComponent attackHitbox;

        // Internal
        protected Rigidbody2D _rb;
        protected HealthComponent _health;
        protected StateMachine<EnemyState> _fsm;
        protected Transform _player;
        protected float _attackTimer;
        protected Vector2 _moveDir;

        public bool IsDead => _health != null && _health.IsDead;

        protected virtual void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _health = GetComponent<HealthComponent>();
            if (_health == null) _health = gameObject.AddComponent<HealthComponent>();
            _health.MaxHP = maxHP;

            _fsm = new StateMachine<EnemyState>();
            RegisterStates();
        }

        protected virtual void Start()
        {
            _player = GameObject.FindGameObjectWithTag("Player")?.transform;
            _fsm.ChangeState(EnemyState.Idle);
        }

        protected virtual void RegisterStates()
        {
            _fsm.RegisterState(EnemyState.Idle, onUpdate: OnIdle);
            _fsm.RegisterState(EnemyState.Chase, onUpdate: OnChase);
            _fsm.RegisterState(EnemyState.Attack, onEnter: OnEnterAttack);
            _fsm.RegisterState(EnemyState.Hurt, onEnter: OnEnterHurt, onUpdate: OnHurt);
            _fsm.RegisterState(EnemyState.Dead, onEnter: OnDead);
        }

        protected virtual void Update()
        {
            if (_player == null)
                _player = GameObject.FindGameObjectWithTag("Player")?.transform;

            _fsm.Update();
            TickAttackCooldown();
        }

        protected virtual void FixedUpdate()
        {
            var state = _fsm.CurrentState;
            if (state == EnemyState.Idle || state == EnemyState.Chase)
                _rb.linearVelocity = new Vector2(_moveDir.x * moveSpeed, _rb.linearVelocity.y);
            else
                _rb.linearVelocity = new Vector2(0, _rb.linearVelocity.y);
        }

        // --- State: Idle ---
        protected virtual void OnIdle()
        {
            if (_player != null && DistToPlayer() < chaseRange)
                _fsm.ChangeState(EnemyState.Chase);
        }

        // --- State: Chase ---
        protected virtual void OnChase()
        {
            if (_player == null) return;
            _moveDir = (_player.position - transform.position).normalized;

            if (DistToPlayer() < attackRange)
                _fsm.ChangeState(EnemyState.Attack);
            else if (DistToPlayer() > chaseRange * 1.5f)
                _fsm.ChangeState(EnemyState.Idle);
        }

        // --- State: Attack ---
        protected virtual void OnEnterAttack()
        {
            _moveDir = Vector2.zero;
            if (attackHitbox != null)
                attackHitbox.Activate();
        }

        // --- State: Hurt ---
        protected virtual void OnEnterHurt()
        {
            _moveDir = Vector2.zero;
        }

        protected virtual void OnHurt()
        {
            if (!_health.IsInvulnerable)
                _fsm.ChangeState(EnemyState.Chase);
        }

        // --- State: Dead ---
        protected virtual void OnDead()
        {
            _rb.simulated = false;
            CurrencyPickup.Spawn(transform.position, currencyDrop);
        }

        // --- Public ---
        public void TakeDamage(int damage, Vector2 knockbackDir, float knockbackForce)
        {
            _health.TakeDamage(damage, knockbackDir, knockbackForce);

            if (_health.IsDead)
                _fsm.ChangeState(EnemyState.Dead);
            else if (_fsm.CurrentState != EnemyState.Dead)
                _fsm.ChangeState(EnemyState.Hurt);
        }

        // --- Helpers ---
        protected float DistToPlayer()
        {
            if (_player == null) return float.MaxValue;
            return Vector2.Distance(transform.position, _player.position);
        }

        protected void TickAttackCooldown()
        {
            if (_attackTimer > 0f) _attackTimer -= Time.deltaTime;
        }
    }
}
