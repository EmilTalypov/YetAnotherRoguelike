using UnityEngine;

public class BulletSpell : Spell {
    [SerializeField] private GameObject _bulletPrefab;

    private Transform _playerTransform;
    private Animator _playerAnimator;
    private Camera _camera;

    private void Start() {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        _playerTransform = player.transform;
        _playerAnimator = player.GetComponent<Animator>();
        _camera = FindAnyObjectByType<Camera>();
    }

    public override void UseSpell() {
        Vector2 mousePos = _camera.ScreenToWorldPoint(Input.mousePosition);

        Vector2 direction = (mousePos - (Vector2)_playerTransform.position).normalized;

        GameObject bullet = Instantiate(_bulletPrefab);
        bullet.transform.up = direction;
        bullet.transform.position = _playerTransform.position;
    }
}
