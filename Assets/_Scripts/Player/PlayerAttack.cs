using UnityEngine;
using Godus.Combat;
using System;

namespace Godus.Player
{
    /// <summary>
    /// Attack component. Activates a child HitboxComponent during the attack window.
    /// Knight: heavy melee, wide arc, slow swing.
    /// </summary>
    public class PlayerAttack : MonoBehaviour
    {
        [SerializeField] private Transform attackHitboxOrigin;
        [SerializeField] private HitboxComponent attackHitbox;

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

            // Activate hitbox at hit window
            if (!_hitDealt && _attackTimer >= _stats.attackHitWindow && attackHitbox != null)
            {
                attackHitbox.Activate(_stats.baseAttackDamage, _stats.attackDuration - _stats.attackHitWindow);
                _hitDealt = true;
            }

            if (_attackTimer >= _stats.attackDuration + _stats.attackCooldown)
            {
                _isAttacking = false;
                _onComplete?.Invoke();
            }
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
}
