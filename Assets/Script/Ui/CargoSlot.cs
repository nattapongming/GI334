using UnityEngine;
using UnityEngine.EventSystems;

public class CargoSlot : MonoBehaviour, IDropHandler
{
    [SerializeField] private NavalUnit ship;

    [SerializeField] private int holdId;

    [SerializeField] private UiManager uiMgr;

    [SerializeField] private CargoDrag cargoDrag;
    public CargoDrag CargoDrag { get { return cargoDrag; } set { cargoDrag = value; } }

    [SerializeField] private GameManager gameMgr;

    [SerializeField] private EuropeManager EUMgr;

    private void Awake()
    {
        gameMgr = GameManager.instance;
        uiMgr = UiManager.instance;
        EUMgr = EuropeManager.instance;
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
        int loaded = stockDrag.Cargo.Quantity - cargoLeft;     // ← what really went on the ship

        if (uiMgr.InEurope)
        {
            uiMgr.UpdateCargoSlots(ship, uiMgr.CargoSLotEurope);
            // Buy from market
            gameMgr.PlayerFaction.Money -=
                loaded * EUMgr.EuropeStocks[stockDrag.Cargo.ProductID].AskPrice; // ← pay only for that
            uiMgr.UpdateMoneyEuropeText();
        }
        else uiMgr.UpdateCargoSlots(ship, uiMgr.CargoSlots);

        stockDrag.Cargo.Quantity = cargoLeft;
    }

}