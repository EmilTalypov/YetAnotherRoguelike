using System.Collections.Generic;
using System.Linq;
using UnityEngine.Tilemaps;
using UnityEngine;

public enum RoomType {
    Start,
    Regular,
    Corridor,
    End
}

public class RoomDefenition {
    public DoorContainer DoorContainer { get; }
    public GameObject GameObject { get; }
    public List<Tilemap> Tilemaps { get; }
    public Rect Bounds { get; }

    public RoomDefenition(RoomDefenition roomDefenition) {
        DoorContainer = roomDefenition.DoorContainer;
        GameObject = roomDefenition.GameObject;
        Tilemaps = roomDefenition.Tilemaps;
        Bounds = roomDefenition.Bounds;
    }

    public RoomDefenition(GameObject roomPrefab) {
        GameObject = roomPrefab;
        DoorContainer = roomPrefab.GetComponent<DoorContainer>();
        Tilemaps = roomPrefab.GetComponentsInChildren<Tilemap>().ToList();

        List<Vector2> usedTiles = new();

        foreach (Tilemap tilemap in Tilemaps) {
            List<Vector2> tileWorldPositions = new();
            BoundsInt bounds = tilemap.cellBounds;

            for (int x = bounds.xMin; x < bounds.xMax; ++x) {
                for (int y = bounds.yMin; y < bounds.yMax; ++y) {
                    if (tilemap.HasTile(new(x, y, 0))) {
                        usedTiles.Add(new(x, y));
                    }
                }
            }
        }

        Vector2 min = new(
            usedTiles.Min(tile => tile.x),
            usedTiles.Min(tile => tile.y)
        );

        Vector2 max = new(
            usedTiles.Max(tile => tile.x),
            usedTiles.Max(tile => tile.y)
        );

        Bounds = new Rect(min, max - min);
    }
}

public class CorridorDefenition : RoomDefenition {
    private readonly Vector2 _entryPos;
    private readonly Vector2 _exitPos;
    private readonly DoorDirection _entryDoorDirection;

    public CorridorDefenition(RoomDefenition roomDefenition) : base(roomDefenition) {
        IReadOnlyList<DoorDefenition> doors = DoorContainer.Doors;

        _entryDoorDirection = doors[0].direction;
        _entryPos = doors[0].connectionPosition;
        _exitPos = doors[1].connectionPosition;
    }

    public CorridorDefenition(GameObject roomPrefab) : base(roomPrefab) {
        IReadOnlyList<DoorDefenition> doors = DoorContainer.Doors;

        _entryDoorDirection = doors[0].direction;
        _entryPos = doors[0].connectionPosition;
        _exitPos = doors[1].connectionPosition;
    }

    public Vector2 GetEntry(DoorDirection entryDoorDirection) =>
        entryDoorDirection == _entryDoorDirection ? _entryPos : _exitPos;

    public Vector2 GetExit(DoorDirection entryDoorDirection) =>
        entryDoorDirection == _entryDoorDirection ? _exitPos : _entryPos;
}
