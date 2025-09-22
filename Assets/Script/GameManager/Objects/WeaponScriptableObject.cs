using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "WeaponScriptableObject", menuName = "Scriptable Objects/WeaponScriptableObject")]
public class WeaponScriptableObject : ScriptableObject
{
    public string weaponName = "New Weapon";
    public bool isMelee = false;
    public int range = 5; // Range in tiles for ranged weapons
    public int damageDie = 6; // e.g., d6
    public int dieCount = 1; // Number of dice to roll
    public int penetration = 0; // Armour penetration value
    public List<WeaponAction> actions; // List of actions this weapon can perform

}
