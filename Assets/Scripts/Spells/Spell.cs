using UnityEngine;

public class Spell : MonoBehaviour {
    [SerializeField] protected int _manaCost = 1;
    [SerializeField] protected int _maxUpgradesCount = 0;
    [SerializeField] protected float _castTime = 0;
    [SerializeField] protected Sprite _uiIcon;

    protected int _upgradesCount = 0;

    public int ManaCost => _manaCost;
    public Sprite UiIcon => _uiIcon;
    public float CastTime => _castTime;

    public virtual void UseSpell() { }

    public virtual void Upgrade() {
        if (_upgradesCount < _maxUpgradesCount) {
            _upgradesCount++;
        }
    }
}
