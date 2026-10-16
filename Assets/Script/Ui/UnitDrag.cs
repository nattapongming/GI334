using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class UnitDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    [SerializeField] private Image unitImage;

    [SerializeField] private LandUnit landUnit;
    public LandUnit LandUnit { get { return landUnit; } set { landUnit = value; } }

    [SerializeField] private Transform iconParent;
    public Transform IconParent { get { return iconParent; } set { iconParent = value; } }

    [SerializeField] private TerrainSlot terrainSlot;

    [SerializeField] private Image statusIcon;

    [SerializeField] private UiManager uiMgr;

    void Awake()
    {
        uiMgr = UiManager.instance;
    }

    public void UnitInit(LandUnit landUnit)
    {
        unitImage.sprite = landUnit.UnitSprite.sprite;
        this.landUnit = landUnit;
    }

    public void QuitOldTerrainSlot()
    {
        if (terrainSlot != null)
        {
            terrainSlot.Hex.Labor = null;
            terrainSlot.ReduceTownYield();
            terrainSlot.RemoveYieldIcons();
            terrainSlot.Hex.YieldID = -1; // no yield for this hex
            terrainSlot = null;
        }
    }

    public void WorkAtNewTerrainSlot(TerrainSlot terrainSlot)
    {
        this.terrainSlot = terrainSlot;
    }

    public void CheckStatusIcon()
    {
        if (statusIcon == null) return;

        switch (landUnit.UnitStatus)
        {
            case UnitStatus.ToBoard:
                statusIcon.gameObject.SetActive(true);
                statusIcon.sprite = landUnit.StatusSpritesList[2];
                break;
            default:
                statusIcon.gameObject.SetActive(false);
                break;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (uiMgr.InEurope) return;

        iconParent = transform.parent;
        transform.SetParent(transform.root);
        transform.SetAsFirstSibling();
        unitImage.raycastTarget = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (uiMgr.InEurope) return;

        transform.position = Mouse.current.position.value;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (uiMgr.InEurope) return;

        transform.SetParent(iconParent);
        unitImage.raycastTarget = true;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (landUnit.UnitStatus == UnitStatus.None 
        || landUnit.UnitStatus == UnitStatus.OnBoard
        || landUnit.UnitStatus == UnitStatus.ToBoard
        || landUnit.UnitStatus == UnitStatus.Hidden)
        {
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                uiMgr.CurLandUnit = landUnit;
                uiMgr.CurUnitIcon = this;
                uiMgr.ToggleOrderPanel(true);
            }
        }
    }
}