using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum DoorDirection {
    Up = 0,
    Right = 1,
    Down = 2,
    Left = 3
}

public enum CorridorPattern {
    Vertical = 0b_0101,
    Horizontal = 0b_1010,
    TopLeft = 0b_1001,
    TopRight = 0b_0011,
    BottomLeft = 0b_1100,
    BottomRight = 0b_0110
}

[Serializable]
public class DoorDefenition {
    public Vector2 connectionPosition;  // position of connection betwenn rooms
    public DoorDirection direction;     // for easier connection between rooms on generation stage
    public List<GameObject> doors;
}

public class DoorContainer : MonoBehaviour {
    [SerializeField] private List<DoorDefenition> _doors = new();

    public IReadOnlyList<DoorDefenition> Doors => _doors.AsReadOnly();
    public int DoorsPattern {
        get {
            int connections = 0;

            foreach (DoorDefenition door in _doors) {
                connections |= 1 << (int)door.direction;
            }

            return connections;
        }
    }
    public event Action PlayerEnteredRoom = delegate { };

    public void InvokeEnter() => PlayerEnteredRoom.Invoke();

    public void OpenDoors() => StartCoroutine(OpeningCoroutine());

    private IEnumerator OpeningCoroutine() {
        Animator firstDoorAnimator = null;

        foreach (DoorDefenition doorDef in _doors) {
            foreach (GameObject door in doorDef.doors) {
                if (firstDoorAnimator == null) {
                    firstDoorAnimator = door.GetComponent<Animator>();
                }

                door.GetComponent<Animator>().SetBool("Open", true);
            }
        }

        yield return new WaitWhile(() => firstDoorAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1);
        yield return null;

        foreach (DoorDefenition doorDef in _doors) {
            foreach (GameObject door in doorDef.doors) {
                door.SetActive(false);
            }
        }
    }

    public void CloseDoors() {
        foreach (DoorDefenition doorDef in _doors) {
            foreach (GameObject door in doorDef.doors) {
                door.SetActive(true);
            }
        }
    }
}
