using UnityEngine;
using UnityEngine.InputSystem;
using Godus.Core;
using Godus.Combat;
using Godus.Enemies;
using Godus.UI;

namespace Godus.Player
{
    /// <summary>
    /// Milestone 2 bootstrap — player + enemy test room.
    /// Controls: WASD=Move, J=Attack, K/Shift=Dash
    /// </summary>
    public class BootstrapM1 : MonoBehaviour
    {
        [Header("Ground")]
        [SerializeField] private float groundWidth = 30f;
        [SerializeField] private float groundY = -3f;

        [Header("Spawns")]
        [SerializeField] private Vector2 playerSpawn = new(-8f, -1f);
        [SerializeField] private Vector2 enemySpawn = new(5f, -1.5f);

        private void Start()
        {
            EnsureGameManager();
            CreateGround();
            var player = CreatePlayer();
            CreateEnemy(player);
            EventBus.EmitRunStarted();
            CreateHUD();
            Debug.Log("[Bootstrap] Room ready. WASD=Move, J=Attack, K/Shift=Dash");
        }

        private void CreateGround()
        {
            var ground = new GameObject("Ground") { layer = 1 };
            var sr = ground.AddComponent<SpriteRenderer>();
            sr.sprite = CreatePlaceholderSprite((int)(groundWidth * 100), 50, Color.darkGray);
            sr.drawMode = SpriteDrawMode.Sliced;
            sr.size = new Vector2(groundWidth, 0.5f);
            ground.AddComponent<BoxCollider2D>().size = new Vector2(groundWidth, 0.5f);
            ground.transform.position = new Vector3(0f, groundY, 0f);
        }

        private GameObject CreatePlayer()
        {
            var p = new GameObject("Player_Knight")
            {
                tag = "Player",
                layer = 1,
                transform = { position = playerSpawn }
            };

            p.AddComponent<SpriteRenderer>().sprite =
                CreatePlaceholderSprite(32, 48, new Color(0.3f, 0.5f, 0.9f));

            var rb = p.AddComponent<Rigidbody2D>();
            rb.gravityScale = 3f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            var col = p.AddComponent<CapsuleCollider2D>();
            col.size = new Vector2(0.6f, 1.4f);
            col.offset = new Vector2(0f, -0.1f);

            // --- Combat components ---
            // Hitbox child (for attacks)
            var hitboxGo = new GameObject("AttackHitbox") { layer = 0 };
            hitboxGo.transform.SetParent(p.transform);
            hitboxGo.transform.localPosition = new Vector3(1f, 0f, 0f);
            var hitboxCol = hitboxGo.AddComponent<BoxCollider2D>();
            hitboxCol.size = new Vector2(1.8f, 1f);
            hitboxCol.isTrigger = true;
            var hitbox = hitboxGo.AddComponent<HitboxComponent>();

            // Hurtbox child
            var hurtboxGo = new GameObject("Hurtbox") { layer = 2 };
            hurtboxGo.transform.SetParent(p.transform);
            var hurtboxCol = hurtboxGo.AddComponent<BoxCollider2D>();
            hurtboxCol.size = new Vector2(0.7f, 1.5f);
            hurtboxCol.isTrigger = true;
            var hurtbox = hurtboxGo.AddComponent<HurtboxComponent>();

            // Health
            var health = p.AddComponent<HealthComponent>();
            SetField(health, "_maxHP", 120);
            SetField(health, "isPlayer", true);
            SetField(hurtbox, "health", health);

            // --- Input ---
            var inputActions = CreateInputActions();
            var inputHandler = p.AddComponent<PlayerInputHandler>();
            SetField(inputHandler, "inputActions", inputActions);
            inputActions.Enable();

            // --- Player components ---
            var dash = p.AddComponent<PlayerDash>();
            var attack = p.AddComponent<PlayerAttack>();
            var controller = p.AddComponent<PlayerController>();

            SetField(attack, "attackHitboxOrigin", hitboxGo.transform);
            SetField(attack, "attackHitbox", hitbox);

            // Stats
            var stats = ScriptableObject.CreateInstance<ClassStats>();
            stats.className = "Knight"; stats.classDescription = "Tanky, slow, high poise.";
            stats.moveSpeed = 4f; stats.acceleration = 30f; stats.deceleration = 25f;
            stats.maxHP = 120; stats.hurtIFrames = 0.5f;
            stats.baseAttackDamage = 25; stats.attackDuration = 0.45f;
            stats.attackHitWindow = 0.15f; stats.attackRange = 1.8f; stats.attackCooldown = 0.3f;
            stats.dashDistance = 3f; stats.dashDuration = 0.18f; stats.dashCooldown = 1.2f;
            stats.dashGrantsPoise = true; stats.dashPoiseReduction = 0.5f;
            stats.dashPoiseKnockbackResist = 0.8f; stats.groundLayer = 1;

            SetField(controller, "stats", stats);
            SetField(controller, "dash", dash);
            SetField(controller, "attack", attack);

            return p;
        }

        private void CreateEnemy(GameObject player)
        {
            var e = new GameObject("Enemy_Melee")
            {
                layer = 1,
                transform = { position = enemySpawn }
            };

            var sr = e.AddComponent<SpriteRenderer>();
            sr.sprite = CreatePlaceholderSprite(32, 40, new Color(0.9f, 0.2f, 0.2f));

            var rb = e.AddComponent<Rigidbody2D>();
            rb.gravityScale = 3f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;

            var col = e.AddComponent<CapsuleCollider2D>();
            col.size = new Vector2(0.5f, 1.2f);
            col.offset = new Vector2(0f, -0.1f);

            // Hurtbox
            var hurtboxGo = new GameObject("Hurtbox") { layer = 2 };
            hurtboxGo.transform.SetParent(e.transform);
            var hCol = hurtboxGo.AddComponent<BoxCollider2D>();
            hCol.size = new Vector2(0.6f, 1.3f);
            hCol.isTrigger = true;
            var hurtbox = hurtboxGo.AddComponent<HurtboxComponent>();

            // Hitbox
            var hitboxGo = new GameObject("AttackHitbox") { layer = 0 };
            hitboxGo.transform.SetParent(e.transform);
            hitboxGo.transform.localPosition = new Vector3(1f, 0f, 0f);
            var atkCol = hitboxGo.AddComponent<BoxCollider2D>();
            atkCol.size = new Vector2(1.3f, 0.8f);
            atkCol.isTrigger = true;
            var hitbox = hitboxGo.AddComponent<HitboxComponent>();

            // Health
            var health = e.AddComponent<HealthComponent>();
            SetField(health, "maxHP", 40);
            SetField(hurtbox, "health", health);

            // Enemy controller
            var enemy = e.AddComponent<EnemyBase>();
            SetField(enemy, "maxHP", 40);
            SetField(enemy, "moveSpeed", 2f);
            SetField(enemy, "contactDamage", 8);
            SetField(enemy, "chaseRange", 8f);
            SetField(enemy, "attackRange", 1.3f);
            SetField(enemy, "attackCooldown", 1.2f);
            SetField(enemy, "currencyDrop", 5);
            SetField(enemy, "attackHitbox", hitbox);
        }

        private void CreateHUD()
        {
            var hud = new GameObject("HUD");
            hud.AddComponent<CurrencyHUD>();
        }

        private void EnsureGameManager()
        {
            if (GameManager.Instance == null)
            {
                var gm = new GameObject("GameManager");
                gm.AddComponent<GameManager>();
            }
        }

        // --- Helpers (same as before) ---

        private InputActionAsset CreateInputActions()
        {
            var asset = ScriptableObject.CreateInstance<InputActionAsset>();
            var map = asset.AddActionMap("Player");

            var move = map.AddAction("Move", InputActionType.Value, "<Gamepad>/leftStick");
            move.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w").With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a").With("Right", "<Keyboard>/d");
            move.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/upArrow").With("Down", "<Keyboard>/downArrow")
                .With("Left", "<Keyboard>/leftArrow").With("Right", "<Keyboard>/rightArrow");

            var attack = map.AddAction("Attack", InputActionType.Button, "<Keyboard>/j");
            attack.AddBinding("<Gamepad>/buttonWest");

            var dash = map.AddAction("Dash", InputActionType.Button, "<Keyboard>/k");
            dash.AddBinding("<Keyboard>/leftShift");
            dash.AddBinding("<Gamepad>/buttonEast");

            return asset;
        }

        private Sprite CreatePlaceholderSprite(int w, int h, Color color)
        {
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            var pixels = new Color[w * h];
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                    pixels[y * w + x] = (x < 2 || x >= w - 2 || y < 2 || y >= h - 2) ? Color.white : color;
            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f);
        }

        private static void SetField(object obj, string name, object value)
        {
            var f = obj.GetType().GetField(name,
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            if (f != null) f.SetValue(obj, value);
            else Debug.LogWarning($"[Bootstrap] Field '{name}' not found on {obj.GetType().Name}");
        }
    }
}
