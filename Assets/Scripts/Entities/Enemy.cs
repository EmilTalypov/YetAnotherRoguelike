using UnityEngine;

public class Enemy : MonoBehaviour {
    [SerializeField] private int _contactDamage = 0;
    [SerializeField] private float _speed = 2f;
    [SerializeField] private float _checkValidGroundDistance = 3f;
    [SerializeField] private float _checkPlayerDistance = 20f;

    private Health _health;
    private Rigidbody2D _rb;
    private Transform _playerTransform;

    private void Start() {
        _playerTransform = GameObject.FindWithTag("Player").transform;

        _health = GetComponent<Health>();
        _rb = GetComponent<Rigidbody2D>();

        _health.Died += () => Destroy(gameObject);
    }

    private void FixedUpdate() {
        if (!_playerTransform) {
            return;
        }

        Vector3 difference = (_playerTransform.position - transform.position);

        if (difference.magnitude > _checkPlayerDistance) {
            return;
        }

        difference.Normalize();
        Vector3 nextPosition = transform.position + _speed * Time.fixedDeltaTime * difference;

        if (!Physics2D.Raycast(nextPosition, difference, _checkValidGroundDistance, LayerMask.GetMask("Abyss"))) {
            _rb.MovePosition(nextPosition);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        if (collision.collider.CompareTag("Player")) {
            collision.collider.GetComponent<PlayerHealth>().TakeDamage(_contactDamage);
        }
    }
}