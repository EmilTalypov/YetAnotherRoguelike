using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;


public enum IframesSourceType {
    Dash,
    Attack
}


public class PlayerHealth : Health {
    [SerializeField] private float _takingDamageIframes = 1.5f;
    [SerializeField] private PlayerHealthUI _ui;
    [SerializeField] private string _startMenuSceneName = "Start_menu";

    private PlayerEffects _effects;

    private float _damageReduction = 0;
    private float _defaultDamageReduction = 0;

    private readonly SortedDictionary<float, int> _currentDamageReductions = new();
    private bool _isInIframes = false;
    private IframesSourceType? _iframesSource;

    public new event Action Died = delegate { };
    public event Action<IframesSourceType> EnteredIframes = delegate { };
    public event Action ExitedIframes = delegate { };

    public float TakingDamageIframes => _takingDamageIframes;
    public bool IsInIframes => _isInIframes;
    public IframesSourceType? IframesSource => _iframesSource;
    public bool IsInAbyss { get; set; }

    protected override void Start() {
        base.Start();
        _effects = GetComponent<PlayerEffects>();
        _ui.UpdateCurrentHealth(_health);
    }

    public override void TakeDamage(int damage) {
        if (_isInIframes) {
            return;
        }

        damage = (int)(damage * (1 - _damageReduction));

        if (damage == 0) {
            return;
        }

        if (_health <= damage) {
            Die();
            return;
        }

        _health -= damage;
        SetIframes(_takingDamageIframes, IframesSourceType.Attack);

        _effects.PlayDamageFlashing();
        _ui.UpdateCurrentHealth(_health);
    }

    public override void Heal(int heal) {
        int prevHealth = _health;
        _health += heal;

        if (_health > _maxHealth) {
            _health = _maxHealth;
        }

        if (prevHealth != _health) {
            _ui.UpdateCurrentHealth(_health);
        }
    }

    public override void UpdateMaxHealth(int diff) {
        _maxHealth = Mathf.Max(1, _maxHealth + diff);
        _health = Mathf.Min(_health, _maxHealth);

        _ui.UpdateCurrentHealth(_health);
    }

    public void SetIframes(float duration, IframesSourceType iframesSource) =>
        StartCoroutine(ResetIframesCoroutine(duration, iframesSource));

    private IEnumerator ResetIframesCoroutine(float duration, IframesSourceType iframesSource) {
        EnteredIframes.Invoke(iframesSource);
        _isInIframes = true;
        _iframesSource = iframesSource;

        yield return new WaitForSeconds(duration);

        _isInIframes = false;
        _iframesSource = null;
        ExitedIframes.Invoke();
    }

    protected override void Die() {
        _ui.UpdateCurrentHealth(0);
        Died.Invoke();
        Destroy(gameObject);
        SceneManager.LoadScene(_startMenuSceneName);
    }



    public void SetDamageReductionWithPeriod(float damageReduction, float duration) =>
        StartCoroutine(ResetDamageReductionCoroutine(damageReduction, duration));

    public void SetDamageReductionConstant(float damageReduction) {
        _defaultDamageReduction = damageReduction;

        if (_defaultDamageReduction > _damageReduction) {
            _damageReduction = _defaultDamageReduction;
        }
    }

    private IEnumerator ResetDamageReductionCoroutine(float damageReduction, float duration) {
        if (!_currentDamageReductions.ContainsKey(damageReduction)) {
            _currentDamageReductions.Add(damageReduction, 0);
        }

        _currentDamageReductions[damageReduction] += 1;

        _damageReduction = _currentDamageReductions.Last().Key;

        yield return new WaitForSeconds(duration);

        _currentDamageReductions[damageReduction] -= 1;

        if (_currentDamageReductions[damageReduction] == 0) {
            _currentDamageReductions.Remove(damageReduction);
        }

        _damageReduction = _currentDamageReductions.Count > 0 ? _currentDamageReductions.Last().Key : _defaultDamageReduction;
    }
}
