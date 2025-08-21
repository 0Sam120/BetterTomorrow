using UnityEngine;
using UnityEngine.UI;

public class TargetIndicator : MonoBehaviour
{
    private GridObject _target;
    private CharacterAttack _attack;

    public void Initialize(GridObject target, CharacterAttack attack)
    {
        _target = target;
        _attack = attack;

        GetComponent<Button>().onClick.AddListener(() => _attack.SelectTarget(_target));
    }
}
