using UnityEngine;
using System.Collections.Generic;

namespace Godus.Systems
{
    /// <summary>
    /// Data for a hand-authored room chunk. Create in the Inspector.
    /// Each chunk has entry/exit points that the stitcher uses to connect rooms.
    /// </summary>
    [CreateAssetMenu(fileName = "RoomChunk_New", menuName = "Godus/Room Chunk")]
    public class RoomChunkData : ScriptableObject
    {
        [Header("Identity")]
        public string chunkId;
        public string biome = "Default";

        [Header("Dimensions")]
        public Vector2 chunkSize = new(20f, 12f);

        [Header("Difficulty")]
        [Range(1, 5)] public int difficultyTier = 1;
        public int minFloor = 0;
        public int maxFloor = 99;

        [Header("Exit Points (where rooms connect)")]
        public List<ExitPoint> exits = new();

        [Header("Flags")]
        public bool isStartRoom;
        public bool isBossRoom;
        public bool isMerchantRoom;
        public bool isTreasureRoom;

        [Header("Enemy Spawns")]
        public List<EnemySpawn> enemySpawns = new();

        [Header("Prefab")]
        public GameObject roomPrefab;
    }

    [System.Serializable]
    public class ExitPoint
    {
        public enum Direction { Left, Right, Up, Down }

        public Direction direction;
        public Vector2 localPosition; // position relative to chunk center
        public bool isEntry;          // true = entry, false = exit

        /// <summary>
        /// Get the matching opposite direction for stitching.
        /// </summary>
        public Direction Opposite => direction switch
        {
            Direction.Left => Direction.Right,
            Direction.Right => Direction.Left,
            Direction.Up => Direction.Down,
            Direction.Down => Direction.Up,
            _ => Direction.Right
        };
    }

    [System.Serializable]
    public class EnemySpawn
    {
        public GameObject enemyPrefab;
        public Vector2 position;
    }
}
