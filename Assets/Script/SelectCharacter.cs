using UnityEngine;

public class SelectCharacter : MonoBehaviour
{
    CursorData cursorData;
    CommandMenu menu;
    CommandInput input;
    ClearUtility clearUtility;

    private void Awake()
    {
        cursorData = GetComponent<CursorData>();
        menu = GetComponent<CommandMenu>();
        input = GetComponent<CommandInput>();
        clearUtility = GetComponent<ClearUtility>();
    }

    public Character selected;
    public Character hoverOverCharacter;
    GridObject hoverOverGridObject;
    Vector2Int positionOnGrid = new Vector2Int(-1, -1);
    [SerializeField] GridMap targetGrid;

    private void Update()
    {
        if(positionOnGrid != cursorData.positionOnGrid)
        {
            positionOnGrid = cursorData.positionOnGrid;
            hoverOverGridObject = targetGrid.GetPlacedObject(positionOnGrid);
            if(hoverOverGridObject != null )
            {
                hoverOverCharacter = hoverOverGridObject.GetComponent<Character>();
            }
            else
            {
                hoverOverCharacter = null;
            }
        }
    }

    public void MoveCommandSelected()
    {
        clearUtility.ClearGridHighlightAttack();
        input.SetCommandType(CommandType.MoveTo);
        input.InitCommand();
    }

    public void AttackCommandSelected()
    {
        clearUtility.ClearPathfinding();
        clearUtility.ClearGridHighlightMove();
        input.SetCommandType(CommandType.Attack);
        input.InitCommand();
    }

    private void UpdateMenu()
    {
        if(selected != null)
        {
            menu.OpenPanel();
        }
        else
        {
            menu.ClosePanel();
        }
    }

    public void Select()
    {
        if (hoverOverCharacter == null) { return; }
        selected = hoverOverCharacter.GetComponent<Character>();
        UpdateMenu();
    }
    public void Deselect()
    {
        selected = null;
        clearUtility.FullClear();
        UpdateMenu();
        input.SetCommandType(CommandType.Default);
    }

    

}
