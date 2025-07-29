using UnityEngine;

public class Abyss : MonoBehaviour {
    [SerializeField] AbyssMaster _abyssMaster;

    private void OnTriggerEnter2D(Collider2D collision) {
        if (!collision.CompareTag("Player")) {
            return;
        }

        if (_abyssMaster == null) {
            Debug.LogError($"{gameObject.name} does not have abyss master");
            return;
        }

        _abyssMaster.RegisterHit();
    }

    private void OnTriggerStay2D(Collider2D collision) => OnTriggerEnter2D(collision);

    private void OnTriggerExit2D(Collider2D collision) {
        if (!collision.CompareTag("Player")) {
            return;
        }

        if (_abyssMaster == null) {
            Debug.LogError($"{gameObject.name} does not have abyss master");
            return;
        }

        _abyssMaster.Unregister();
    }
}
