using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class PrefabSelector {
    public static Dictionary<Vector2Int, RoomDefenition> Select(
        LevelGraphData levelGraphData,
        Dictionary<RoomType, List<RoomDefenition>> sourceRooms
    ) {
        List<Vector2Int> roomPositions = levelGraphData.Edges
            .SelectMany(pair => new Vector2Int[2] { pair.Item1, pair.Item2 })
            .Distinct()
            .ToList();

        Dictionary<(RoomType, int), List<RoomDefenition>> roomByDoors = new();

        foreach (var (roomType, roomsByType) in sourceRooms) {
            foreach (RoomDefenition room in roomsByType) {
                int doorsPattern = 0;

                foreach (DoorDefenition door in room.DoorContainer.Doors) {
                    doorsPattern |= 1 << (int)door.direction;
                }

                var key = (roomType, doorsPattern);

                if (!roomByDoors.TryGetValue(key, out List<RoomDefenition> rooms)) {
                    rooms = new List<RoomDefenition>();
                    roomByDoors.Add(key, rooms);
                }

                rooms.Add(room);
            }
        }

        Dictionary<Vector2Int, RoomDefenition> prefabs = new();

        HashSet<(Vector2Int, Vector2Int)> edgesHashset = levelGraphData.Edges.ToHashSet();

        foreach (var roomPosition in roomPositions) {
            int connections = GridGenerator.GetExistingConnections(roomPosition, edgesHashset);

            RoomType roomType = roomPosition == levelGraphData.StartingRoom ? RoomType.Start : (
                roomPosition == levelGraphData.EndingRoom ? RoomType.End : RoomType.Regular
            );

            int randomIndex = UnityEngine.Random.Range(0, roomByDoors[(roomType, connections)].Count);

            prefabs.Add(roomPosition, roomByDoors[(roomType, connections)][randomIndex]);
        }

        return prefabs;
    }
}
