using System.Collections;
using System.Linq;
using UnityEngine;

public class PlayerEffects : MonoBehaviour {
    [SerializeField] private int _damageFlashesCount = 2;
    [SerializeField] private string _damageFlashName = "DamageFlash";
    [SerializeField] private string _dashRecoveredName = "DashRecovered";

    private FlashEffect _damageFlash;
    private FlashEffect _dashRecovered;

    private float _damageFlashingDuration;

    private void Start() {
        _damageFlashingDuration = GetComponent<PlayerHealth>().TakingDamageIframes;

        FlashEffect[] flashEffects = GetComponents<FlashEffect>();

        _damageFlash = flashEffects.Where(e => e.Name == _damageFlashName).First();
        _dashRecovered = flashEffects.Where(e => e.Name == _dashRecoveredName).First();

        _damageFlash.SetDuration(_damageFlashingDuration / _damageFlashesCount);
    }

    public void PlayDamageFlashing() => StartCoroutine(DamageFlashCoroutine());

    public void PlayDashRecovered() => _dashRecovered.Play();

    private IEnumerator DamageFlashCoroutine() {
        for (int i = 0; i < _damageFlashesCount; ++i) {
            yield return _damageFlash.EffectCoroutine();
        }
    }
}
