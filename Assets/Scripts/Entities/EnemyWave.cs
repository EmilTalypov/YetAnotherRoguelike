using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class EnemyWave : MonoBehaviour {
    private List<GameObject> _enemies;
    private int _diedCount = 0;

    public event Action Ended = delegate { };

    public void Init(List<GameObject> enemyPrefabs, Tilemap spawnArea) {
        _enemies = new(enemyPrefabs.Count);

        BoundsInt bounds = spawnArea.cellBounds;
        Vector3 cellSize = spawnArea.cellSize;
        List<Vector3> spawnTiles = new();

        for (int x = bounds.xMin; x < bounds.xMax; ++x) {
            for (int y = bounds.yMin; y < bounds.yMax; ++y) {
                if (spawnArea.HasTile(new(x, y, 0))) {
                    spawnTiles.Add(
                        spawnArea.CellToWorld(new(x, y, 0)) + cellSize / 2
                    );
                }
            }
        }

        spawnTiles.Shuffle();

        for (int i = 0; i < enemyPrefabs.Count; ++i) {
            GameObject enemy = Instantiate(
                enemyPrefabs[i],
                new(spawnTiles[i].x, spawnTiles[i].y, 0),
                Quaternion.identity,
                transform
            );

            enemy.SetActive(false);
            enemy.GetComponent<Health>().Died += HandleEnemyDeath;

            _enemies.Add(enemy);
        }
    }

    private void HandleEnemyDeath() {
        _diedCount += 1;

        if (_diedCount == _enemies.Count) {
            Ended.Invoke();
            gameObject.SetActive(false);
            _diedCount = 0;
        }
    }

    public void Spawn() {
        foreach (GameObject enemy in _enemies) {
            if (enemy) {
                enemy.SetActive(true);
            }
        }
    }
}
