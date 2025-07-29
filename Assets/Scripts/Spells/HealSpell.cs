using System.Collections.Generic;
using UnityEngine;

public class HealSpell : Spell {
    [SerializeField] private List<int> _healAmounts = new();

    private PlayerHealth _playerHealth;
    private Animator _playerAnimator;

    private void Start() {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        _playerHealth = player.GetComponent<PlayerHealth>();  
        _playerAnimator = player.GetComponent<Animator>();
    }

    public override void UseSpell() {
        _playerHealth.Heal(_healAmounts[_upgradesCount]);
    }
}
