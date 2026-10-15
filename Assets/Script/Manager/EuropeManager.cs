using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EuropeManager : MonoBehaviour
{
    private const int EUROPE_DISTANCE = 3;

    // Ships To Europe
    [SerializeField] private List<ShipInTransit> shipsToEurope = new List<ShipInTransit>();
    public List<ShipInTransit> ShipsToEurope { get { return shipsToEurope; } set { shipsToEurope = value; } }

    // Ships From Europe
    [SerializeField] private List<ShipInTransit> shipsFromEurope = new List<ShipInTransit>();
    public List<ShipInTransit> ShipsFromEurope { get { return shipsFromEurope; } set { shipsFromEurope = value; } }

    [SerializeField] private List<NavalUnit> shipsInEurope = new List<NavalUnit>();
    public List<NavalUnit> ShipsInEurope { get { return shipsInEurope; } set { shipsInEurope = value; } }

    [SerializeField] private EUStock[] europeStocks = new EUStock[16];
    public EUStock[] EuropeStocks { get { return europeStocks; } set { europeStocks = value; } }

    [SerializeField] private NavalUnit curShip;
    public NavalUnit CurShip { get { return curShip; } set { curShip = value; } }

    [SerializeField] private List<LandUnit> landUnitsInEurope; // people waiting at the port
    public List<LandUnit> LandUnitsInEurope { get { return landUnitsInEurope; } set { landUnitsInEurope = value; } }

    public static EuropeManager instance;

    void Awake()
    {
        instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InitStartPrice();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void InitStartPrice()
    {
        GameManager gameMgr = GameManager.instance;

        for (int i = 0; i < europeStocks.Length; i++)
        {
            europeStocks[i] =
                new EUStock(i,
                gameMgr.ProductDatas[i].startBit,
                gameMgr.ProductDatas[i].startAsk,
                500);
        }
    }

    public void QuestionGoToEurope(NavalUnit ship)
    {
        curShip = ship;
        DialogManager.instance.GoToEuropeQuestion();
    }

    public void AllowToGoToEurope()
    {
        GameManager gameMgr = GameManager.instance;

        ShipInTransit shipInTransit = new ShipInTransit(curShip, EUROPE_DISTANCE);
        shipsToEurope.Add(shipInTransit);

        gameMgr.LeaveSeenFogAroundUnit(curShip);

        // passengers leave the map with the ship  ← new
        foreach (LandUnit passenger in curShip.Passengers)
        {
            if (passenger.CurHex != null)
                passenger.CurHex.UnitsInHex.Remove(passenger);
            passenger.CurHex = null;
        }


        curShip.CurHex.UnitsInHex.Remove(curShip);
        curShip.CurHex = null;
        CurShip.gameObject.SetActive(false);
    }

    public void UpdateShipInTransitTurn()
    {
        // To Europe
        List<ShipInTransit> shipsReachEurope = new List<ShipInTransit>();

        foreach (ShipInTransit shipsInTransit in shipsToEurope)
        {
            if (shipsInTransit.TurnLeft == 1)
            {
                shipsInEurope.Add(shipsInTransit.Ship);
                shipsReachEurope.Add(shipsInTransit);
            } else
            {
                shipsInTransit.TurnLeft --;
                if (shipsInTransit.TurnLeft < 1)
                {
                    shipsInTransit.TurnLeft = 1;
                }
            }
        }

        // Remove from Transit
        foreach (ShipInTransit shipReachEU in shipsReachEurope)
        {
            if (shipsToEurope.Contains(shipReachEU))
                shipsToEurope.Remove(shipReachEU);
        }
        shipsReachEurope.Clear();

        // From Europe
        List<ShipInTransit> shipsReachNewWorld = new List<ShipInTransit>();

        foreach (ShipInTransit shipInTransit in shipsFromEurope)
        {
        if (shipInTransit.TurnLeft <= 1)
            shipsReachNewWorld.Add(shipInTransit);
        else
            shipInTransit.TurnLeft--;
        }

        // Remove from Transit and place on the map
        foreach (ShipInTransit shipReachNW in shipsReachNewWorld)
        {
            shipsFromEurope.Remove(shipReachNW);
            GameManager.instance.CheckToGenerateShipFromEurope(shipReachNW.Ship);
        }

        UiManager.instance.RefreshEuropePanel();
    }

    public void UpdateEuropePrice(int i, int increment)
    {
        europeStocks[i].UpdatePrice(increment);
    }

    public void AllowToGoToNewWorld(NavalUnit ship)
    {
        if (!shipsInEurope.Contains(ship)) return;   // only ships docked in Europe may leave

        ShipInTransit shipInTransit = new ShipInTransit(ship, EUROPE_DISTANCE);

        shipsFromEurope.Add(shipInTransit);
        shipsInEurope.Remove(ship);

        UiManager.instance.UpdateIconsFromEuropeToNewWorld();
    }

    public void CheckIfPassengerInEuropeReadyToBoardShip(NavalUnit ship)
    {
        List<LandUnit> unitsToRemove = new List<LandUnit>();

        foreach (LandUnit landUnit in landUnitsInEurope)
        {
            if (landUnit.UnitType == UnitType.Land && landUnit.UnitStatus == UnitStatus.OnBoard)
            {
                if ((ship.Passengers.Count + ship.CargoList.Count) < ship.CargoHoldNum)
                {
                    landUnit.BoardingShip(ship);
                    landUnit.gameObject.SetActive(false);
                    unitsToRemove.Add(landUnit);
                }
            }
        }
        
        foreach (LandUnit unit in unitsToRemove)
            landUnitsInEurope.Remove(unit);
    }
}
