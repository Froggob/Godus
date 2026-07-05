using System;
using System.Collections.Generic;

namespace Godus.Core
{
    /// <summary>
    /// Save schema. Meta-progression data is fully separate from in-run state.
    /// Includes saveVersion from day one for safe migration.
    /// </summary>
    [Serializable]
    public class SaveData
    {
        /// <summary>Increment this when the save schema changes. Used for migration.</summary>
        public int saveVersion = 1;

        // ---- Meta-Progression (persists across runs) ----
        public int totalCurrency;
        public List<string> unlockedItemIds = new();
        public List<string> purchasedUpgradeNodeIds = new();
        public List<string> unlockedClassIds = new() { "Knight" }; // Knight is default

        // NPC dialogue state — tracks which dialogue milestones have been reached
        public Dictionary<string, int> npcDialogueProgress = new();

        // ---- Player Stats (meta-progression upgrades) ----
        public int permanentMaxHP = 100;
        public float permanentDamageBonus;
        public float permanentSpeedBonus;

        // ---- Stats Tracking ----
        public int totalRuns;
        public int totalDeaths;
        public int totalKills;
        public int highestFloorReached;
    }
}
