using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class RoomWavesDefenition {
    public RoomWavesType type;
    public int maxWavesCount;
}

[System.Serializable]
public class EnemyTypeDefenition {
    public EnemyType type;
    public int maxEnemiesCount;
}

public class LevelGenerator : MonoBehaviour {
    [Header("Room Prefabs")]
    [SerializeField] private string _playerSpawnpoint = "Player_Spawnpoint";
    [SerializeField] private List<GameObject> _startingRoomPrefabs = new();
    [SerializeField] private List<GameObject> _regularRoomPrefabs = new();
    [SerializeField] private List<GameObject> _corridorPrefabs = new();
    [SerializeField] private List<GameObject> _endingRoomPrefabs = new();

    [Header("Generation settings")]
    [SerializeField] private int _minRoomCount = 1;
    [SerializeField] private int _maxRoomCount = 5;
    [Min(0f), SerializeField] private float _minExtraConnections = 0f;
    [Min(0f), SerializeField] private float _maxExtraConnections = 0.5f;

    [Header("Enemies settings")]
    [SerializeField] private List<EnemyDefenition> _enemies;
    [SerializeField] private List<RoomWavesDefenition> _maxWavesInRoom;
    [SerializeField] private List<EnemyTypeDefenition> _maxEnemiesOfType;

    private Transform _player;
    private Dictionary<RoomType, List<RoomDefenition>> _roomDefenitions;
    private readonly List<(Vector2Int, Rect)> _desiredRoomRects = new();
    private readonly List<(Vector2Int, Rect)> _prefabRoomRects = new();

    private void OnEnable() {
        _player = GameObject.FindGameObjectWithTag("Player").transform;

        _roomDefenitions = new Dictionary<RoomType, List<RoomDefenition>>() {
            { RoomType.Start, _startingRoomPrefabs.Select(prefab => new RoomDefenition(prefab)).ToList() },
            { RoomType.Regular, _regularRoomPrefabs.Select(prefab => new RoomDefenition(prefab)).ToList() },
            { RoomType.Corridor, _corridorPrefabs.Select(prefab => new RoomDefenition(prefab)).ToList() },
            { RoomType.End, _endingRoomPrefabs.Select(prefab => new RoomDefenition(prefab)).ToList() }
        };

        int numberOfRooms = Random.Range(_minRoomCount, _maxRoomCount) + 2; // + start/end rooms
        int extraConnections = (int)(numberOfRooms * Random.Range(_minExtraConnections, _maxExtraConnections));

        HashSet<int> validDoorPatterns = new();

        foreach (var (roomType, rooms) in _roomDefenitions) {
            foreach (DoorContainer doorContainer in rooms.Select(room => room.DoorContainer)) {
                validDoorPatterns.Add(doorContainer.DoorsPattern);
            }
        }

        LevelGraphData levelLayout = GridGenerator.Generate(numberOfRooms, extraConnections, validDoorPatterns);

        Debug.Log(string.Join("\n", levelLayout.Edges.Select(pair => $"{pair.Item1} <-> {pair.Item2}")));

        Dictionary<Vector2Int, RoomDefenition> selectedRooms = PrefabSelector.Select(levelLayout, _roomDefenitions);

        List<GameObject> spawnedRooms = InstantiateRooms(levelLayout, selectedRooms);

        EnemyWavesGenerator.Generate(spawnedRooms, _enemies, _maxWavesInRoom, _maxEnemiesOfType);
    }

    private void OnDrawGizmos() {
        if (!Application.isPlaying) {
            return;
        }

        foreach (var (grid, rect) in _desiredRoomRects) {
            DrawRectGizmo(rect, Color.green);

            UnityEditor.Handles.Label(rect.center + 6 * Vector2.left, $"[{grid.x}, {grid.y}]");
        }

        foreach (var (grid, rect) in _prefabRoomRects) {
            DrawRectGizmo(rect, Color.blue);

            UnityEditor.Handles.Label(rect.center + 6 * Vector2.left, $"[{grid.x}, {grid.y}]");
        }
    }

    private Vector2 CalculateMaxRoomArea(Dictionary<Vector2Int, RoomDefenition> selectedRooms) {
        Vector2 maxArea = Vector2.zero;

        foreach (Rect roomBounds in selectedRooms.Values.Select(room => room.Bounds)) {
            foreach (Rect corridorBounds in _roomDefenitions[RoomType.Corridor].Select(corridor => corridor.Bounds)) {
                Vector2 currentArea = roomBounds.size + 4 * corridorBounds.size;

                maxArea.x = Mathf.Max(maxArea.x, currentArea.x);
                maxArea.y = Mathf.Max(maxArea.y, currentArea.y);
            }
        }

        return maxArea;
    }

    private Vector2 GetDoorPosition(RoomDefenition room, DoorDirection direction) {
        try {
            return room.DoorContainer.Doors
                .Where(door => door.direction == direction)
                .First()
                .connectionPosition;
        } catch (System.InvalidOperationException) {
            throw new System.ArgumentException($"Can't find doors with {direction} in room");
        }
    }

    private List<GameObject> InstantiateRooms(
        LevelGraphData levelLayout,
        Dictionary<Vector2Int, RoomDefenition> selectedRooms
    ) {
        Vector2 maxAreaForRoom = CalculateMaxRoomArea(selectedRooms);

        Dictionary<Vector2Int, GameObject> spawnedRooms = new();

        Queue<Vector2Int> queue = new();
        HashSet<Vector2Int> used = new();

        queue.Enqueue(Vector2Int.zero);
        used.Add(Vector2Int.zero);

        while (queue.Count > 0) {
            Vector2Int currentGridPos = queue.Dequeue();
            GameObject roomPrefab = selectedRooms[currentGridPos].GameObject;
            Rect desiredRoomRect = new(
                currentGridPos.x * maxAreaForRoom.x,
                currentGridPos.y * maxAreaForRoom.y,
                maxAreaForRoom.x,
                maxAreaForRoom.y
            );

            Vector2 spawnPosition = -selectedRooms[currentGridPos].Bounds.min;
            bool foundPosition = false;

            foreach (Vector2Int direction in GridGenerator.Directions) {
                Vector2Int neighbourGridPos = currentGridPos + direction;

                bool existsEdge = levelLayout.Edges.Contains((currentGridPos, neighbourGridPos))
                    || levelLayout.Edges.Contains((neighbourGridPos, currentGridPos));

                if (!used.Contains(neighbourGridPos) && existsEdge) {
                    queue.Enqueue(neighbourGridPos);
                    used.Add(neighbourGridPos);
                }

                if (!foundPosition && existsEdge && spawnedRooms.ContainsKey(neighbourGridPos)) {
                    spawnPosition = InstantiateCorridors(
                        direction,
                        currentGridPos,
                        neighbourGridPos,
                        maxAreaForRoom,
                        spawnedRooms,
                        selectedRooms
                    );
                    foundPosition = true;
                }
            }

            _desiredRoomRects.Add((currentGridPos, desiredRoomRect));

            spawnedRooms.Add(currentGridPos, Instantiate(
                roomPrefab,
                spawnPosition,
                roomPrefab.transform.rotation,
                transform
            ));
        }

        PostProcess(spawnedRooms);

        return spawnedRooms.Values.ToList();
    }

    private void PostProcess(Dictionary<Vector2Int, GameObject> spawnedRooms) {
        Transform spawnPoint = spawnedRooms[Vector2Int.zero].transform.Find(_playerSpawnpoint);
        _player.position = spawnPoint.position;

        PlayerHealth playerHealth = _player.GetComponent<PlayerHealth>();
        PlayerMovement playerMovement = _player.GetComponent<PlayerMovement>();

        foreach (GameObject room in spawnedRooms.Values) {
            Transform doorContainer = room.transform.Find("Doors");

            if (doorContainer) {
                foreach (Transform door in doorContainer) {
                    if (door.CompareTag("Door")) {
                        door.gameObject.SetActive(false);
                    }
                }
            }

            Transform abyssContainer = room.transform.Find("Abyss");

            if (abyssContainer) {
                abyssContainer.GetComponent<AbyssMaster>().InitPlayer(playerMovement, playerHealth);
            }
        }
    }

    private RoomDefenition GetRandomCorridor(CorridorPattern corridorPattern) {
        List<RoomDefenition> suitableCorridors = _roomDefenitions[RoomType.Corridor]
            .Where(corridor => corridor.DoorContainer.DoorsPattern == (int)corridorPattern)
            .ToList();
        suitableCorridors.Shuffle();

        return suitableCorridors[0];
    }

    private Vector2 InstantiateTurningCorridors(
        Vector2Int direction,
        Vector2Int currentGridPos,
        Vector2Int neighbourGridPos,
        Vector2 maxAreaForRoom,
        Dictionary<Vector2Int, GameObject> spawnedRooms,
        Dictionary<Vector2Int, RoomDefenition> selectedRooms
    ) {
        Rect desiredRoomRect = new(
            currentGridPos.x * maxAreaForRoom.x,
            currentGridPos.y * maxAreaForRoom.y,
            maxAreaForRoom.x,
            maxAreaForRoom.y
        );

        DoorDirection currentDoorDir = direction.GetDoorDirection();
        DoorDirection neighbourDoorDir = currentDoorDir.GetOpposite();

        Vector2 currentDoorPos = GetDoorPosition(selectedRooms[currentGridPos], currentDoorDir);

        Vector2 neighbourRoomPos = (Vector2)spawnedRooms[neighbourGridPos].transform.position;
        Vector2 neighbourDoorPos = GetDoorPosition(selectedRooms[neighbourGridPos], neighbourDoorDir);
        Vector2 newObjectPos = neighbourRoomPos + neighbourDoorPos;

        Rect roomPrefabRect = selectedRooms[currentGridPos].Bounds;
        Vector2Int correctingDirection = Vector2Int.zero;

        switch (neighbourDoorDir) {
            case DoorDirection.Left:
                roomPrefabRect.x += desiredRoomRect.xMax - roomPrefabRect.xMax;
                roomPrefabRect.y = newObjectPos.y - (currentDoorPos.y - roomPrefabRect.yMin);
                correctingDirection = roomPrefabRect.yMin < desiredRoomRect.yMin ? Vector2Int.up : Vector2Int.down;
                break;
            case DoorDirection.Right:
                roomPrefabRect.x += desiredRoomRect.xMin - roomPrefabRect.xMin;
                roomPrefabRect.y = newObjectPos.y - (currentDoorPos.y - roomPrefabRect.yMin);
                correctingDirection = roomPrefabRect.yMin < desiredRoomRect.yMin ? Vector2Int.up : Vector2Int.down;
                break;
            case DoorDirection.Up:
                roomPrefabRect.y += desiredRoomRect.yMin - roomPrefabRect.yMin;
                roomPrefabRect.x = newObjectPos.x - (currentDoorPos.x - roomPrefabRect.xMin);
                correctingDirection = roomPrefabRect.xMin < desiredRoomRect.xMin ? Vector2Int.right : Vector2Int.left;
                break;
            case DoorDirection.Down:
                roomPrefabRect.y += desiredRoomRect.yMax - roomPrefabRect.yMax;
                roomPrefabRect.x = newObjectPos.x - (currentDoorPos.x - roomPrefabRect.xMin);
                correctingDirection = roomPrefabRect.xMin < desiredRoomRect.xMin ? Vector2Int.right : Vector2Int.left;
                break;
        }

        _prefabRoomRects.Add((currentGridPos, roomPrefabRect));
        if (desiredRoomRect.Contains(roomPrefabRect)) {
            return newObjectPos;
        }

        Debug.Log($"Needs correction: {neighbourGridPos} -> {currentGridPos}");

        DoorDirection tangentDoorDir = correctingDirection.GetDoorDirection();
        DoorDirection oppositeTangentDoorDir = tangentDoorDir.GetOpposite();

        CorridorPattern firstCorridorPattern = (CorridorPattern)(
            (1 << (int)currentDoorDir) | (1 << (int)tangentDoorDir)
        );

        CorridorPattern tangentCorridorPattern = correctingDirection.x == 0 ?
            CorridorPattern.Vertical : CorridorPattern.Horizontal;

        CorridorPattern secondCorridorPattern = (CorridorPattern)(
            (1 << (int)neighbourDoorDir) | (1 << (int)oppositeTangentDoorDir)
        );

        CorridorDefenition firstCorridorDef = new(GetRandomCorridor(firstCorridorPattern));
        CorridorDefenition tangentCorridorDef = new(GetRandomCorridor(tangentCorridorPattern));
        CorridorDefenition secondCorridorDef = new(GetRandomCorridor(secondCorridorPattern));

        Vector2 firstCorridorEntryPos = firstCorridorDef.GetEntry(tangentDoorDir);
        Vector2 firstCorridorExitPos = firstCorridorDef.GetExit(tangentDoorDir);
        Vector2 tangentCorridorEntryPos = tangentCorridorDef.GetEntry(tangentDoorDir);
        Vector2 tangentCorridorExitPos = tangentCorridorDef.GetExit(tangentDoorDir);
        Vector2 secondCorridorEntryPos = secondCorridorDef.GetEntry(neighbourDoorDir);
        Vector2 secondCorridorExitPos = secondCorridorDef.GetExit(neighbourDoorDir);

        Instantiate(
            firstCorridorDef.GameObject,
            newObjectPos - firstCorridorExitPos,
            firstCorridorDef.GameObject.transform.rotation,
            transform
        );

        newObjectPos += firstCorridorEntryPos - firstCorridorExitPos;

        for (int i = 0; ; ++i) {
            Rect newObjectRect = new(
                newObjectPos - tangentCorridorExitPos + tangentCorridorDef.Bounds.min,
                tangentCorridorDef.Bounds.size
            );

            switch (neighbourDoorDir) {
                case DoorDirection.Left:
                    newObjectRect.xMax = desiredRoomRect.xMax;
                    break;
                case DoorDirection.Right:
                    newObjectRect.xMin = desiredRoomRect.xMin;
                    break;
                case DoorDirection.Up:
                    newObjectRect.yMin = desiredRoomRect.yMin;
                    break;
                case DoorDirection.Down:
                    newObjectRect.yMax = desiredRoomRect.yMax;
                    break;
            }

            if (desiredRoomRect.Contains(newObjectRect)) {
                break;
            } else if (i == 32) {
                Debug.LogError("Infinite cycle of placing corridors");
                return newObjectPos;
            }

            Instantiate(
                tangentCorridorDef.GameObject,
                newObjectPos - tangentCorridorExitPos,
                tangentCorridorDef.GameObject.transform.rotation,
                transform
            );

            newObjectPos += tangentCorridorEntryPos - tangentCorridorExitPos;
        }

        Instantiate(
            secondCorridorDef.GameObject,
            newObjectPos - secondCorridorExitPos,
            secondCorridorDef.GameObject.transform.rotation,
            transform
        );

        newObjectPos += secondCorridorEntryPos - secondCorridorExitPos;

        return newObjectPos;
    }

    private Vector2 InstantiateCorridors(
        Vector2Int direction,
        Vector2Int currentGridPos,
        Vector2Int neighbourGridPos,
        Vector2 maxAreaForRoom,
        Dictionary<Vector2Int, GameObject> spawnedRooms,
        Dictionary<Vector2Int, RoomDefenition> selectedRooms
    ) {
        Rect desiredRoomRect = new(
            currentGridPos.x * maxAreaForRoom.x,
            currentGridPos.y * maxAreaForRoom.y,
            maxAreaForRoom.x,
            maxAreaForRoom.y
        );

        DoorDirection currentDoorDir = direction.GetDoorDirection();
        DoorDirection neighbourDoorDir = currentDoorDir.GetOpposite();

        Vector2 currentDoorPos = GetDoorPosition(selectedRooms[currentGridPos], currentDoorDir);
        Vector2 newObjectPos = InstantiateTurningCorridors(
            direction,
            currentGridPos,
            neighbourGridPos,
            maxAreaForRoom,
            spawnedRooms,
            selectedRooms
        );

        CorridorPattern corridorPattern = direction.x == 0 ?
                        CorridorPattern.Vertical : CorridorPattern.Horizontal;

        CorridorDefenition corridorDef = new(GetRandomCorridor(corridorPattern));

        Vector2 corridorEntryPos = corridorDef.GetEntry(neighbourDoorDir);
        Vector2 corridorExitPos = corridorDef.GetExit(neighbourDoorDir);

        for (int i = 0; ; ++i) {
            if (desiredRoomRect.Contains(newObjectPos)) {
                break;
            } else if (i == 32) {
                Debug.LogError("Infinite cycle of placing corridors");
                return newObjectPos - currentDoorPos;
            }

            Instantiate(
                corridorDef.GameObject,
                newObjectPos - corridorExitPos,
                corridorDef.GameObject.transform.rotation,
                transform
            );

            newObjectPos += corridorEntryPos - corridorExitPos;
        }

        return newObjectPos - currentDoorPos;
    }

    private void DrawRectGizmo(Rect rect, Color color) {
        Gizmos.color = color;
        Vector3 bottomLeft = new(rect.xMin, rect.yMin, 0);
        Vector3 topLeft = new(rect.xMin, rect.yMax, 0);
        Vector3 topRight = new(rect.xMax, rect.yMax, 0);
        Vector3 bottomRight = new(rect.xMax, rect.yMin, 0);

        Gizmos.DrawLine(bottomLeft, topLeft);
        Gizmos.DrawLine(topLeft, topRight);
        Gizmos.DrawLine(topRight, bottomRight);
        Gizmos.DrawLine(bottomRight, bottomLeft);
    }
}
