using UnityEngine;

namespace Godus.Combat
{
    /// <summary>
    /// Receives damage from hitboxes. Lives on layer 2.
    /// Forwards to a HealthComponent on the same or parent GameObject.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class HurtboxComponent : MonoBehaviour
    {
        [SerializeField] private HealthComponent health;

        private Collider2D _col;

        private void Awake()
        {
            _col = GetComponent<Collider2D>();
            _col.isTrigger = true;
            _col.includeLayers = 0; // hurtboxes don't detect
            _col.excludeLayers = ~0;
            gameObject.layer = 2; // Layer 2 = Hurtboxes

            if (health == null)
                health = GetComponentInParent<HealthComponent>();
        }

        /// <summary>Called by HitboxComponent.OnTriggerEnter2D.</summary>
        public void TakeHit(int damage, Vector2 knockbackDir, float knockbackForce)
        {
            health?.TakeDamage(damage, knockbackDir, knockbackForce);
        }
    }
}
