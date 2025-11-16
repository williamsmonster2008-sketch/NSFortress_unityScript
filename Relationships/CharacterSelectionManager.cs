using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 角色选中与信息展示控制器
/// </summary>
public class CharacterSelectionManager : MonoBehaviour
{
    [Header("引用")]
    public Camera mainCamera;
    public FamilySystem familySystem;
    public CharacterInfoPanel infoPanel;
    
    [Header("选择效果")]
    public LayerMask selectableLayer = ~0;
    public GameObject selectionIndicatorPrefab;
    public Vector3 indicatorOffset = new Vector3(0f, 0.1f, 0f);
    
    private NPCController currentSelection;
    private GameObject indicatorInstance;
    
    private void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }
    
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }
            
            TrySelectUnderCursor();
        }
    }
    
    private void TrySelectUnderCursor()
    {
        if (mainCamera == null)
        {
            return;
        }
        
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, selectableLayer))
        {
            NPCController controller = hit.collider.GetComponentInParent<NPCController>();
            if (controller != null)
            {
                Select(controller);
                return;
            }
        }
        
        ClearSelection();
    }
    
    private void Select(NPCController controller)
    {
        if (controller == null || controller.characterData == null)
        {
            return;
        }
        
        currentSelection = controller;
        UpdateIndicator();
        infoPanel?.ShowCharacter(controller.characterData, familySystem);
    }
    
    private void UpdateIndicator()
    {
        if (selectionIndicatorPrefab == null || currentSelection == null)
        {
            return;
        }
        
        if (indicatorInstance == null)
        {
            indicatorInstance = Instantiate(selectionIndicatorPrefab);
        }
        
        indicatorInstance.transform.SetParent(currentSelection.transform, false);
        indicatorInstance.transform.localPosition = indicatorOffset;
        indicatorInstance.SetActive(true);
    }
    
    private void ClearSelection()
    {
        currentSelection = null;
        if (indicatorInstance != null)
        {
            indicatorInstance.SetActive(false);
        }
        infoPanel?.ShowCharacter(null, familySystem);
    }
}
