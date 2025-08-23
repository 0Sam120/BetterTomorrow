using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Animations;
using static TurnManager;
using static UnityEngine.GraphicsBuffer;


public enum AIState
{
    Idle,
    Evaluate,
    Moving,
    Attacking,
    SeekingCover,
    Waiting,
    EndTurn
}

public enum AIType
{
    Aggressive,
    Defensive,
    Cowardly,
    Opportunistic,
    Support
}

public class AIManager : MonoBehaviour
{
    private Coroutine coroutine;
    CharacterTurn characterTurn;
    Character thisUnit;
    GridObject gridObj;
    AnimatorProxy proxy;
    Animator animator;
    CoverLogic coverLogic;
    Vector2Int currentPos;
    AIType currentType;
    AIState currentState;
    int optimalRange;
    int maxRange;
    int minRange;

    private const int COVER_BONUS_SCORE = 15;
    private const int OUT_OF_COVER_TARGET_BONUS = 10;
    private const int LOW_HEALTH_COVER_MULTIPLIER = 2;

    public void UpdateAI()
    {
        switch (currentState)
        {
            case AIState.Evaluate:
                currentState = WellnessCheck();
                UpdateAI(); // run again immediately with new state
                break;

            case AIState.SeekingCover:
                SeekCover();
                break;

            case AIState.Attacking:
                PerformAttack();
                break;

            case AIState.Moving:
                MoveToPosition();
                break;

            case AIState.Idle:
                currentState = AIState.EndTurn;
                break;

            case AIState.EndTurn:
                TurnManager.Instance.EndCurrentUnitTurn();
                break;
        }
    }


    public void HandleAITurn(Character unit)
    {
        if (TurnManager.Instance.state != GameState.EnemyTurn)
        {
            Debug.LogWarning("Tried to run AI turn outside of EnemyTurn phase!");
            return;
        }

        thisUnit = unit;
        currentState = AIState.Evaluate;
        characterTurn = thisUnit.GetComponent<CharacterTurn>();
        gridObj = thisUnit.GetComponent<GridObject>();
        animator = thisUnit.GetComponentInChildren<Animator>();
        proxy = new AnimatorProxy(animator, this);
        currentPos = gridObj.positionOnGrid;
        maxRange = thisUnit.atkRange;
        minRange = 1;
        optimalRange = Mathf.CeilToInt(maxRange / 2);
        UpdateAI();
    }

    private AIState WellnessCheck()
    {
        if (!CanIAct(thisUnit, 1))
            return AIState.Idle;

        var enemies = GetEnemiesWithDistance();
        bool isLowHealth = thisUnit.HP < thisUnit.maxHP * 0.3f; // Slightly higher threshold
        bool hasAnyCover = thisUnit.IsInCover();

        // If low health and not in cover, prioritize seeking cover
        if (isLowHealth && !hasAnyCover && CanIAct(thisUnit, 2))
        {
            Debug.Log($"{thisUnit.name} has low HP ({thisUnit.HP}/{thisUnit.maxHP}) and no cover, seeking cover.");
            return AIState.SeekingCover;
        }

        // If low health but in cover, be more defensive
        if (isLowHealth && hasAnyCover && CanIAct(thisUnit, 2))
        {
            // Check if we can attack from cover without moving
            var enemiesInRange = enemies.Where(ed => ed.distance <= thisUnit.atkRange).ToList();
            if (enemiesInRange.Count > 0)
            {
                Debug.Log($"{thisUnit.name} is low HP but in cover, attacking from defensive position.");
                return AIState.Attacking;
            }
            else
            {
                Debug.Log($"{thisUnit.name} is low HP in cover but no targets in range, staying put.");
                return AIState.Idle;
            }
        }

        if (enemies.Count > 0)
        {
            var nearest = enemies.OrderBy(ed => ed.distance).First().enemy.GetComponent<GridObject>().positionOnGrid;

            if (IsInRange(currentPos, nearest, minRange) && CanIAct(thisUnit, 2))
            {
                Debug.Log("Enemy too close, disengaging.");
                return AIState.Moving;
            }

            if (IsInRange(currentPos, nearest, optimalRange) && CanIAct(thisUnit, 2))
            {
                return AIState.Attacking;
            }
            else
            {
                return AIState.Moving;
            }
        }
        else if (CanIAct(thisUnit, 2))
        {
            Debug.Log("No enemies in range, moving to closest enemy.");
            return AIState.Moving;
        }

        return AIState.Idle;
    }


    private void SeekCover()
    {
        // Find the best cover position
        Vector2Int bestCoverPos = FindBestCoverPosition();

        if (bestCoverPos != currentPos)
        {
            var pathfinder = new Pathfinder(GridMap.Instance);
            var path = pathfinder.FindPath(currentPos, bestCoverPos);
            bool canSpend = CanIAct(thisUnit, 2);
            bool pathIsValid = thisUnit.GetComponent<UnitMovement>().PathIsValid(path);

            if (pathIsValid && canSpend)
            {
                characterTurn.SpendMomentum(2);
                thisUnit.GetComponent<UnitMovement>().Move(path);
                proxy.WaitUntilAnimationStops(() =>
                {
                    currentPos = thisUnit.GetComponent<GridObject>().positionOnGrid;
                    currentState = AIState.Evaluate;
                    StartCoroutine(DelayedUpdateAI());
                });
                return;
            }
        }

        Debug.Log($"{thisUnit.name} couldn't find suitable cover or path blocked, ending turn.");
        currentState = AIState.Idle;
    }

    private Vector2Int FindBestCoverPosition()
    {
        Vector2Int bestPos = currentPos;
        int bestScore = int.MinValue;

        List<Vector2Int> walkableTiles = GetWalkableTilesInRange(thisUnit, (int)thisUnit.MaxMoveSpeed);
        var enemies = GetAllEnemiesOnMap();

        foreach (Vector2Int tile in walkableTiles)
        {
            int coverScore = ScoreCoverPosition(tile, enemies);

            if (coverScore > bestScore)
            {
                bestScore = coverScore;
                bestPos = tile;
            }
        }

        return bestPos;
    }

    private int ScoreCoverPosition(Vector2Int position, List<Character> enemies)
    {
        int score = 0;

        // Use your cover calculation system to check cover at this position
        Dictionary<CoverDirection, CoverType> potentialCover = GetCoverAtPosition(position);

        foreach (Character enemy in enemies)
        {
            Vector3 enemyWorldPos = enemy.transform.position;
            Vector3 tileWorldPos = GridMap.Instance.GetWorldPosition(position.x, position.y);

            // Check if this position would provide cover against this enemy
            Vector3 dirToEnemy = (enemyWorldPos - tileWorldPos).normalized;
            CoverDirection coverDir = GridHelper.GetDirectionFromVector(dirToEnemy);

            if (potentialCover.TryGetValue(coverDir, out var coverType) && coverType != CoverType.None)
            {
                int coverValue = coverType == CoverType.Full ? COVER_BONUS_SCORE * 2 : COVER_BONUS_SCORE;
                score += coverValue;
                Debug.Log($"Position {position} would provide {coverType} cover against {enemy.name}");
            }
        }

        // Bonus for positions that aren't too far from enemies (we still want to be able to engage)
        if (enemies.Count > 0)
        {
            var closestEnemy = enemies.OrderBy(e =>
                ManhattanDistance(position, e.GetComponent<GridObject>().positionOnGrid)).First();
            int distanceToClosest = ManhattanDistance(position, closestEnemy.GetComponent<GridObject>().positionOnGrid);

            // Prefer positions within 2x our attack range but not too close
            if (distanceToClosest <= thisUnit.atkRange * 2 && distanceToClosest > minRange)
            {
                score += 5;
            }
        }

        return score;
    }

    private Dictionary<CoverDirection, CoverType> GetCoverAtPosition(Vector2Int position)
    {
        // Access pre-calculated cover data from the Node at this position
        Node tile = GridMap.Instance.grid[position.x, position.y];

        if (tile != null && tile.coverData != null)
        {
            return tile.coverData;
        }

        // Fallback if node doesn't exist or has no cover data
        Debug.LogWarning($"No cover data found for position {position}");
        return new Dictionary<CoverDirection, CoverType>
        {
            { CoverDirection.North, CoverType.None },
            { CoverDirection.South, CoverType.None },
            { CoverDirection.East, CoverType.None },
            { CoverDirection.West, CoverType.None }
        };
    }

    private void MoveToPosition()
    {
        var pathfinder = new Pathfinder(GridMap.Instance);
        Vector2Int targetPos = CalculateBestMovePosition(pathfinder);
        var path = pathfinder.FindPath(currentPos, targetPos);
        bool canSpend = CanIAct(thisUnit, 2);
        bool pathIsValid = thisUnit.GetComponent<UnitMovement>().PathIsValid(path);

        if (!pathIsValid || !canSpend)
        {
            Debug.Log("Cannot move to position, either path is invalid or not enough momentum.");
            return;
        }

        characterTurn.SpendMomentum(2);
        thisUnit.GetComponent<UnitMovement>().Move(path);
        proxy.WaitUntilAnimationStops(() =>
        {
            currentPos = thisUnit.GetComponent<GridObject>().positionOnGrid;
            currentState = AIState.Evaluate;
            StartCoroutine(DelayedUpdateAI());
        }
        );
    }

    private void PerformAttack()
    {
        var enemies = GetEnemiesWithDistance();
        var enemiesInRange = enemies.Where(ed => ed.distance <= thisUnit.atkRange).ToList();
        if (enemiesInRange.Count > 0 && CanIAct(thisUnit, 2))
        {
            // Choose the best target based on cover and other factors
            Character targetEnemy = ChooseBestTarget(enemiesInRange);

            characterTurn.SpendMomentum(2);
            int total = thisUnit.RollToHit();
            thisUnit.GetComponent<AttackComponent>().AttackPosition(targetEnemy.GetComponent<GridObject>(), total);
            proxy.WaitUntilAnimationStops(() =>
            {
                currentState = AIState.Evaluate;
                StartCoroutine(DelayedUpdateAI()); // Delay to allow for attack animation
            }
            );
        }
        else
        {
            Debug.Log("No enemies in range to attack.");
            currentState = AIState.Evaluate;
        }
    }

    private Character ChooseBestTarget(List<(Character enemy, int distance)> enemiesInRange)
    {
        if (enemiesInRange.Count == 1)
            return enemiesInRange[0].enemy;

        Character bestTarget = null;
        int bestScore = int.MinValue;

        foreach (var enemyData in enemiesInRange)
        {
            Character enemy = enemyData.enemy;
            int score = 0;

            // Prefer targets that are NOT in cover
            if (!enemy.HasCoverAgainst(thisUnit.transform.position))
            {
                score += OUT_OF_COVER_TARGET_BONUS;
                Debug.Log($"{enemy.name} is not in cover, bonus applied");
            }
            else
            {
                Debug.Log($"{enemy.name} is in cover, no bonus");
            }

            // Prefer closer targets (slight preference)
            score += (10 - enemyData.distance);

            // Prefer targets with lower health (finishing them off)
            float healthPercent = (float)enemy.HP / enemy.maxHP;
            if (healthPercent < 0.3f)
                score += 8; // High priority for low health enemies
            else if (healthPercent < 0.6f)
                score += 4; // Medium priority

            // Random factor to add some unpredictability
            score += UnityEngine.Random.Range(-3, 3);

            if (score > bestScore)
            {
                bestScore = score;
                bestTarget = enemy;
            }
        }

        return bestTarget ?? enemiesInRange[0].enemy;
    }

    Vector2Int CalculateBestMovePosition(Pathfinder pathfinder)
    {
        Vector2Int bestPos = currentPos;
        int bestScore = int.MinValue;

        List<Vector2Int> walkableTiles = GetWalkableTilesInRange(thisUnit, (int)thisUnit.MaxMoveSpeed);
        var enemies = GetEnemiesWithDistance();
        var inRange = enemies.Where(ed => ed.distance <= thisUnit.atkRange).ToList();

        if (enemies.Count == 0)
            return currentPos;

        Character targetEnemy = null;
        if (inRange.Count > 0)
        {
            // Choose best target from those in range
            targetEnemy = ChooseBestTarget(inRange);
        }
        else
        {
            // Move towards the best potential target
            var allEnemies = enemies.Select(ed => (ed.enemy, ed.distance)).ToList();
            targetEnemy = ChooseBestTarget(allEnemies);
        }

        Vector2Int enemyPos = targetEnemy.GetComponent<GridObject>().positionOnGrid;
        int distanceToEnemy = ManhattanDistance(currentPos, enemyPos);
        bool pursuitMode = distanceToEnemy > thisUnit.atkRange;

        foreach (Vector2Int tile in walkableTiles)
        {
            List<Vector2Int> path = pathfinder.FindPath(currentPos, tile);
            if (path == null || path.Count == 0) continue;

            int score = ScoreTile(tile, targetEnemy, pursuitMode);

            if (score > bestScore)
            {
                bestScore = score;
                bestPos = tile;
            }
        }

        return bestPos;
    }

    List<Vector2Int> GetWalkableTilesInRange(Character unit, int moveRange)
    {
        var targetGrid = GridMap.Instance;

        Vector2Int startPos = unit.GetComponent<GridObject>().positionOnGrid;
        List<Vector2Int> walkableTiles = new List<Vector2Int>();

        for (int x = -moveRange; x <= moveRange; x++)
        {
            for (int y = -moveRange; y <= moveRange; y++)
            {
                Vector2Int tilePos = new Vector2Int(startPos.x + x, startPos.y + y);

                if (!targetGrid.CheckBoundry(tilePos)) continue;
                if (!targetGrid.CheckWalkable(tilePos)) continue;

                int distance = Mathf.Abs(x) + Mathf.Abs(y);
                if (distance <= moveRange)
                {
                    walkableTiles.Add(tilePos);
                }
            }
        }

        return walkableTiles;
    }


    public List<Character> GetAllEnemiesOnMap()
    {
        List<Character> enemies = new List<Character>();

        foreach (Character otherUnit in UnitRegistry.AllUnits)
        {
            if (otherUnit.team != thisUnit.team && otherUnit != thisUnit)
            {
                Vector2Int unitPos = currentPos;
                Vector2Int targetPos = otherUnit.GetComponent<GridObject>().positionOnGrid;

                enemies.Add(otherUnit);
            }
        }

        return enemies;
    }

    List<(Character enemy, int distance)> GetEnemiesWithDistance()
    {
        var enemies = new List<(Character, int)>();
        foreach (var e in GetAllEnemiesOnMap())
        {
            int dist = ManhattanDistance(currentPos, e.GetComponent<GridObject>().positionOnGrid); // or pathfinding cost
            enemies.Add((e, dist));
        }
        return enemies;
    }


    int ManhattanDistance(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    public bool IsInRange(Vector2Int unitPos, Vector2Int targetPos, int range)
    {
        int distance = Mathf.Abs(unitPos.x - targetPos.x) + Mathf.Abs(unitPos.y - targetPos.y);
        return distance <= range;
    }

    internal bool CanIAct(Character unit, int momentumCost)
    {
        if (unit.GetComponent<CharacterTurn>().Momentum < momentumCost)
        {
            Debug.Log("Unit has no momentum to act.");
            return false;
        }

        return true;
    }

    private int ScoreTile(Vector2Int tilePos, Character targetUnit, bool pursuitMode = false)
    {
        int score = 0;
        int randomMod = UnityEngine.Random.Range(-5, 5);
        Vector2Int targetPos = targetUnit.GetComponent<GridObject>().positionOnGrid;
        Vector3 tileWorldPos = GridMap.Instance.GetWorldPosition(tilePos.x,tilePos.y);

        int distance = ManhattanDistance(tilePos, targetPos);

        // Base distance scoring
        if (pursuitMode)
        {
            score -= Mathf.Abs(distance - optimalRange);
            if (distance < minRange)
                score -= 5;
        }
        else
        {
            if (distance < minRange)
                score -= 20;
            else if (distance == optimalRange)
                score += 10;
            else
                score -= Mathf.Abs(distance - optimalRange);
        }

        // Cover considerations
        var allEnemies = GetAllEnemiesOnMap();
        int coverBonus = 0;

        // Get potential cover at this tile
        Dictionary<CoverDirection, CoverType> potentialCover = GetCoverAtPosition(tilePos);

        foreach (Character enemy in allEnemies)
        {
            Vector3 enemyPos = enemy.transform.position;
            Vector3 dirToEnemy = (enemyPos - tileWorldPos).normalized;
            CoverDirection coverDir = GridHelper.GetDirectionFromVector(dirToEnemy);

            if (potentialCover.TryGetValue(coverDir, out var coverType) && coverType != CoverType.None)
            {
                int coverValue = coverType == CoverType.Full ? COVER_BONUS_SCORE * 2 : COVER_BONUS_SCORE;
                coverBonus += coverValue;
            }
        }

        // Apply cover bonus, with multiplier for low health units
        bool isLowHealth = thisUnit.HP < thisUnit.maxHP * 0.3f;
        if (isLowHealth)
            coverBonus *= LOW_HEALTH_COVER_MULTIPLIER;

        score += coverBonus;

        // Prefer positions that give us good shots at enemies without cover
        if (distance <= thisUnit.atkRange && !targetUnit.HasCoverAgainst(tileWorldPos))
        {
            score += OUT_OF_COVER_TARGET_BONUS;
        }

        score += randomMod;
        return score;
    }

    private IEnumerator DelayedUpdateAI()
    {
        yield return new WaitForSeconds(UnityEngine.Random.Range(0.5f, 1.5f));
        UpdateAI();
    }

}