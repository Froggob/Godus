using System;
using UnityEngine;

namespace Godus.Core
{
    /// <summary>
    /// Central event bus for cross-system communication.
    /// Systems communicate via C# events — no direct references.
    /// Prevents "changing the shop breaks the save system" failures.
    /// </summary>
    public static class EventBus
    {
        // ---- Player Events ----
        public static event Action<int, int> PlayerHealthChanged;      // current, max
        public static event Action PlayerDied;
        public static event Action<int> CurrencyChanged;               // new total
        public static event Action<string> PlayerEnteredRoom;          // room name/id

        // ---- Combat Events ----
        public static event Action<GameObject, int> EnemyDamaged;      // enemy, damage
        public static event Action<GameObject, Vector3> EnemyDied;     // enemy, position
        public static event Action<GameObject, int> PlayerDamaged;     // source, damage

        // ---- Run Events ----
        public static event Action RunStarted;
        public static event Action RunEnded;                           // death or completion
        public static event Action<int> FloorChanged;                  // new floor number

        // ---- Hub Events ----
        public static event Action HubEntered;
        public static event Action<string> ItemUnlocked;               // item ID
        public static event Action<string> UpgradePurchased;           // upgrade node ID

        // ---- Save Events ----
        public static event Action GameSaved;
        public static event Action GameLoaded;

        #region Emit Methods (safe fire-and-forget)

        public static void EmitPlayerHealthChanged(int current, int max) =>
            PlayerHealthChanged?.Invoke(current, max);

        public static void EmitPlayerDied() =>
            PlayerDied?.Invoke();

        public static void EmitCurrencyChanged(int newTotal) =>
            CurrencyChanged?.Invoke(newTotal);

        public static void EmitPlayerEnteredRoom(string roomId) =>
            PlayerEnteredRoom?.Invoke(roomId);

        public static void EmitEnemyDamaged(GameObject enemy, int damage) =>
            EnemyDamaged?.Invoke(enemy, damage);

        public static void EmitEnemyDied(GameObject enemy, Vector3 position) =>
            EnemyDied?.Invoke(enemy, position);

        public static void EmitPlayerDamaged(GameObject source, int damage) =>
            PlayerDamaged?.Invoke(source, damage);

        public static void EmitRunStarted() =>
            RunStarted?.Invoke();

        public static void EmitRunEnded() =>
            RunEnded?.Invoke();

        public static void EmitFloorChanged(int floor) =>
            FloorChanged?.Invoke(floor);

        public static void EmitHubEntered() =>
            HubEntered?.Invoke();

        public static void EmitItemUnlocked(string itemId) =>
            ItemUnlocked?.Invoke(itemId);

        public static void EmitUpgradePurchased(string nodeId) =>
            UpgradePurchased?.Invoke(nodeId);

        public static void EmitGameSaved() =>
            GameSaved?.Invoke();

        public static void EmitGameLoaded() =>
            GameLoaded?.Invoke();

        #endregion

        /// <summary>
        /// Clear all subscribers. Call when resetting for a new game or in tests.
        /// </summary>
        public static void ClearAll()
        {
            PlayerHealthChanged = null;
            PlayerDied = null;
            CurrencyChanged = null;
            PlayerEnteredRoom = null;
            EnemyDamaged = null;
            EnemyDied = null;
            PlayerDamaged = null;
            RunStarted = null;
            RunEnded = null;
            FloorChanged = null;
            HubEntered = null;
            ItemUnlocked = null;
            UpgradePurchased = null;
            GameSaved = null;
            GameLoaded = null;
        }
    }
}
