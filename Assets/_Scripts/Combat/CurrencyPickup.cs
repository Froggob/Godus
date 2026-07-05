using UnityEngine;
using Godus.Core;

namespace Godus.Combat
{
    /// <summary>
    /// Currency pickup dropped by enemies. Magnetizes to player when close.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class CurrencyPickup : MonoBehaviour
    {
        [SerializeField] private int amount = 5;
        [SerializeField] private float magnetRange = 3f;
        [SerializeField] private float magnetSpeed = 8f;
        [SerializeField] private float lifetime = 10f;

        private Transform _player;
        private Rigidbody2D _rb;
        private float _timer;

        public int Amount { get => amount; set => amount = value; }

        private void Awake()
        {
            var col = GetComponent<Collider2D>();
            col.isTrigger = true;
            _rb = GetComponent<Rigidbody2D>();
            if (_rb == null) _rb = gameObject.AddComponent<Rigidbody2D>();
            _rb.gravityScale = 1f;

            // Small random bounce on spawn
            _rb.linearVelocity = new Vector2(Random.Range(-1f, 1f), Random.Range(2f, 4f));
        }

        private void Start()
        {
            _player = GameObject.FindGameObjectWithTag("Player")?.transform;
        }

        private void Update()
        {
            _timer += Time.deltaTime;
            if (_timer > lifetime)
            {
                Destroy(gameObject);
                return;
            }

            if (_player == null) return;

            float dist = Vector2.Distance(transform.position, _player.position);
            if (dist < magnetRange)
            {
                Vector2 dir = (_player.position - transform.position).normalized;
                float speed = Mathf.Lerp(magnetSpeed, magnetSpeed * 1.5f, 1f - dist / magnetRange);
                _rb.linearVelocity = dir * speed;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                Collect();
            }
        }

        private void Collect()
        {
            SaveManager.AddCurrency(amount);
            Destroy(gameObject);
        }

        /// <summary>Spawn a pickup at a position.</summary>
        public static CurrencyPickup Spawn(Vector2 position, int amount)
        {
            var go = new GameObject("CurrencyPickup");
            go.transform.position = position;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = CreateCoinSprite();

            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.3f;
            col.isTrigger = true;

            var pickup = go.AddComponent<CurrencyPickup>();
            pickup.amount = amount;
            return pickup;
        }

        private static Sprite CreateCoinSprite()
        {
            var tex = new Texture2D(16, 16, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            var pixels = new Color[256];
            var yellow = new Color(1f, 0.85f, 0.1f);
            for (int y = 0; y < 16; y++)
                for (int x = 0; x < 16; x++)
                {
                    int dx = x - 8, dy = y - 8;
                    float r = Mathf.Sqrt(dx * dx + dy * dy);
                    bool edge = r > 6.5f || r < 3f;
                    pixels[y * 16 + x] = edge ? Color.clear : yellow;
                }
            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, 16, 16), new Vector2(0.5f, 0.5f), 100f);
        }
    }
}
