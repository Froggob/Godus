using System.IO;
using UnityEngine;

namespace Godus.Core
{
    /// <summary>
    /// Handles save/load of meta-progression data.
    /// In-run state is NOT saved — only meta-progression persists.
    /// Uses JSON serialization for human-readable saves.
    /// </summary>
    public static class SaveManager
    {
        private const string SaveFileName = "godus_save.json";
        private const int CurrentSaveVersion = 1;

        public static SaveData CurrentSave { get; private set; }

        private static string SavePath =>
            Path.Combine(Application.persistentDataPath, SaveFileName);

        /// <summary>
        /// Load save data from disk. Creates a fresh save if none exists.
        /// </summary>
        public static void Load()
        {
            if (File.Exists(SavePath))
            {
                try
                {
                    var json = File.ReadAllText(SavePath);
                    CurrentSave = JsonUtility.FromJson<SaveData>(json);

                    if (CurrentSave == null)
                    {
                        Debug.LogWarning("[SaveManager] Save file was corrupt. Creating fresh save.");
                        CreateFreshSave();
                    }
                    else if (CurrentSave.saveVersion < CurrentSaveVersion)
                    {
                        Debug.Log($"[SaveManager] Migrating save from v{CurrentSave.saveVersion} to v{CurrentSaveVersion}");
                        MigrateSave(CurrentSave.saveVersion, CurrentSaveVersion);
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[SaveManager] Failed to load save: {e.Message}. Creating fresh save.");
                    CreateFreshSave();
                }
            }
            else
            {
                CreateFreshSave();
            }

            EventBus.EmitGameLoaded();
        }

        /// <summary>
        /// Save current data to disk.
        /// </summary>
        public static void Save()
        {
            if (CurrentSave == null)
            {
                Debug.LogError("[SaveManager] Cannot save — no save data loaded.");
                return;
            }

            try
            {
                CurrentSave.saveVersion = CurrentSaveVersion;
                var json = JsonUtility.ToJson(CurrentSave, prettyPrint: true);
                File.WriteAllText(SavePath, json);
                EventBus.EmitGameSaved();
                Debug.Log($"[SaveManager] Saved to {SavePath}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[SaveManager] Failed to save: {e.Message}");
            }
        }

        /// <summary>
        /// Add currency and save immediately.
        /// </summary>
        public static void AddCurrency(int amount)
        {
            if (CurrentSave == null) Load();
            CurrentSave.totalCurrency += amount;
            EventBus.EmitCurrencyChanged(CurrentSave.totalCurrency);
            Save();
        }

        /// <summary>
        /// Spend currency. Returns false if insufficient funds.
        /// </summary>
        public static bool SpendCurrency(int amount)
        {
            if (CurrentSave == null) Load();
            if (CurrentSave.totalCurrency < amount) return false;

            CurrentSave.totalCurrency -= amount;
            EventBus.EmitCurrencyChanged(CurrentSave.totalCurrency);
            Save();
            return true;
        }

        /// <summary>
        /// Unlock an item and add it to the drop pool.
        /// </summary>
        public static void UnlockItem(string itemId)
        {
            if (CurrentSave == null) Load();
            if (CurrentSave.unlockedItemIds.Contains(itemId)) return;

            CurrentSave.unlockedItemIds.Add(itemId);
            EventBus.EmitItemUnlocked(itemId);
            Save();
        }

        /// <summary>
        /// Purchase an upgrade node.
        /// </summary>
        public static void PurchaseUpgrade(string nodeId)
        {
            if (CurrentSave == null) Load();
            if (CurrentSave.purchasedUpgradeNodeIds.Contains(nodeId)) return;

            CurrentSave.purchasedUpgradeNodeIds.Add(nodeId);
            EventBus.EmitUpgradePurchased(nodeId);
            Save();
        }

        private static void CreateFreshSave()
        {
            CurrentSave = new SaveData
            {
                saveVersion = CurrentSaveVersion
            };
            Save();
            Debug.Log("[SaveManager] Created fresh save.");
        }

        private static void MigrateSave(int fromVersion, int toVersion)
        {
            // Add migration steps here as schema evolves.
            // Example:
            // if (fromVersion < 2) { CurrentSave.someNewField = defaultValue; }
            CurrentSave.saveVersion = toVersion;
            Save();
        }
    }
}
