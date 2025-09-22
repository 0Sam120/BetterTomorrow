using UnityEngine;

[CreateAssetMenu(fileName = "WeaponAction", menuName = "Scriptable Objects/WeaponAction")]
public class WeaponAction : ScriptableObject
{
    public string actionName = "New Action";
    public Sprite icon;
    public TargetType targetType = TargetType.Enemy;
    public int numAttacks;
    public float accuracyModifier;

}
