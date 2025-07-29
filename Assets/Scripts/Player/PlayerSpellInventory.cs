using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerSpellInventory : MonoBehaviour {
    [SerializeField] private int _maxSpellsCount = 3;
    [SerializeField] private List<Spell> _startingSpells = new();
    [SerializeField] private PlayerSpellInventoryUI _ui;

    private static readonly List<KeyCode> _spellKeys = new() { KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3 };

    private List<Spell> _spells;
    private PlayerMana _playerMana;
    private int _currentSpell = 0;

    public int MaxSpellsCount => _maxSpellsCount;
    public IReadOnlyList<Spell> Spells => _spells.AsReadOnly();

    private void Start() {
        _spells = _startingSpells
            .Select(prefab => Instantiate(prefab, Vector3.zero, Quaternion.identity, transform))
            .ToList();

        for (int i = 0; i < _spells.Count; ++i) {
            _ui.SetItemSprite(i, _spells[i].UiIcon);
        }

        _playerMana = GetComponent<PlayerMana>();
    }

    private void Update() {
        for (int i = 0; i < _spells.Count; ++i) {
            if (Input.GetKeyDown(_spellKeys[i]) && _spells[i] != null) {
                _currentSpell = i;
                _ui.SetActive(i);
            }
        }

        if (Input.GetMouseButtonDown(0)) {
            _playerMana.UseSpell(_spells[_currentSpell]);
        }
    }

    private void AddNewSpell(Spell spell, int index) {
        if (index < 0 || index >= _maxSpellsCount) {
            Debug.LogError($"Trying to pick spell {spell} into {index} slot, which is bad");
            return;
        }

        while (_spells.Count <= index) {
            _spells.Add(null);
        }

        _spells[index] = spell;
        _ui.SetItemSprite(index, spell.UiIcon);
    }
}
