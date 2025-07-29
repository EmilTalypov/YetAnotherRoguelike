using UnityEngine;

public class Bullet : MonoBehaviour {
    [SerializeField] private float _speed = 1.0f;
    [SerializeField] private int _damage = 2;

    private Rigidbody2D _rb;

    private void Start() {
        _rb = GetComponent<Rigidbody2D>();
        _rb.linearVelocity = _speed * transform.up;
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        if (collision.collider.TryGetComponent(out Health otherHealth)) {
            otherHealth.TakeDamage(_damage);
        }

        Destroy(gameObject);
    }
}
