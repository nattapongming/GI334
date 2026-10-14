using UnityEngine;
using UnityEngine.EventSystems;

public class ToNewWorldSlot : MonoBehaviour, IDropHandler
{
    [SerializeField] private EuropeManager EUMgr;

    void Awake()
    {
        EUMgr = EuropeManager.instance;
    }

    public void OnDrop(PointerEventData eventData)
    {
        GameObject unitObj = eventData.pointerDrag;
        ShipDrag shipDrag = unitObj.GetComponent<ShipDrag>();
        if (shipDrag == null) 
            return;
        EUMgr.AllowToGoToNewWorld(shipDrag.Ship);  // go to new world

    }
}