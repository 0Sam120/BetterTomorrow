using UnityEngine;

public enum ArmourType
{
    Clothing,
    Light,
    Medium,
    Heavy
}

[CreateAssetMenu(fileName = "ArmourScriptableObject", menuName = "Scriptable Objects/Armour")]
public class ArmourScriptableObject : ScriptableObject
{
    public string armourName = "New Armour";
    public ArmourType armourType = ArmourType.Clothing;
    public int baseAC = 10;

}
