using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour {
    [SerializeField] private List<Texture2D> _hpBarStates;
    [SerializeField] private Image _image;

    private int _currentHealth;

    public void UpdateCurrentHealth(int health) {
        _currentHealth = health;

        _image.sprite = Sprite.Create(_hpBarStates[_currentHealth], new(0, 0, 160, 32), new(0, 0));
    }
}
