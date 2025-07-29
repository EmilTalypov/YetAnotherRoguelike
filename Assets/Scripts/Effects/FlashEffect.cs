using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FlashEffect : MonoBehaviour {
    [ColorUsage(true, true)] [SerializeField] private Color _flashColor = Color.white;
    [SerializeField] private float _flashDuration = 0.25f;
    [SerializeField] private AnimationCurve _flashSpeedCurve;
    [SerializeField] private string _effectName;

    private List<SpriteRenderer> _sprites;
    private List<Material> _materials;

    public string Name => _effectName;

    private void Start() {
        _sprites = GetComponentsInChildren<SpriteRenderer>().ToList();

        _materials = new();

        for (int i = 0; i < _sprites.Count; ++i) {
            _materials.Add(_sprites[i].material);
        }
    }

    public void SetDuration(float duration) => _flashDuration = duration;

    public void Play() => StartCoroutine(EffectCoroutine());

    public IEnumerator EffectCoroutine() {
        SetFlashColor();

        float elapsedTime = 0f;

        while (elapsedTime < _flashDuration) {
            elapsedTime += Time.deltaTime;

            SetFlashAmount(_flashSpeedCurve.Evaluate(elapsedTime / _flashDuration));

            yield return null;
        }
    }

    private void SetFlashColor() {
        foreach (Material material in _materials) {
            material.SetColor("_FlashColor", _flashColor);
        }
    }

    private void SetFlashAmount(float amount) {
        foreach (Material material in _materials) {
            material.SetFloat("_FlashAmount", amount);
        }
    }
}
