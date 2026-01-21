using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class CharacterMenu : MonoBehaviour
{
    [SerializeField] private GameObject actionWindow;
    [SerializeField] private GameObject skillWindow;
    [SerializeField] private GameObject characterPortrait;
    [SerializeField] private GameObject actionButton;
    [SerializeField] private Slider hpBar;
    [SerializeField] private Slider apBar;
    [SerializeField] private TextMeshProUGUI hpValue;
    [SerializeField] private TextMeshProUGUI apValue;

    public static CharacterMenu instance { get; private set; }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            gameObject.SetActive(false); // Hide by default
        }
        else
        {
            Destroy(gameObject); // Ensure only one instance exists
        }
    }

    public void CharacterInfo()
    {
        ClearOldInfo();

        var activeCharacter = TurnManager.Instance.currentUnit;
        var weapon = activeCharacter.GetComponent<GearComponent>().weaponData;
        var skills = activeCharacter.GetComponent<SkillComponent>().knownSkills;

        UpdateBarValue(activeCharacter.HP, activeCharacter.maxHP, true);
        UpdateBarValue(activeCharacter.AP, activeCharacter.maxAP, false);

        characterPortrait.GetComponent<Image>().sprite = activeCharacter.GetComponent<Character>().Portrait;
        for(int i = 0; i < weapon.actions.Count; i++)
        {
            var button = Instantiate(actionButton, actionWindow.transform);
            button.GetComponent<Image>().sprite = weapon.actions[i].icon;

            if (weapon.actions[i].targetType == TargetType.Enemy)
            {
                button.GetComponent<Button>().onClick.AddListener(() => 
                {
                    SelectCharacter.Instance.AttackCommandSelected();
                });
            }
        }

        for (int i = 0; i < skills.Count; i++)
        {
            int index = i; // Capture index for the lambda
            var button = Instantiate(actionButton, skillWindow.transform);
            button.GetComponent<Image>().sprite = skills[i].icon;
            button.GetComponent<Button>().onClick.AddListener(() =>
            {
                SelectCharacter.Instance.AbilityCommandSelected(skills[index]);
            });
        }

        OpenMenu();
    }

    public void UpdateBarValue(float currentValue, float maxValue, bool hp)
    {
        if (hp)
        {
            hpBar.value = currentValue/maxValue;
            hpValue.text = $"{currentValue}/{maxValue}";
        }
        else
        {
            apBar.value = currentValue / maxValue;
            apValue.text = $"{currentValue}/{maxValue}";
        }
    }

    public void ClearOldInfo()
    {
        for(int i = 0; i < actionWindow.transform.childCount; i++)
        {
            Destroy(actionWindow.transform.GetChild(i).gameObject);
        }

        for(int i = 0; i < skillWindow.transform.childCount; i++)
        {
            Destroy(skillWindow.transform.GetChild(i).gameObject);
        }
    }

    public void OpenMenu()
    {
        gameObject.SetActive(true);
    }

    public void Hide() => gameObject.SetActive(false);
}
