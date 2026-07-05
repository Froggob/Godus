using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Godus.Systems
{
    /// <summary>
    /// Procedural room stitcher. Selects hand-authored chunks and connects them
    /// via matching exit points. The Dead Cells / Gungeon approach.
    /// </summary>
    public class RoomStitcher : MonoBehaviour
    {
        [Header("Chunk Pool")]
        [SerializeField] private List<RoomChunkData> availableChunks = new();
        [SerializeField] private RoomChunkData startRoom;

        [Header("Generation")]
        [SerializeField] private int roomsPerFloor = 7;
        [SerializeField] private int currentFloor = 1;
        [SerializeField] private int maxDepth = 3;

        private List<PlacedRoom> _placedRooms = new();
        private System.Random _rng;

        public IReadOnlyList<PlacedRoom> PlacedRooms => _placedRooms;

        /// <summary>
        /// Generate a floor of connected rooms. Returns the start room.
        /// </summary>
        public GameObject Generate(int floor, int seed = 0)
        {
            _placedRooms.Clear();
            currentFloor = floor;
            _rng = seed == 0 ? new System.Random() : new System.Random(seed);

            // Pick chunks valid for this floor
            var validChunks = availableChunks
                .Where(c => floor >= c.minFloor && floor <= c.maxFloor)
                .ToList();

            if (validChunks.Count == 0)
            {
                Debug.LogError("[RoomStitcher] No valid chunks for floor " + floor);
                return null;
            }

            // Place start room at origin
            var startData = startRoom != null ? startRoom : validChunks.First(c => c.isStartRoom);
            if (startData == null) startData = validChunks[0];

            var startPlaced = PlaceRoom(startData, Vector2.zero);
            _placedRooms.Add(startPlaced);

            // Grow rooms from exits
            var frontier = new Queue<(PlacedRoom room, ExitPoint exit)>();
            foreach (var exit in startData.exits.Where(e => !e.isEntry))
                frontier.Enqueue((startPlaced, exit));

            int roomsPlaced = 1;
            while (frontier.Count > 0 && roomsPlaced < roomsPerFloor)
            {
                var (currentRoom, currentExit) = frontier.Dequeue();

                // Find matching chunks (have an entry matching this exit direction)
                var matchingChunks = validChunks
                    .Where(c => c.exits.Any(e => e.isEntry && e.direction == currentExit.Opposite))
                    .ToList();

                if (matchingChunks.Count == 0) continue;

                var nextData = matchingChunks[_rng.Next(matchingChunks.Count)];

                // Calculate position
                var entryPoint = nextData.exits.First(e =>
                    e.isEntry && e.direction == currentExit.Opposite);

                Vector2 worldExit = currentRoom.worldPosition + currentExit.localPosition;
                Vector2 newPos = worldExit - entryPoint.localPosition;

                // Offset by half-size for proper alignment
                newPos += GetExitOffset(currentExit.direction, nextData.chunkSize);

                var nextPlaced = PlaceRoom(nextData, newPos);
                _placedRooms.Add(nextPlaced);
                roomsPlaced++;

                // Add this room's exits to frontier (except the one we entered from)
                int depth = nextPlaced.depth;
                if (depth < maxDepth)
                {
                    foreach (var exit in nextData.exits.Where(e => !e.isEntry && e.direction != entryPoint.direction))
                        frontier.Enqueue((nextPlaced, exit));
                }
            }

            Debug.Log($"[RoomStitcher] Generated floor {floor}: {roomsPlaced} rooms");
            return startPlaced.instance;
        }

        private PlacedRoom PlaceRoom(RoomChunkData data, Vector2 position)
        {
            GameObject instance = null;
            if (data.roomPrefab != null)
            {
                instance = Instantiate(data.roomPrefab, position, Quaternion.identity);
            }
            else
            {
                // Create gray-box placeholder
                instance = CreateGrayBoxRoom(data, position);
            }

            int depth = 0;
            // Calculate depth from connected rooms
            foreach (var room in _placedRooms)
            {
                float dist = Vector2.Distance(room.worldPosition, position);
                if (dist < data.chunkSize.magnitude)
                    depth = room.depth + 1;
            }

            return new PlacedRoom
            {
                data = data,
                instance = instance,
                worldPosition = position,
                depth = depth
            };
        }

        private GameObject CreateGrayBoxRoom(RoomChunkData data, Vector2 position)
        {
            var room = new GameObject($"Room_{data.chunkId}_{_placedRooms.Count}");
            room.transform.position = position;

            // Visual bounds
            var sr = room.AddComponent<SpriteRenderer>();
            var tex = new Texture2D(2, 2);
            tex.SetPixels(new[] { Color.gray, Color.gray, Color.gray, Color.gray });
            tex.Apply();
            sr.sprite = Sprite.Create(tex, new Rect(0, 0, 2, 2), Vector2.zero, 1f);
            sr.drawMode = SpriteDrawMode.Sliced;
            sr.size = data.chunkSize;
            sr.color = new Color(0.2f, 0.2f, 0.25f, 0.5f);

            // Floor collider
            var floor = new GameObject("Floor") { layer = 1 };
            floor.transform.SetParent(room.transform);
            floor.transform.localPosition = new Vector3(0f, -data.chunkSize.y / 2f + 1f, 0f);
            var floorCol = floor.AddComponent<BoxCollider2D>();
            floorCol.size = new Vector2(data.chunkSize.x - 2f, 1f);

            // Spawn enemies
            foreach (var spawn in data.enemySpawns)
            {
                if (spawn.enemyPrefab != null)
                {
                    var enemy = Instantiate(spawn.enemyPrefab, room.transform);
                    enemy.transform.localPosition = spawn.position;
                }
            }

            return room;
        }

        private Vector2 GetExitOffset(ExitPoint.Direction dir, Vector2 chunkSize)
        {
            return dir switch
            {
                ExitPoint.Direction.Left => new Vector2(-chunkSize.x / 2f, 0),
                ExitPoint.Direction.Right => new Vector2(chunkSize.x / 2f, 0),
                ExitPoint.Direction.Up => new Vector2(0, chunkSize.y / 2f),
                ExitPoint.Direction.Down => new Vector2(0, -chunkSize.y / 2f),
                _ => Vector2.zero
            };
        }
    }

    /// <summary>
    /// Runtime state of a placed room in the generated floor.
    /// </summary>
    public class PlacedRoom
    {
        public RoomChunkData data;
        public GameObject instance;
        public Vector2 worldPosition;
        public int depth; // distance from start room

        public bool IsCleared { get; set; }
    }
}
