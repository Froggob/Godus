using UnityEngine;
using UnityEngine.InputSystem;
using Godus.Player;

namespace Godus.Core
{
    /// <summary>
    /// Milestone 1 bootstrap — creates the gray-box test room at runtime.
    /// Attach to a GameObject in an empty scene with a Main Camera.
    /// Controls: WASD=Move, J=Attack, K/Shift=Dash
    /// </summary>
    public class BootstrapM1 : MonoBehaviour
    {
        [Header("Ground Settings")]
        [SerializeField] private float groundWidth = 20f;
        [SerializeField] private float groundY = -3f;

        [Header("Player Spawn")]
        [SerializeField] private Vector2 playerSpawn = new(0f, -1f);

        private void Start()
        {
            CreateGround();
            CreatePlayer();
            EventBus.EmitRunStarted();
            Debug.Log("[BootstrapM1] Gray-box test room ready. WASD=Move, J=Attack, K/Shift=Dash");
        }

        private void CreateGround()
        {
            var ground = new GameObject("Ground") { layer = 1 };

            var sr = ground.AddComponent<SpriteRenderer>();
            sr.sprite = CreatePlaceholderSprite((int)(groundWidth * 100), 50, Color.darkGray);
            sr.drawMode = SpriteDrawMode.Sliced;
            sr.size = new Vector2(groundWidth, 0.5f);

            var bc = ground.AddComponent<BoxCollider2D>();
            bc.size = new Vector2(groundWidth, 0.5f);

            ground.transform.position = new Vector3(0f, groundY, 0f);
        }

        private void CreatePlayer()
        {
            var player = new GameObject("Player_Knight")
            {
                layer = 1,
                transform = { position = playerSpawn }
            };

            // Visual
            var sr = player.AddComponent<SpriteRenderer>();
            sr.sprite = CreatePlaceholderSprite(32, 48, new Color(0.3f, 0.5f, 0.9f));

            // Physics
            var rb = player.AddComponent<Rigidbody2D>();
            rb.gravityScale = 3f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            var col = player.AddComponent<CapsuleCollider2D>();
            col.size = new Vector2(0.6f, 1.4f);
            col.offset = new Vector2(0f, -0.1f);

            // Components
            var dash = player.AddComponent<PlayerDash>();
            var attack = player.AddComponent<PlayerAttack>();
            var controller = player.AddComponent<PlayerController>();
            var inputHandler = player.AddComponent<PlayerInputHandler>();

            // Stats — create default Knight stats at runtime
            var stats = ScriptableObject.CreateInstance<ClassStats>();
            stats.className = "Knight";
            stats.classDescription = "Tanky, slow, high poise. Heavy melee.";
            stats.moveSpeed = 4f;
            stats.acceleration = 30f;
            stats.deceleration = 25f;
            stats.maxHP = 120;
            stats.hurtIFrames = 0.5f;
            stats.baseAttackDamage = 25;
            stats.attackDuration = 0.45f;
            stats.attackHitWindow = 0.15f;
            stats.attackRange = 1.8f;
            stats.attackCooldown = 0.3f;
            stats.dashDistance = 3f;
            stats.dashDuration = 0.18f;
            stats.dashCooldown = 1.2f;
            stats.dashGrantsPoise = true;
            stats.dashPoiseReduction = 0.5f;
            stats.dashPoiseKnockbackResist = 0.8f;
            stats.groundLayer = 1;

            // Wire up via reflection (runtime-created objects can't use Inspector serialization)
            SetField(controller, "stats", stats);
            SetField(controller, "dash", dash);
            SetField(controller, "attack", attack);

            // Attack hitbox origin
            var hitboxOrigin = new GameObject("AttackOrigin");
            hitboxOrigin.transform.SetParent(player.transform);
            hitboxOrigin.transform.localPosition = new Vector3(1f, 0f, 0f);
            SetField(attack, "attackHitboxOrigin", hitboxOrigin.transform);

            // Input actions — create and wire up directly
            var inputActions = CreateInputActions();
            SetField(inputHandler, "inputActions", inputActions);
        }

        private InputActionAsset CreateInputActions()
        {
            var asset = ScriptableObject.CreateInstance<InputActionAsset>();

            var playerMap = asset.AddActionMap("Player");

            // Move action
            var moveAction = playerMap.AddAction("Move", InputActionType.Value, "<Gamepad>/leftStick");
            moveAction.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d");

            // Alternate: arrows
            moveAction.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/upArrow")
                .With("Down", "<Keyboard>/downArrow")
                .With("Left", "<Keyboard>/leftArrow")
                .With("Right", "<Keyboard>/rightArrow");

            // Attack
            var attackAction = playerMap.AddAction("Attack", InputActionType.Button, "<Keyboard>/j");
            attackAction.AddBinding("<Gamepad>/buttonWest");

            // Dash
            var dashAction = playerMap.AddAction("Dash", InputActionType.Button, "<Keyboard>/k");
            dashAction.AddBinding("<Keyboard>/leftShift");
            dashAction.AddBinding("<Gamepad>/buttonEast");

            return asset;
        }

        private Sprite CreatePlaceholderSprite(int width, int height, Color color)
        {
            var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point; // crisp pixel art
            var pixels = new Color[width * height];

            // Add a border for visibility
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    bool isBorder = x < 2 || x >= width - 2 || y < 2 || y >= height - 2;
                    pixels[y * width + x] = isBorder ? Color.white : color;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();

            return Sprite.Create(tex,
                new Rect(0, 0, width, height),
                new Vector2(0.5f, 0.5f),
                100f);
        }

        private static void SetField(object obj, string fieldName, object value)
        {
            var field = obj.GetType().GetField(fieldName,
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.Public);

            if (field != null)
            {
                field.SetValue(obj, value);
            }
            else
            {
                Debug.LogWarning($"[BootstrapM1] Field '{fieldName}' not found on {obj.GetType().Name}");
            }
        }
    }
}
