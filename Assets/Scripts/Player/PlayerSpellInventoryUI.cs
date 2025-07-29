using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSpellInventoryUI : MonoBehaviour {
    [Header("Settings")]
    [SerializeField] private Sprite _activeBackground;
    [SerializeField] private Sprite _inactiveBackground;

    [Header("Arrays")]
    [SerializeField]  private List<Image> _images = new();
    [SerializeField]  private List<Image> _backgrounds = new();

    private int _active = 0;

    public void SetActive(int index) {
        _backgrounds[_active].sprite = _inactiveBackground;
        _backgrounds[index].sprite = _activeBackground;

        _active = index;
    }

    public void SetItemSprite(int index, Sprite sprite) {
        _images[index].sprite = sprite;
        _images[index].gameObject.SetActive(true);
    }
}
