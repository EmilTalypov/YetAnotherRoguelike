using System.Collections;
using UnityEngine;

public class AbyssMaster : MonoBehaviour {
    [SerializeField] float _forgiveDuration = 0.25f;

    private bool _isHitting = false;
    private PlayerMovement _playerMovement;
    private PlayerHealth _playerHealth;

    public void InitPlayer(PlayerMovement playerMovement, PlayerHealth playerHealth) {
        _playerMovement = playerMovement;
        _playerHealth = playerHealth;
    }

    public void RegisterHit() {
        _playerHealth.IsInAbyss = true;

        if (_playerMovement.IsDashing || _isHitting) {
            return;
        }

        StartCoroutine(AbyssCoroutine());
    }

    private IEnumerator AbyssCoroutine() {
        _isHitting = true;

        yield return new WaitForSeconds(_forgiveDuration);

        _isHitting = false;
        _playerHealth.TakeDamage(1);
        _playerMovement.TeleportToLastSavePosition();
    }

    public void Unregister() {
        _playerHealth.IsInAbyss = false;
    }
}
