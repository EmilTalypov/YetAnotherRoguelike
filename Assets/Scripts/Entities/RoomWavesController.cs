using System;
using System.Collections.Generic;
using UnityEngine;

public class RoomWavesController : MonoBehaviour {
    private List<EnemyWave> _waves;
    private DoorContainer _doorContainer;

    private int _current = 0;

    public static event Action Entered = delegate { };
    public static event Action Ended = delegate { };
    public static bool IsPlayerInBattle = false;

    public void Init(List<EnemyWave> waves) {
        _doorContainer = GetComponent<DoorContainer>();
        _waves = waves;

        _doorContainer.PlayerEnteredRoom += StartRoomWaves;
    }

    private void StartRoomWaves() {
        if (_current != 0) {
            return;
        }

        IsPlayerInBattle = true;
        Entered.Invoke();

        _waves[0].Ended += HandleWaveEnd;
        _waves[0].Spawn();
        _doorContainer.CloseDoors();
    }

    private void HandleWaveEnd() {
        _waves[_current].Ended -= HandleWaveEnd;
        _current += 1;

        if (_current == _waves.Count) {
            _doorContainer.OpenDoors();
            Ended.Invoke();
            IsPlayerInBattle = false;
            return;
        }

        _waves[_current].Ended += HandleWaveEnd;
        _waves[_current].Spawn();
    }
}
