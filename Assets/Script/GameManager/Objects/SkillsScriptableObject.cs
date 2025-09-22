using UnityEngine;

public enum SkillType
{
    Damaging,
    Healing,
    Utility
}

public enum TargetType
{
    Self,
    Ally,
    Enemy,
    Area
}

[CreateAssetMenu(fileName = "SkillsScriptableObject", menuName = "Scriptable Objects/Skills")]
public class SkillsScriptableObject : ScriptableObject
{
    
    public string skillName;
    public Sprite icon;
    public string description;
    public int cooldown;
    public SkillType type;
    public int duration;
    public int range;
    public int areaOfEffect;

    public int cost;
    public TargetType targeting;
    public float effectPower;
    public bool canCrit;
    public bool ignoreCover;
    public bool friendlyFire;
    public bool limitedCharges;
    public int maxCharges;
    public bool requiresLOS;

}
