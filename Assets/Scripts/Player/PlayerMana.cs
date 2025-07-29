using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerMana : MonoBehaviour {
    [Header("Mana Settings")]
    [SerializeField] private int _startingMana = 10;
    [SerializeField] private float _startingManaRegeneration = 0.1f;
    [SerializeField] private float _manaRegenerationStep = 0.01f;
    [SerializeField] private float _maxManaRegeneration = 0.3f;

    [Header("Misc")]
    [SerializeField] private Dictionary<string, Spell> _spells = new();
    [SerializeField] private TextMeshProUGUI _manaText;

    private float _mana = 0;
    private int _maxMana;
    private PlayerHealth _health;
    private Coroutine _manaRegenerationCoroutine;

    private void Start() {
        _health = GetComponent<PlayerHealth>();
        _maxMana = _startingMana;
        UpdateUI();

        RoomWavesController.Entered += StartRegeneration;
        RoomWavesController.Ended += StopRegeneration;
        _health.EnteredIframes += StopRegenerationIframes;
        _health.ExitedIframes += StartRegeneration;
    }
    
    private void StopRegenerationIframes(IframesSourceType iframesSource) {
        if (iframesSource == IframesSourceType.Attack) {
            StopRegeneration();
        }
    }

    private void StopRegeneration() {
        if (_manaRegenerationCoroutine != null) {
            StopCoroutine(_manaRegenerationCoroutine);
            _manaRegenerationCoroutine = null;
        }
    }

    private void StartRegeneration() {
        if (_manaRegenerationCoroutine == null && RoomWavesController.IsPlayerInBattle) {
            _manaRegenerationCoroutine = StartCoroutine(RegenerationCoroutine(0f));
        }
    }

    public void UseSpell(Spell spell) {
        if (_mana < spell.ManaCost) {
            Debug.Log("Insufficient mana");
            return;
        }

        _mana -= spell.ManaCost;
        UpdateUI();
        spell.UseSpell();

        if (_manaRegenerationCoroutine != null) {
            StopCoroutine(_manaRegenerationCoroutine);
        }

        if (!_health.IsInIframes) {
            _manaRegenerationCoroutine = StartCoroutine(RegenerationCoroutine(spell.CastTime));
        }
    }

    private IEnumerator RegenerationCoroutine(float delay) {
        if (delay > 0) {
            yield return new WaitForSeconds(delay);
        }

        float currentRegeneration = _startingManaRegeneration;

        while (_mana < _maxMana) {
            yield return new WaitForSeconds(1f);

            _mana += currentRegeneration;

            if (_mana > _maxMana) {
                _mana = _maxMana;
            }

            UpdateUI();

            currentRegeneration += _manaRegenerationStep;

            if (currentRegeneration > _maxManaRegeneration) {
                currentRegeneration = _maxManaRegeneration;
            }
        }
    }

    public void UpdateMaxMana(int diff) {
        _maxMana += diff;

        if (_maxMana < 0) {
            _maxMana = 0;
        }
    }

    private void UpdateUI() {
        _manaText.text = $"Mana: {_mana}";
    }
}
