using UnityEngine;
using Godus.Combat;

namespace Godus.Enemies
{
    /// <summary>
    /// Boss enemy. Inherits EnemyBase, adds multi-phase attack patterns.
    /// Simple for M2: charge attack + ground slam.
    /// </summary>
    public class BossEnemy : EnemyBase
    {
        [Header("Boss Stats")]
        [SerializeField] private int phase2HPPercent = 50; // % HP to enter phase 2
        [SerializeField] private float chargeSpeed = 8f;
        [SerializeField] private float chargeWindup = 0.5f;
        [SerializeField] private float slamRadius = 3f;
        [SerializeField] private int slamDamage = 20;

        private bool _phase2;
        private float _chargeTimer;
        private bool _charging;

        protected override void Awake()
        {
            base.Awake();
            maxHP = 200;
            moveSpeed = 1.5f;
            chaseRange = 12f;
            attackRange = 2f;
            attackCooldown = 1.5f;
            currencyDrop = 100;
        }

        protected override void Update()
        {
            base.Update();

            // Phase transition
            if (!_phase2 && _health.CurrentHP <= maxHP * phase2HPPercent / 100f)
            {
                _phase2 = true;
                moveSpeed *= 1.5f;
                attackCooldown *= 0.7f;
                Debug.Log("[Boss] Phase 2 — enraged!");
            }

            // Charge attack (phase 2)
            if (_phase2 && _chargeTimer > 0f)
            {
                _chargeTimer -= Time.deltaTime;
                if (_chargeTimer <= 0f && !_charging)
                {
                    StartCharge();
                }
            }
        }

        protected override void OnChase()
        {
            base.OnChase();

            // In phase 2, occasionally wind up a charge
            if (_phase2 && _chargeTimer <= 0f && !_charging && Random.value < 0.003f)
            {
                _chargeTimer = chargeWindup;
                _moveDir = Vector2.zero; // pause briefly
            }
        }

        private void StartCharge()
        {
            if (_player == null) return;
            _charging = true;
            _moveDir = (_player.position - transform.position).normalized;
            _rb.linearVelocity = _moveDir * chargeSpeed;

            // Enable contact damage during charge
            if (attackHitbox != null)
            {
                attackHitbox.Damage = slamDamage;
                attackHitbox.Activate();
            }

            Invoke(nameof(EndCharge), 0.6f);
        }

        private void EndCharge()
        {
            _charging = false;
            _rb.linearVelocity = Vector2.zero;
            attackHitbox?.Deactivate();

            // Ground slam on charge end
            SlamAttack();
        }

        private void SlamAttack()
        {
            var hits = Physics2D.OverlapCircleAll(transform.position, slamRadius);
            foreach (var hit in hits)
            {
                var hurtbox = hit.GetComponent<HurtboxComponent>();
                if (hurtbox != null)
                {
                    Vector2 dir = (hit.transform.position - transform.position).normalized;
                    hurtbox.TakeHit(slamDamage, dir, 10f);
                }
            }

            Debug.Log("[Boss] Ground slam!");
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, slamRadius);
        }
    }
}
