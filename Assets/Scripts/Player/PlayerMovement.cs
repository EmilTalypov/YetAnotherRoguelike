using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {
    [Header("Movement")]
    [SerializeField] private float _speed = 10f;
    [SerializeField] private float _dashSpeed = 50f;
    [SerializeField] private float _dashMovementDuration = 0.1f;
    [SerializeField] private float _dashCooldown = 1f;

    [Header("Misc")]
    [SerializeField] private float _dashIframes = 0.15f;
    [SerializeField] private PlayerHealth _health;

    private Rigidbody2D _rb;
    private Animator _animator;
    private PlayerEffects _effects;

    private Vector3 _inputMovement = Vector3.zero;
    private Vector3 _dashDirection = Vector3.zero;
    private Vector3 _lastSafePosition = Vector3.zero;
    private bool _isDashing = false;
    private bool _isDashAvailable = true;

    public bool IsDashing => _isDashing;

    private void Start() {
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _effects = GetComponent<PlayerEffects>();
    }

    private void Update() {
        _inputMovement.x = Input.GetAxisRaw("Horizontal");
        _inputMovement.y = Input.GetAxisRaw("Vertical");

        _animator.SetFloat("Horizontal_move", _inputMovement.x);
        _animator.SetFloat("Vertical_move", _inputMovement.y);

        _inputMovement.Normalize();

        if (_isDashAvailable && Input.GetMouseButtonDown(1)) {
            _dashDirection = new(_inputMovement.x, _inputMovement.y, 0);

            _isDashAvailable = false;
            _isDashing = true;

            StartCoroutine(DashMovementStopCoroutine());
            _health.SetIframes(_dashIframes, IframesSourceType.Dash);
        }
    }

    private void FixedUpdate() {
        if (!_health.IsInAbyss) {
            _lastSafePosition = transform.position;
        }

        if (_isDashing) {
            _rb.MovePosition(transform.position + _dashSpeed * Time.fixedDeltaTime * _dashDirection);
        } else {
            _rb.MovePosition(transform.position + _speed * Time.fixedDeltaTime * _inputMovement);
        }
    }

    private IEnumerator DashMovementStopCoroutine() {
        yield return new WaitForSeconds(_dashMovementDuration);

        _isDashing = false;
        StartCoroutine(DashCooldownCoroutine());
    }

    private IEnumerator DashCooldownCoroutine() {
        yield return new WaitForSeconds(_dashCooldown);

        _isDashAvailable = true;
        _effects.PlayDashRecovered();
    }

    public void TeleportToLastSavePosition() {
        transform.position = _lastSafePosition;
        _health.IsInAbyss = false;
    }
}
