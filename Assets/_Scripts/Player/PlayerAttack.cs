using UnityEngine;
using System;

namespace Godus.Player
{
    /// <summary>
    /// Attack component. Knight: heavy melee, wide arc, slow swing.
    /// Uses a hitbox that activates during the attack window.
    /// </summary>
    public class PlayerAttack : MonoBehaviour
    {
        [Header("Hitbox")]
        [SerializeField] private Transform attackHitboxOrigin;
        [SerializeField] private LayerMask hurtboxLayer = 2; // Layer 2 = Hurtboxes

        private PlayerController _player;
        private ClassStats _stats;
        private float _attackTimer;
        private bool _hitDealt;
        private bool _isAttacking;
        private Action _onComplete;

        public bool IsAttacking => _isAttacking;

        public void Initialize(PlayerController player, ClassStats stats)
        {
            _player = player;
            _stats = stats;
        }

        /// <summary>
        /// Execute the attack. Called by PlayerController's FSM.
        /// </summary>
        public void ExecuteAttack(Action onComplete)
        {
            _isAttacking = true;
            _attackTimer = 0f;
            _hitDealt = false;
            _onComplete = onComplete;
        }

        private void Update()
        {
            if (!_isAttacking) return;

            _attackTimer += Time.deltaTime;

            // Activate hitbox at the hit window
            if (!_hitDealt && _attackTimer >= _stats.attackHitWindow)
            {
                DealDamage();
                _hitDealt = true;
            }

            // Attack complete
            if (_attackTimer >= _stats.attackDuration + _stats.attackCooldown)
            {
                _isAttacking = false;
                _onComplete?.Invoke();
            }
        }

        private void DealDamage()
        {
            Vector3 origin = attackHitboxOrigin != null
                ? attackHitboxOrigin.position
                : transform.position + (Vector3)(_player.IsFacingRight ? Vector2.right : Vector2.left) * 1f;

            // Overlap circle to find hurtboxes
            var hits = Physics2D.OverlapCircleAll(origin, _stats.attackRange, hurtboxLayer);

            foreach (var hit in hits)
            {
                // Check if it's an enemy hurtbox
                var hurtbox = hit.GetComponent<IHurtbox>();
                if (hurtbox != null)
                {
                    Vector2 knockbackDir = (hit.transform.position - transform.position).normalized;
                    hurtbox.TakeDamage(_stats.baseAttackDamage, knockbackDir, 5f);
                }
            }

            // Visual feedback placeholder
            Debug.Log($"[Attack] Dealt {_stats.baseAttackDamage} damage. Hits: {hits.Length}");
        }

        private void OnDrawGizmosSelected()
        {
            if (_stats == null) return;
            Gizmos.color = Color.red;
            Vector3 origin = attackHitboxOrigin != null
                ? attackHitboxOrigin.position
                : transform.position + (_player != null && _player.IsFacingRight ? Vector3.right : Vector3.left) * 1f;
            Gizmos.DrawWireSphere(origin, _stats.attackRange);
        }
    }

    /// <summary>
    /// Interface for anything that can take damage (enemies, breakables, etc).
    /// </summary>
    public interface IHurtbox
    {
        void TakeDamage(int damage, Vector2 knockbackDirection, float knockbackForce);
    }
}
