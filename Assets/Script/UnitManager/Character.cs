using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.ReorderableList.Internal;
using UnityEngine;
using UnityEngine.UIElements;
using static TurnManager;

public enum Team
{
    Player,
    Enemy
}

public class Character : MonoBehaviour
{
    public string Name = "NaN";
    public Sprite Portrait;
    public float MaxMoveSpeed = 5f;
    public int maxHP = 100;
    public int HP;
    public int maxAP = 20;
    public int AP;
    public int AC = 14;
    public int atkRange = 5;
    public int atkMod = 4;
    public int DMG = 8;
    public int DMGMod = 2; // Damage modifier
    public int InitiativeMod = 2;
    public int Initiative;
    public int damageTaken;
    public Team team;
    private HealthBar healthBar;
    public static event System.Action<Character> OnCharacterDeath;

    public Vector2Int CurrentTile { get; private set; }
    public Dictionary<CoverDirection, CoverType> CurrentCover { get; private set; }

    void OnEnable() => UnitRegistry.Register(this);
    void OnDisable() => UnitRegistry.Deregister(this);

    public void Awake()
    {
        var pos = transform.position;
        UpdateTile(GridMap.Instance.GetGridPosition(pos));
        healthBar = GetComponentInChildren<HealthBar>();
        HP = maxHP; // Initialize HP to max at start
        AP = maxAP; // Initialize AP to max at start
        healthBar.SetName(Name);
        healthBar.UpdateHealthBar(HP, maxHP);
    }
    public bool IsAlive()
    {
        if (HP <= 0)
        {
            Debug.Log($"{Name} is dead");
            return false;
        }
        return true;
    }

    public int RollToHit()
    {
        int roll = Random.Range(1, 21); // Simulate a d20 roll
        int total = roll + atkMod; // Add attack modifier
        return total;
    }

    public void TakeDamage(int damage)
    {
        HP -= damage;
        if (HP <= 0)
        {
            Die();
        }
        else
        {
            healthBar.UpdateHealthBar(HP, maxHP);
        }
    }

    public void Heal(int amount)
    {
        HP += amount;
        if (HP > maxHP)
        {
            HP = maxHP; // Cap HP at maxHP
        }
        healthBar.UpdateHealthBar(HP, maxHP);
    }

    private void Die()
    {
        // Notify all systems BEFORE destroying
        OnCharacterDeath?.Invoke(this);

        // Play death animation/effects before destroying
        StartCoroutine(DeathSequence());
    }

    private IEnumerator DeathSequence()
    {

        var combatLog = FindAnyObjectByType<CombatLog>();

        combatLog.LogUnitDeath(Name);

        yield return new WaitForSeconds(1f);

        Destroy(gameObject);
    }

    public void UpdateTile(Vector2Int tilePos)
    {
        CurrentTile = tilePos;
        CurrentCover = GridMap.Instance.grid[tilePos.x, tilePos.y].coverData;
    }

    public bool IsInCover()
    {
        // quick check if any direction offers cover
        foreach (var kv in CurrentCover)
        {
            if (kv.Value != CoverType.None) return true;
        }
        return false;
    }

    public bool HasCoverAgainst(Vector3 attackerPos)
    {
        
        Vector3 dirToAttacker = (attackerPos - transform.position).normalized;
        //Debug.Log($"{Name} checking cover against attacker at {attackerPos}, direction: {dirToAttacker}");

        // figure out which grid direction is most aligned
        CoverDirection facing = GridHelper.GetDirectionFromVector(dirToAttacker);
        //Debug.Log($"{Name} facing direction: {facing}");

        if (CurrentCover.TryGetValue(facing, out var cover))
        {
            //Debug.Log($"{Name} has {cover} cover against attack from {attackerPos}");
            return cover != CoverType.None;
        }

        //Debug.Log($"{Name} has no cover against attack from {attackerPos}, AC remains {AC}");
        return false;
    }

    public int GetEffectiveAC(Vector3 attackerPos)
    {
        int effectiveAC = AC;

        Vector3 dirToAttacker = (attackerPos - transform.position).normalized;
        CoverDirection coverDir = GridHelper.GetDirectionFromVector(dirToAttacker);

        if (CurrentCover.TryGetValue(coverDir, out var cover))
        {
            if (cover == CoverType.Half)
            {
                effectiveAC += 2;
                Debug.Log($"{Name} has half cover against attack from {attackerPos}, increasing AC by 2");
            }
            else if (cover == CoverType.Full)
            {
                effectiveAC += 5;
                Debug.Log($"{Name} has full cover against attack from {attackerPos}, increasing AC by 5");
            }
            else
            {
                Debug.Log($"{Name} has no cover against attack from {attackerPos}, AC remains {AC}");
            }
        }
        else
        {
            Debug.Log($"{Name} has no cover against attack from {attackerPos}, AC remains {AC}");
        }

        return effectiveAC;
    }
}
