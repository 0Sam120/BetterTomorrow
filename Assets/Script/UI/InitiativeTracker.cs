using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InitiativeTracker : MonoBehaviour
{

    public TextMeshProUGUI combatRound;
    public GameObject initiativeParent;
    public GameObject initiativePrefab;
    public Sprite allyIndicator;
    public Sprite enemyIndicator;

    public static InitiativeTracker Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        combatRound.text = $"Combat Round: {TurnManager.Instance.combatRound}";
        UpdateInitiativeTracker(TurnManager.Instance.combatRound, TurnManager.Instance.ActiveUnits);
    }

    public void UpdateInitiativeTracker(int round, List<Character> charactersInOrder)
    {
        combatRound.text = "Round: " + round.ToString();
        // Clear existing initiative entries
        foreach (Transform child in initiativeParent.transform)
        {
            Destroy(child.gameObject);
        }

        charactersInOrder = GetDisplayOrder(charactersInOrder, TurnManager.Instance.currentInitiativeIndex);

        // Populate initiative tracker with current characters
        foreach (Character character in charactersInOrder)
        {
            if(character.team == Team.Player)
            {
                initiativePrefab.GetComponent<Image>().sprite = allyIndicator;
            }
            else
            {
                initiativePrefab.GetComponent<Image>().sprite = enemyIndicator;
            }

            GameObject entry = Instantiate(initiativePrefab, initiativeParent.transform);
            TextMeshProUGUI entryText = entry.GetComponentInChildren<TextMeshProUGUI>();
            if (entryText != null)
            {
                entryText.text = character.name;
            }
        }
    }

    private List<Character> GetDisplayOrder(List<Character> originalList, int currentIndex)
    {
        return originalList.Skip(currentIndex)
                      .Concat(originalList.Take(currentIndex)).Reverse<Character>()
                      .ToList();
    }
}
