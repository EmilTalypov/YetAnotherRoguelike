using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum EnemyType {
    //None,
    Easy,
    //Medium,
    //Hard,
    //Miniboss
}

public enum RoomWavesType {
    //None,
    Easy,
    //Medium,
    //Hard,
}

[System.Serializable]
public class EnemyDefenition {
    public GameObject gameObject;
    public float probability;
    public EnemyType type;
}

public static class EnemyWavesGenerator {
    private class EnemyTypeGroup {
        public List<EnemyDefenition> enemyDefs;
        public float[] prefWights;
        public float totalWeight;
    }

    private static Dictionary<EnemyType, EnemyTypeGroup> _enemiesByType;
    private static Dictionary<RoomWavesType, int> _maxWavesInRoom;
    private static Dictionary<EnemyType, int> _maxEnemiesOfType;

    private static EnemyDefenition GetRandomEnemyByWeight(EnemyTypeGroup typeGroup) {
        if (typeGroup.enemyDefs.Count == 1) {
            return typeGroup.enemyDefs[0];
        }

        float randomPoint = Random.Range(0f, typeGroup.totalWeight);

        int left = 0;
        int right = typeGroup.prefWights.Length;

        while (right - left > 1) {
            int mid = (left + right) / 2;

            if (typeGroup.prefWights[mid] < randomPoint) {
                left = mid;
            } else {
                right = mid;
            }
        }

        return typeGroup.enemyDefs[left];
    }

    private static EnemyWave GenerateRandomEnemyWave(int enemyTypesCount, GameObject room) {
        int randomEnemyTypesCount = Random.Range(1, enemyTypesCount + 1);

        List<GameObject> enemyPrefabs = new();

        for (int i = 0; i < randomEnemyTypesCount; ++i) {
            EnemyType randomEnemyType = (EnemyType)Random.Range(0, randomEnemyTypesCount);

            int desiredEnemiesCount = Random.Range(1, _maxEnemiesOfType[randomEnemyType]);

            for (int j = 0; j < desiredEnemiesCount; ++j) {
                enemyPrefabs.Add(GetRandomEnemyByWeight(_enemiesByType[randomEnemyType]).gameObject);
            }
        }

        GameObject waveGameObject = new() {
            name = $"Enemy_wave"
        };
        waveGameObject.transform.parent = room.transform;

        EnemyWave wave = waveGameObject.AddComponent<EnemyWave>();
        wave.Init(enemyPrefabs, room.transform.Find("Floor").GetComponent<Tilemap>());

        return wave;
    }

    public static void Generate(
        List<GameObject> spawnedRooms,
        List<EnemyDefenition> enemyDefs,
        List<RoomWavesDefenition> maxWavesInRoomList,
        List<EnemyTypeDefenition> maxEnemiesOfTypeList
    ) {
        InitializeDictionaries(enemyDefs, maxWavesInRoomList, maxEnemiesOfTypeList);

        int enemyTypesCount = System.Enum.GetValues(typeof(EnemyType)).Length;
        int roomDifficultiesCount = System.Enum.GetValues(typeof(RoomWavesType)).Length;

        foreach (GameObject room in spawnedRooms) {
            if (!room.CompareTag("RegularRoom")) {
                continue;
            }

            RoomWavesType randomRoomType = (RoomWavesType)Random.Range(0, roomDifficultiesCount);
            int numberOfWaves = Random.Range(1, _maxWavesInRoom[randomRoomType]);

            RoomWavesController roomWavesController = room.AddComponent<RoomWavesController>();

            List<EnemyWave> waves = new();

            for (int waveIndex = 0; waveIndex < numberOfWaves; ++waveIndex) {
                waves.Add(GenerateRandomEnemyWave(enemyTypesCount, room));
            }

            roomWavesController.Init(waves);
        }
    }

    private static void InitializeDictionaries(
       List<EnemyDefenition> enemyDefs,
       List<RoomWavesDefenition> maxWavesInRoomList,
       List<EnemyTypeDefenition> maxEnemiesOfTypeList
   ) {
        _enemiesByType = System.Enum.GetValues(typeof(EnemyType))
            .Cast<EnemyType>()
            .ToDictionary(
                type => type,
                type => new EnemyTypeGroup {
                    enemyDefs = enemyDefs
                        .Where(e => e.type == type)
                        .ToList(),
                    prefWights = null,
                    totalWeight = 0f
                }
            );

        foreach (EnemyTypeGroup group in _enemiesByType.Values) {
            if (group.enemyDefs.Count == 0) {
                continue;
            }

            group.totalWeight = group.enemyDefs.Sum(e => e.probability);
            group.prefWights = new float[group.enemyDefs.Count];

            float sum = 0;

            for (int i = 0; i < group.enemyDefs.Count; ++i) {
                sum += group.enemyDefs[i].probability;
                group.prefWights[i] = sum;
            }
        }

        _maxWavesInRoom = maxWavesInRoomList
            .GroupBy(wave => wave.type)
            .ToDictionary(g => g.Key, g => g.First().maxWavesCount);

        _maxEnemiesOfType = maxEnemiesOfTypeList
            .GroupBy(enemy => enemy.type)
            .ToDictionary(g => g.Key, g => g.First().maxEnemiesCount);
    }
}