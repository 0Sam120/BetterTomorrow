using UnityEngine;
using UnityEngine.UI;

public class TargetIndicator : MonoBehaviour
{
    private GridObject target;
    private CharacterAttack attack;

    public void Initialize(GridObject target, CharacterAttack attack)
    {
        this.target = target;
        this.attack = attack;

        GetComponent<Button>().onClick.AddListener(() => attack.SelectTarget(target));
    }
}
