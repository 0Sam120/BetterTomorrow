using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TargetingUI : MonoBehaviour
{
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button backButton;
    [SerializeField] private TextMeshProUGUI hitChanceText;
    [SerializeField] private TextMeshProUGUI expectedDamageText;
    [SerializeField] private TextMeshProUGUI critChanceText;
    [SerializeField] private Transform indicatorsParent;
    [SerializeField] private GameObject indicatorPrefab;

    [HideInInspector] public AttackComponent attackSystem;

    public static TargetingUI Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            gameObject.SetActive(false); // Hide by default
        }
        else
        {
            Destroy(gameObject); // Ensure only one instance exists
        }
    }

    public void ShowForTarget(GridObject target)
    {
        // Debug each component
        if (hitChanceText == null)
        {
            Debug.LogError("hitChanceText is null! Make sure it's assigned in the inspector.");
            return;
        }

        if (attackSystem == null)
        {
            Debug.LogError("attackSystem is null! Make sure it's assigned or initialized.");
            return;
        }

        if (target == null)
        {
            Debug.LogError("target parameter is null!");
            return;
        }

        // Update texts
        hitChanceText.text = $"Hit Chance: {attackSystem.CalculateHitChance(target)}%";
        expectedDamageText.text = $"Expected Damage: {attackSystem.CalculateExpectedDamage()}";
        critChanceText.text = $"Crit Chance: {attackSystem.CalculateCritChance()}%";
    }

    public void SpawnIndicators(List<GridObject> targets, CharacterAttack attack)
    {
        // clear old ones
        foreach (Transform child in indicatorsParent)
            Destroy(child.gameObject);

        // make a new indicator button for each target
        foreach (var target in targets)
        {
            GameObject indicator = Instantiate(indicatorPrefab, indicatorsParent);
            var script = indicator.GetComponent<TargetIndicator>();
            script.Initialize(target, attack);
        }

        gameObject.SetActive(true);
    }

    public void Hide() => gameObject.SetActive(false);
}
