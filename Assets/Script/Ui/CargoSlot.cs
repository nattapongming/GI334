using UnityEngine;
using UnityEngine.EventSystems;

public class CargoSlot : MonoBehaviour, IDropHandler
{
    [SerializeField] private NavalUnit ship;

    [SerializeField] private int holdId;

    [SerializeField] private UiManager uiMgr;

    private void Awake()
    {
        uiMgr = UiManager.instance;
    }

    public void SlotInit(NavalUnit ship, int holdId)
    {
        this.ship = ship;
        this.holdId = holdId;
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (ship.CargoList.Count > holdId) // there something in this hold
        {
            if (ship.CargoList[holdId].Quantity >= 100)
            {
                return;
            }
        }
        GameObject obj = eventData.pointerDrag;
        StockDrag stockDrag = obj.GetComponent<StockDrag>();

        if (stockDrag == null) return;

        int cargoLeft = ship.AddCargo(holdId, stockDrag.Cargo);
        stockDrag.Cargo.Quantity = cargoLeft;
    }
}