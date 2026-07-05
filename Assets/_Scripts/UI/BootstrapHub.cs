using UnityEngine;
using Godus.Core;

namespace Godus.UI
{
    /// <summary>
    /// Hub scene bootstrap. Creates the persistent hub area with placeholder NPCs.
    /// Player returns here after death. Currency can be spent here.
    /// </summary>
    public class BootstrapHub : MonoBehaviour
    {
        [Header("Hub Layout")]
        [SerializeField] private float groundWidth = 40f;
        [SerializeField] private float groundY = -2f;

        private void Start()
        {
            EnsureGameManager();
            CreateGround();
            CreateNPCs();
            CreateHUD();
            EventBus.EmitHubEntered();
            Debug.Log("[Hub] Welcome back. Press Enter to start a run.");
        }

        private void EnsureGameManager()
        {
            if (GameManager.Instance == null)
            {
                var gm = new GameObject("GameManager");
                gm.AddComponent<GameManager>();
            }
        }

        private void CreateGround()
        {
            var ground = new GameObject("Ground") { layer = 1 };
            var sr = ground.AddComponent<SpriteRenderer>();
            sr.sprite = CreatePlaceholderSprite((int)(groundWidth * 100), 50, new Color(0.15f, 0.2f, 0.15f));
            sr.drawMode = SpriteDrawMode.Sliced;
            sr.size = new Vector2(groundWidth, 0.5f);
            ground.AddComponent<BoxCollider2D>().size = new Vector2(groundWidth, 0.5f);
            ground.transform.position = new Vector3(0f, groundY, 0f);
        }

        private void CreateNPCs()
        {
            // Merchant (left)
            SpawnNPC("Merchant", new Vector3(-12f, -0.3f), new Color(0.2f, 0.8f, 0.2f));
            // Upgrade tree (center)
            SpawnNPC("UpgradeTree", new Vector3(0f, -0.3f), new Color(0.8f, 0.8f, 0.2f));
            // Lore NPC (right)
            SpawnNPC("NPC_Lore", new Vector3(12f, -0.3f), new Color(0.6f, 0.4f, 0.8f));
        }

        private void SpawnNPC(string name, Vector3 position, Color color)
        {
            var npc = new GameObject(name);
            npc.transform.position = position;
            var sr = npc.AddComponent<SpriteRenderer>();
            sr.sprite = CreatePlaceholderSprite(24, 48, color);

            // Interaction trigger
            var col = npc.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(2f, 3f);
        }

        private void CreateHUD()
        {
            var hud = new GameObject("HUD");
            hud.AddComponent<CurrencyHUD>();
            hud.AddComponent<HubInputHandler>();
        }

        // Moved from BootstrapM1 — shared helpers should eventually live in a Utils class
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
    }

    /// <summary>
    /// Simple input handler for the Hub scene. Enter/space starts a run.
    /// </summary>
    public class HubInputHandler : MonoBehaviour
    {
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
            {
                if (GameManager.Instance != null)
                    GameManager.Instance.StartRun();
                else
                    UnityEngine.SceneManagement.SceneManager.LoadScene("TestRoom_M1");
            }
        }
    }
}
