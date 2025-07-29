using UnityEngine;

public class DoorTriggerArea : MonoBehaviour {
    [SerializeField] private DoorContainer _doorContainer;

    private void OnTriggerExit2D(Collider2D collision) {
        if (collision.CompareTag("Player")) {
            if (!_doorContainer) {
                Debug.LogError($"{gameObject.transform.parent.name}/{gameObject.name} lacks doorContainer");
                return;
            }

            _doorContainer.InvokeEnter();
        }
    }
}