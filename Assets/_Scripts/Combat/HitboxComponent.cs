using UnityEngine;

namespace Godus.Combat
{
    /// <summary>
    /// Deals damage to hurtboxes. Lives on layer 0, detects layer 2 (Hurtboxes) ONLY.
    /// Prevents friendly-fire, self-damage, terrain hits per the design doc.
    /// Attach to attack hitbox child GameObjects.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class HitboxComponent : MonoBehaviour
    {
        [SerializeField] private int damage = 10;
        [SerializeField] private float knockbackForce = 5f;
        [SerializeField] private float activeDuration = 0.2f;

        private float _timer;
        private bool _active;
        private Collider2D _col;

        public int Damage { get => damage; set => damage = value; }
        public float KnockbackForce { get => knockbackForce; set => knockbackForce = value; }

        private void Awake()
        {
            _col = GetComponent<Collider2D>();
            _col.isTrigger = true;
            _col.enabled = false;

            // Layer 0 detects layer 2
            _col.includeLayers = 1 << 2; // Layer 2 = Hurtboxes
            _col.excludeLayers = ~(1 << 2);
        }

        /// <summary>Activate the hitbox for a duration.</summary>
        public void Activate(int damageOverride = -1, float durationOverride = -1)
        {
            if (damageOverride >= 0) damage = damageOverride;
            if (durationOverride > 0) activeDuration = durationOverride;

            _timer = activeDuration;
            _active = true;
            _col.enabled = true;
        }

        /// <summary>Deactivate immediately.</summary>
        public void Deactivate()
        {
            _active = false;
            _col.enabled = false;
        }

        private void Update()
        {
            if (!_active) return;
            _timer -= Time.deltaTime;
            if (_timer <= 0f) Deactivate();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!_active) return;

            var hurtbox = other.GetComponent<HurtboxComponent>();
            if (hurtbox != null)
            {
                Vector2 dir = (other.transform.position - transform.position).normalized;
                hurtbox.TakeHit(damage, dir, knockbackForce);
            }
        }
    }
}
