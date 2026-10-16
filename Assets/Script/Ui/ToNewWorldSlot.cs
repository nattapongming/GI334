using UnityEngine;
using UnityEngine.EventSystems;

public class ToNewWorldSlot : MonoBehaviour, IDropHandler
{
    [SerializeField] private EuropeManager EUMgr;
    [SerializeField] private UiManager UImgr;

    void Awake()
    {
        EUMgr = EuropeManager.instance;
        UImgr = UiManager.instance;
    }

    public void OnDrop(PointerEventData eventData)
    {
        GameObject unitObj = eventData.pointerDrag;
        ShipDrag shipDrag = unitObj.GetComponent<ShipDrag>();
        if (shipDrag == null) 
            return;

        //EUMgr.CheckIfPassengerInEuropeReadyToBoardShip(shipDrag.Ship);
        EUMgr.AllowToGoToNewWorld(shipDrag.Ship);  // go to new world

        UImgr.DestoryOldUnitDrag();
        UImgr.SetUpUnitDragInEuropePort();

    }
}