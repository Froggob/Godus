using UnityEngine;

namespace Godus.Core
{
    /// <summary>
    /// Bootstrap manager. Lives in the persistent scene (Scene 0).
    /// Handles game-wide state and scene transitions.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Scenes")]
        [SerializeField] private string hubSceneName = "Hub";
        [SerializeField] private string runSceneName = "Run";

        public bool IsInRun { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnEnable()
        {
            EventBus.RunStarted += OnRunStarted;
            EventBus.RunEnded += OnRunEnded;
            EventBus.PlayerDied += OnPlayerDied;
        }

        private void OnDisable()
        {
            EventBus.RunStarted -= OnRunStarted;
            EventBus.RunEnded -= OnRunEnded;
            EventBus.PlayerDied -= OnPlayerDied;
        }

        public void StartRun()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(runSceneName);
        }

        public void ReturnToHub()
        {
            IsInRun = false;
            UnityEngine.SceneManagement.SceneManager.LoadScene(hubSceneName);
        }

        private void OnRunStarted()
        {
            IsInRun = true;
        }

        private void OnRunEnded()
        {
            IsInRun = false;
        }

        private void OnPlayerDied()
        {
            // Death = run over, return to hub
            // Currency/unlocks are kept via the save system
            ReturnToHub();
        }
    }
}
