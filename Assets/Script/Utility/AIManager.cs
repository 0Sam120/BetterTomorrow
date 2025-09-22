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
        bool isLowHealth = thisUnit.HP < thisUnit.maxHP * 0.3f;
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
            var enemiesInRange = enemies.Where(ed => ed.distance <= thisUnit.atkRange).ToList();
            if (enemiesInRange.Count > 0)
            {
                Debug.Log($"{thisUnit.name} is low HP but in cover, attacking from defensive position.");
                return AIState.Attacking;
            }
            else
            {
                // NEW: Even if no optimal targets, try repositioning for shots
                var enemiesInExtendedRange = enemies.Where(ed => ed.distance <= thisUnit.atkRange + 2).ToList();
                if (enemiesInExtendedRange.Count > 0)
                {
                    Debug.Log($"{thisUnit.name} is low HP in cover, repositioning for potential shots.");
                    return AIState.Moving;
                }

                Debug.Log($"{thisUnit.name} is low HP in cover but no targets accessible, staying put.");
                return AIState.Idle;
            }
        }

        if (enemies.Count > 0)
        {
            var nearest = enemies.OrderBy(ed => ed.distance).First().enemy.GetComponent<GridObject>().positionOnGrid;

            // Check for immediate threats
            if (IsInRange(currentPos, nearest, minRange) && CanIAct(thisUnit, 2))
            {
                Debug.Log("Enemy too close, tactical repositioning.");
                return AIState.Moving;
            }

            // Primary attack check - optimal range
            if (IsInRange(currentPos, nearest, optimalRange) && CanIAct(thisUnit, 2))
            {
                return AIState.Attacking;
            }

            // NEW: Secondary attack check - any enemy in max range
            var anyEnemyInMaxRange = enemies.Where(ed => ed.distance <= thisUnit.atkRange).ToList();
            if (anyEnemyInMaxRange.Count > 0 && CanIAct(thisUnit, 2))
            {
                // Check if it's worth taking a suboptimal shot
                if (ShouldTakeSuboptimalShot(anyEnemyInMaxRange))
                {
                    Debug.Log($"{thisUnit.name} taking opportunistic shot at max range.");
                    return AIState.Attacking;
                }
            }

            // Move to engage
            return AIState.Moving;
        }
        else if (CanIAct(thisUnit, 2))
        {
            Debug.Log("No enemies in range, moving to closest enemy.");
            return AIState.Moving;
        }

        return AIState.Idle;
    }

    // NEW: Helper method to determine if a suboptimal shot is worth taking
    private bool ShouldTakeSuboptimalShot(List<(Character enemy, int distance)> enemiesInRange)
    {
        // Take the shot if:
        // 1. Enemy is low on health (high chance to finish them)
        // 2. Enemy is out of cover (better hit chance)
        // 3. We're in good cover (safe to take the shot)
        // 4. No better positioning is easily available

        foreach (var enemyData in enemiesInRange)
        {
            Character enemy = enemyData.enemy;

            // High priority: Enemy is very low health
            if (enemy.HP < enemy.maxHP * 0.25f)
            {
                Debug.Log($"Taking shot at low-health {enemy.name}");
                return true;
            }

            // Medium priority: Enemy is out of cover and we're in cover
            if (!enemy.HasCoverAgainst(thisUnit.transform.position) && thisUnit.IsInCover())
            {
                Debug.Log($"Taking shot at exposed {enemy.name} from cover");
                return true;
            }

            // Low priority: We have nothing better to do and enemy is moderately damaged
            if (enemy.HP < enemy.maxHP * 0.6f)
            {
                // Check if we could easily get to a better position
                var walkableTiles = GetWalkableTilesInRange(thisUnit, (int)thisUnit.MaxMoveSpeed);
                bool betterPositionExists = walkableTiles.Any(tile =>
                {
                    int distanceToEnemy = ManhattanDistance(tile, enemy.GetComponent<GridObject>().positionOnGrid);
                    return distanceToEnemy <= optimalRange &&
                           GetCoverAtPosition(tile).Values.Any(cover => cover != CoverType.None);
                });

                if (!betterPositionExists)
                {
                    Debug.Log($"No better position available, taking available shot at {enemy.name}");
                    return true;
                }
            }
        }

        return false;
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
        StartCoroutine(DelayedUpdateAI());
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
            StartCoroutine(DelayedUpdateAI());
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

            // HIGH PRIORITY: Finishing off low-health enemies
            float healthPercent = (float)enemy.HP / enemy.maxHP;
            if (healthPercent < 0.25f)
                score += 20; // Very high priority for very low health
            else if (healthPercent < 0.5f)
                score += 12; // High priority for medium-low health
            else if (healthPercent < 0.75f)
                score += 6;  // Medium priority

            // MEDIUM PRIORITY: Prefer targets that are NOT in cover
            if (!enemy.HasCoverAgainst(thisUnit.transform.position))
            {
                score += OUT_OF_COVER_TARGET_BONUS;
                Debug.Log($"{enemy.name} is not in cover, bonus applied");
            }

            // RANGE OPTIMIZATION: Prefer enemies closer to optimal range
            int distanceFromOptimal = Mathf.Abs(enemyData.distance - optimalRange);
            score += (10 - distanceFromOptimal); // Better score for closer to optimal

            // TACTICAL CONSIDERATIONS: 

            // Prefer enemies that are isolated (can't get support)
            var nearbyAllies = GetAllEnemiesOnMap().Where(e =>
                e != enemy &&
                ManhattanDistance(e.GetComponent<GridObject>().positionOnGrid,
                                enemy.GetComponent<GridObject>().positionOnGrid) <= 3
            ).Count();

            if (nearbyAllies == 0)
                score += 5; // Bonus for isolated targets

            // Prefer enemies that pose a threat (are in range to hit us back)
            if (enemyData.distance <= enemy.atkRange)
                score += 3; // Slight bonus for neutralizing threats

            // OPPORTUNITY COST: If this enemy is at max range but not optimal, 
            // reduce score unless they're high-value targets
            if (enemyData.distance > optimalRange)
            {
                if (healthPercent > 0.5f && enemy.HasCoverAgainst(thisUnit.transform.position))
                {
                    score -= 8; // Penalty for suboptimal shots at healthy, covered enemies
                    Debug.Log($"Reducing score for suboptimal shot at healthy, covered {enemy.name}");
                }
            }

            // Small random factor to add unpredictability but not dominate decision
            score += UnityEngine.Random.Range(-2, 2);

            Debug.Log($"Target scoring: {enemy.name} = {score} (HP: {healthPercent:P}, Distance: {enemyData.distance}, Cover: {enemy.HasCoverAgainst(thisUnit.transform.position)})");

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
        int randomMod = UnityEngine.Random.Range(-3, 3); // Reduced randomness
        Vector2Int targetPos = targetUnit.GetComponent<GridObject>().positionOnGrid;
        Vector3 tileWorldPos = GridMap.Instance.GetWorldPosition(tilePos.x, tilePos.y);

        int distance = ManhattanDistance(tilePos, targetPos);
        bool isLowHealth = thisUnit.HP < thisUnit.maxHP * 0.2f;

        // More aggressive distance scoring
        if (pursuitMode)
        {
            // Heavily favor getting closer to enemies
            score += 20 - distance; // Big bonus for closer positions

            if (distance <= thisUnit.atkRange)
                score += 15; // Bonus for reaching attack range

            if (distance < minRange)
                score -= 10; // Reduced penalty (was -5)
        }
        else
        {
            if (distance < minRange)
                score -= 15; // Reduced from -20
            else if (distance <= thisUnit.atkRange)
                score += 15; // Big bonus for staying in attack range
            else
                score -= (distance - thisUnit.atkRange) * 3; // Penalty for being too far
        }

        // Cover scoring - but don't let it dominate
        var allEnemies = GetAllEnemiesOnMap();
        int coverBonus = 0;
        Dictionary<CoverDirection, CoverType> potentialCover = GetCoverAtPosition(tilePos);

        foreach (Character enemy in allEnemies)
        {
            Vector3 enemyPos = enemy.transform.position;
            Vector3 dirToEnemy = (enemyPos - tileWorldPos).normalized;
            CoverDirection coverDir = GridHelper.GetDirectionFromVector(dirToEnemy);

            if (potentialCover.TryGetValue(coverDir, out var coverType) && coverType != CoverType.None)
            {
                int coverValue = coverType == CoverType.Full ? 8 : 5; // Reduced from 30/15
                coverBonus += coverValue;
            }
        }

        // Only multiply cover bonus for very low health units
        if (isLowHealth)
            coverBonus = Mathf.RoundToInt(coverBonus * 1.5f); // Reduced from 2x multiplier

        score += coverBonus;

        // Aggressive bonus for positions that allow attacks on exposed enemies
        if (distance <= thisUnit.atkRange)
        {
            if (!targetUnit.HasCoverAgainst(tileWorldPos))
            {
                score += 12; // Increased from OUT_OF_COVER_TARGET_BONUS (10)
            }

            // Bonus for positions that can hit multiple enemies
            var enemiesInAttackRange = allEnemies.Where(e =>
                ManhattanDistance(tilePos, e.GetComponent<GridObject>().positionOnGrid) <= thisUnit.atkRange
            ).Count();

            if (enemiesInAttackRange > 1)
                score += enemiesInAttackRange * 5; // Multi-target bonus
        }

        // Flanking bonus - prefer positions that get behind enemies
        Vector2Int enemyToCurrentUnit = currentPos - targetPos;
        Vector2Int enemyToNewPos = tilePos - targetPos;

        // If we're moving to the opposite side of the enemy
        if (enemyToCurrentUnit.x * enemyToNewPos.x < 0 || enemyToCurrentUnit.y * enemyToNewPos.y < 0)
        {
            score += 8; // Flanking bonus
        }

        score += randomMod;
        return score;
    }

    private IEnumerator DelayedUpdateAI()
    {
        yield return new WaitForSeconds(UnityEngine.Random.Range(0.2f, 0.8f));
        UpdateAI();
    }

}