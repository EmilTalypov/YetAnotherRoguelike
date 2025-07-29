using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LevelGraphData {
    public Vector2Int StartingRoom { get; set; }
    public Vector2Int EndingRoom { get; set; }
    public List<(Vector2Int, Vector2Int)> Edges { get; set; }
}

public static class GridGenerator {
    public readonly static List<Vector2Int> Directions = new() {
        new Vector2Int(0, 1),   // Up
        new Vector2Int(1, 0),   // Right
        new Vector2Int(0, -1),  // Down
        new Vector2Int(-1, 0)   // Left
    };

    public static List<Vector2Int> ShuffledDirections {
        get {
            List<Vector2Int> shuffled = Directions.Select(d => d).ToList();
            shuffled.Shuffle();

            return shuffled;
        }
    }

    public static int GetExistingConnections(Vector2Int room, HashSet<(Vector2Int, Vector2Int)> edges) {
        int existingConnections = 0;

        for (int i = 0; i < 4; ++i) {
            Vector2Int neighbour = room + Directions[i];

            if (edges.Contains((room, neighbour)) || edges.Contains((neighbour, room))) {
                existingConnections |= 1 << i;
            }
        }

        return existingConnections;
    }

    private static bool GenerateRandomGrid(
        Vector2Int current,
        int numberOfRooms,
        HashSet<int> validConnectionPatterns,
        HashSet<Vector2Int> generatedRooms,
        HashSet<(Vector2Int, Vector2Int)> edges
    ) {
        if (numberOfRooms == generatedRooms.Count) {
            return true;
        }

        int existingConnections = GetExistingConnections(current, edges);

        List<Vector2Int> shuffledDirections = ShuffledDirections;

        foreach (Vector2Int directionVector in shuffledDirections) {
            Vector2Int neighbour = current + directionVector;

            int direction = Directions.IndexOf(directionVector);

            int newConnections = existingConnections | (1 << direction);
            int oppositeDoor = 1 << (direction ^ 2);

            if (!generatedRooms.Contains(neighbour)
                && validConnectionPatterns.Contains(newConnections)
                && validConnectionPatterns.Contains(oppositeDoor)
            ) {
                generatedRooms.Add(neighbour);
                edges.Add((current, neighbour));

                if (GenerateRandomGrid(neighbour, numberOfRooms, validConnectionPatterns, generatedRooms, edges)) {
                    return true;
                }
            }
        }

        return numberOfRooms == generatedRooms.Count;
    }

    private static Vector2Int FindEndingRoom(
        Vector2Int startingRoom,
        HashSet<Vector2Int> rooms,
        HashSet<(Vector2Int, Vector2Int)> edges
    ) {
        Queue<Vector2Int> queue = new();
        Dictionary<Vector2Int, int> distance = new();

        queue.Enqueue(startingRoom);
        distance.Add(startingRoom, 0);

        Vector2Int endingRoom = startingRoom;
        int maxDistance = 0;

        while (queue.Count > 0) {
            Vector2Int current = queue.Dequeue();
            int currentConnections = GetExistingConnections(current, edges);

            // second is check if currentConnections has 1 bit
            if (distance[current] > maxDistance && (currentConnections & (currentConnections - 1)) == 0) {
                maxDistance = distance[current];
                endingRoom = current;
            }

            foreach (Vector2Int direction in Directions) {
                Vector2Int neighbor = current + direction;

                if (!rooms.Contains(neighbor) || distance.ContainsKey(neighbor)) {
                    continue;
                }

                if (!edges.Contains((current, neighbor)) && !edges.Contains((neighbor, current))) {
                    continue;
                }

                distance[neighbor] = distance[current] + 1;
                queue.Enqueue(neighbor);
            }
        }

        return endingRoom;
    }

    public static LevelGraphData Generate(int numberOfRooms, int extraEdges, HashSet<int> validConnectionPatterns) {
        int numberOfTries = 10;

        HashSet<Vector2Int> generatedRooms = new() { Vector2Int.zero };
        HashSet<(Vector2Int, Vector2Int)> edges = new();
        
        for (int i = 0; i < numberOfTries; ++i) {
            Vector2Int randomRoom = generatedRooms.ElementAt(Random.Range(0, generatedRooms.Count));

            if (GenerateRandomGrid(randomRoom, numberOfRooms, validConnectionPatterns, generatedRooms, edges)) {
                break;
            }
        }

        Vector2Int startingRoom = Vector2Int.zero;
        Vector2Int endingRoom = FindEndingRoom(startingRoom, generatedRooms, edges);
        List<Vector2Int> roomPositions = generatedRooms.ToList();

        for (int i = 0; i < extraEdges; ++i) {
            for (int attempt = 0; attempt < numberOfTries; ++attempt) {
                Vector2Int roomA = roomPositions[Random.Range(0, roomPositions.Count)];

                int directionIndex = Random.Range(0, 4);
                Vector2Int direction = Directions[directionIndex];
                
                Vector2Int roomB = roomA + direction;

                if (roomA == startingRoom
                    || roomA == endingRoom
                    || roomB == startingRoom
                    || roomB == endingRoom
                ) {
                    continue;
                }

                if (!generatedRooms.Contains(roomB)) {
                    continue;
                }

                if (edges.Contains((roomA, roomB)) || edges.Contains((roomB, roomA))) {
                    continue;
                }

                int oppositeDirection = (directionIndex + 2) % 4;
                int roomAConnections = GetExistingConnections(roomA, edges) + (1 << directionIndex);
                int roomBConnections = GetExistingConnections(roomB, edges) + (1 << oppositeDirection);

                if (validConnectionPatterns.Contains(roomAConnections) && validConnectionPatterns.Contains(roomBConnections)) {
                    edges.Add((roomA, roomB));
                    break;
                }
            }
        }

        return new LevelGraphData {
            StartingRoom = startingRoom,
            EndingRoom = endingRoom,
            Edges = edges.ToList()
        };
    }
}
