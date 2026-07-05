using UnityEngine;
using Godus.Core;

namespace Godus.Combat
{
    /// <summary>
    /// Health, damage, and death for any entity (player, enemies, breakables).
    /// Emits events via EventBus. Used by both player and all enemy types.
    /// </summary>
    public class HealthComponent : MonoBehaviour
    {
        [SerializeField] private int _maxHP = 100;
        [SerializeField] private float invulnDuration = 0.3f;
        [SerializeField] private bool isPlayer;

        private int _currentHP;
        private float _invulnTimer;
        private bool _isDead;

        public int CurrentHP => _currentHP;
        public int MaxHP { get => _maxHP; set => _maxHP = value; }
        public bool IsDead => _isDead;
        public bool IsInvulnerable => _invulnTimer > 0f || _isDead;

        private void Awake()
        {
            _currentHP = _maxHP;
        }

        public void TakeDamage(int damage, Vector2 knockbackDir, float knockbackForce)
        {
            if (_isDead || _invulnTimer > 0f) return;

            _currentHP -= damage;
            _invulnTimer = invulnDuration;

            // Apply knockback via Rigidbody2D
            if (TryGetComponent<Rigidbody2D>(out var rb))
                rb.linearVelocity = knockbackDir * knockbackForce;

            if (isPlayer)
                EventBus.EmitPlayerDamaged(gameObject, damage);
            else
                EventBus.EmitEnemyDamaged(gameObject, damage);

            if (_currentHP <= 0)
                Die();
        }

        public void Die()
        {
            if (_isDead) return;
            _isDead = true;

            if (isPlayer)
                EventBus.EmitPlayerDied();
            else
                EventBus.EmitEnemyDied(gameObject, transform.position);
        }

        public void Heal(int amount)
        {
            _currentHP = Mathf.Min(_currentHP + amount, _maxHP);
        }

        public void SetMaxHP(int newMax)
        {
            _maxHP = newMax;
            _currentHP = Mathf.Min(_currentHP, _maxHP);
        }

        private void Update()
        {
            if (_invulnTimer > 0f)
                _invulnTimer -= Time.deltaTime;
        }
    }
}
