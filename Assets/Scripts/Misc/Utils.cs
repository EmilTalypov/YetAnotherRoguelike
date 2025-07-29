using System.Collections.Generic;
using UnityEngine;

public static class DoorDirectionExtensions {
    public static DoorDirection GetOpposite(this DoorDirection direction) => (DoorDirection)((int)direction ^ 2);
}

public static class Vector2IntExtensions {
    public static DoorDirection GetDoorDirection(this Vector2Int direction) =>
        (direction.x, direction.y) switch {
            (1, 0) => DoorDirection.Right,
            (-1, 0) => DoorDirection.Left,
            (0, 1) => DoorDirection.Up,
            (0, -1) => DoorDirection.Down,
            _ => throw new System.ArgumentException("Direction can not be diagonal")
        };
}

public static class IListExtensions {
    public static void Shuffle<T>(this IList<T> list) {
        for (int i = 0; i < list.Count - 1; ++i) {
            int j = Random.Range(i, list.Count);
            (list[j], list[i]) = (list[i], list[j]);
        }
    }
}

public static class RectExtensions {
    public static bool Contains(this Rect rect, Rect other) =>
        rect.xMin <= other.xMin
        && other.xMax <= rect.xMax
        && rect.yMin <= other.yMin
        && other.yMax <= rect.yMax;
}
