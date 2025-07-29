using System;
using UnityEngine;

public class Health : MonoBehaviour {
    [SerializeField] protected int _startingHealth = 5;

    protected int _health;
    protected int _maxHealth;

    public event Action Died = delegate { };

    protected virtual void Start() {
        _health = _startingHealth;
        _maxHealth = _startingHealth;
    }

    public virtual void TakeDamage(int damage) {
        if (damage == 0) {
            return;
        }

        if (_health <= damage) {
            Die();
            return;
        }

        _health -= damage;
    }

    public virtual void Heal(int heal) {
        _health += heal;

        if (_health > _maxHealth) {
            _health = _maxHealth;
        }
    }

    public virtual void UpdateMaxHealth(int diff) {
        _maxHealth += diff;

        if (_maxHealth < 0) {
            _maxHealth = 1;
        }
    }

    protected virtual void Die() => Died.Invoke();
}
