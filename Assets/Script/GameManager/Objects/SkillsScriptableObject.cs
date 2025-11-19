using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public enum SkillType
{
    Damaging,
    Healing,
    Utility
}

public enum RollType
{
    None,
    Attack,
    Save
}

public enum TargetType
{
    None,
    Self,
    Ally,
    Enemy,
    Area,
    Circle,
    Line,
    Cone
}

[CreateAssetMenu(fileName = "SkillsScriptableObject", menuName = "Scriptable Objects/Skills")]
public class SkillsScriptableObject : ScriptableObject
{
    
    public string skillName;
    public Sprite icon;
    public string description;
    public int cooldown;
    public SkillType type;
    public RollType rollType;
    public int duration;
    public int range;
    public int areaOfEffect;

    public List<SkillEffect> effects;

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
