using UnityEngine;

public class CharacterTurn : MonoBehaviour
{
    public int Momentum; // Momentum for the character, used to track actions
    public int momentumGains;
    public int maxMomentumGained;


    public void GrantTurn()
    {
        Momentum = 4;
        maxMomentumGained = Momentum * 2;
        momentumGains = Momentum;
    }

    public bool CanSpendMomentum(int amount)
    {
        return Momentum >= amount;
    }

    public bool SpendMomentum(int amount)
    {
        if (amount <= Momentum)
        {
            Momentum -= amount;
            if (Momentum <= 0)
            {
                AutomaticTurnEnd();
            }
            return true;
            
        }
        else
        {
            Debug.LogWarning($"Not enough momentum to spend {amount}. Current momentum: {Momentum}");
            return false;
        }
    }

    private void LoseMomentum(int amount)
    {
        Momentum -= amount;
        if(Momentum - amount <= 0)
        {
            Momentum = 0;
            AutomaticTurnEnd();
        }
    }

    private void GainMomentum(int amount)
    {
        if(momentumGains <= maxMomentumGained)
        {
            Momentum += amount;
            momentumGains += amount;
            Debug.Log($"Gained {amount} momentum. Current momentum: {Momentum}");
        }
        else
        {
            Debug.LogWarning($"Cannot gain {amount} momentum. Max momentum is {maxMomentumGained}. Current momentum: {Momentum}");
        }
    }

    public int CheckMomentum(int damage, DamageTypes damageType, Character target)
    {
        
        var armourData = target.GetComponent<GearComponent>().armourData;
        int adjustedDamage;

        foreach(var weakness in armourData.weakness)
        {
            if (damageType == weakness)
            {
                adjustedDamage = ModifyDamage(damage, 2);
                GainMomentum(1);
                Debug.Log("You hit an enemy's weakness and gain momentum!");
                return adjustedDamage;
            }
        }

        foreach(var resistance in armourData.resistance)
        {
            if(damageType == resistance)
            {
                adjustedDamage = ModifyDamage(damage, 0.5f);
                LoseMomentum(2);
                Debug.Log("You lose momentum by hitting a resistance.");
                return adjustedDamage;
            }
        }
        
        foreach(var immunity in armourData.immunity)
        {
            if (damageType == immunity)
            {
                adjustedDamage = ModifyDamage(damage, 0);
                LoseMomentum(Momentum);
                Debug.Log("Hitting an immunity has cost you a turn");
                return adjustedDamage;
            }
        }

        return damage;
    }

    public void AutomaticTurnEnd()
    {
        TurnManager.Instance.EndCurrentUnitTurn();
        Debug.Log($"{gameObject.name} has automatically ended their turn.");
    }

    private int ModifyDamage(int damage, float modifier)
    {
        int adjustedDamage;
        adjustedDamage = (int)(damage * modifier);
        
        return adjustedDamage;
    }
}
