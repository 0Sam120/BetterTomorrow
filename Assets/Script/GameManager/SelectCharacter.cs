using UnityEngine;

public class SelectCharacter : MonoBehaviour
{
    // Component references
    CursorData cursorData;
    CommandMenu menu;
    CommandInput input;
    CharacterAttack characterAttack;
    CommandManager manager;

    public static SelectCharacter Instance { get; private set; }

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }

        cursorData = GetComponent<CursorData>();
        menu = GetComponent<CommandMenu>();
        input = GetComponent<CommandInput>();
        manager = new CommandManager();
        characterAttack = GetComponent<CharacterAttack>();
    }

    // Currently selected character
    public Character selected;

    // Character currently hovered over
    public Character hoverOverCharacter;

    // Grid object currently hovered over
    GridObject hoverOverGridObject;

    // Tracks the last grid position the cursor was on
    Vector2Int positionOnGrid = new Vector2Int(-1, -1);

    // Reference to the target grid map
    [SerializeField] GridMap targetGrid;

    private void Update()
    {
        // Check if cursor moved to a different grid position
        if (positionOnGrid != cursorData.positionOnGrid)
        {
            // Update current cursor position
            positionOnGrid = cursorData.positionOnGrid;

            // Get grid object at the new position
            hoverOverGridObject = targetGrid.GetPlacedObject(positionOnGrid);

            // Check if the grid object contains a Character component
            if (hoverOverGridObject != null)
            {
                hoverOverCharacter = hoverOverGridObject.GetComponent<Character>();
            }
            else
            {
                hoverOverCharacter = null;
            }
        }
    }

    // Called when Move command is selected from menu
    public void MoveCommandSelected()
    {
        input.SetCommandType(CommandType.MoveTo);
        input.InitCommand();
    }

    // Called when Attack command is selected from menu
    public void AttackCommandSelected()
    {
        input.SetCommandType(CommandType.Attack);
        input.InitCommand();
    }

    public void AbilityCommandSelected(SkillsScriptableObject skill)
    {
        if (!TurnManager.Instance.currentUnit.GetComponent<SkillComponent>().CanUseSkill(skill)) return;
        GetComponent<SkillResolution>().SkillTargeting(skill);
        input.SetCommandType(CommandType.UseAbility);
    }

    // Opens or closes the command menu based on selection state
    private void UpdateMenu()
    {
        //if (selected != null)
        //{
        //    menu.OpenPanel();
        //}
        //else
        //{
        //    menu.ClosePanel();
        //}
    }

    // Selects the character currently hovered over
    public void Select(Character hoverOverCharacter)
    {
        if (hoverOverCharacter == null) { return; }

        selected = hoverOverCharacter;
    }

    // Deselects the current character and clears highlights
    public void Deselect()
    {
        selected = null;
        UpdateMenu();
        input.SetCommandType(CommandType.Default);
        ClearUtility.Instance.FullClear();
    }

    public void PerformAction()
    {
        manager.ExecuteCommand();
    }

    public void CancelAction()
    {
        characterAttack.CancelAttack();
        CharacterMenu.instance.OpenMenu();
    }
}
