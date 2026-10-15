using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public enum LandUnitType
{
    FreeColonists,
    VeteranSoldiers,
    HardyPioneers,
    PlainIndians,
    TropicalIndians,
    SeasonedScouts,
    Farmers,
    Fishermen,
    Lumberjacks,
    Carpenters,
    OreMiners,
    Blacksmiths
}

public class LandUnit : Unit
{
    [SerializeField] private LandUnitType landUnitType;
    public LandUnitType LandUnitType { get { return landUnitType; } set { landUnitType = value; } }

    [SerializeField] private int toolsNum; // max 100
    public int ToolsNum { get { return toolsNum; } set { toolsNum = value; } }

    [SerializeField] private int musketsNum; // max 50
    public int MusketsNum { get { return musketsNum; } set { musketsNum = value; } }

    [SerializeField] private int horseNum; // max 50
    public int HorseNum { get { return horseNum; } set { horseNum = value; } }

    [SerializeField] private bool armed = false;
    public bool Armed { get { return armed; } set { armed = value; } }

    [SerializeField] private bool hasMusket = false;
    public bool HasMusket { get { return hasMusket; } set { hasMusket = value; } }

    [SerializeField] private bool hasHorse;
    public bool HasHorse { get { return hasHorse; } set { hasHorse = value; } }

    [SerializeField] private NavalUnit transportShip;
    public NavalUnit TransportShip { get { return transportShip;} set { transportShip = value; } }

    [SerializeField] protected UiManager uiMgr;

    protected override void Update()
    {
        base.Update();

        if (Keyboard.current.bKey.wasPressedThisFrame)
        {
            if (this == gameMgr.CurUnit && gameMgr.PlayerTurn)
                BuildSettlement();
        }

        if (Keyboard.current.pKey.wasPressedThisFrame)
        {
            if (this == gameMgr.CurUnit && gameMgr.PlayerTurn)
                ClearingLand();
        }
    }

    public override void PrepareMoveToHex(Hex target)
    {
        if (unitStatus == UnitStatus.Clearing 
        || unitStatus == UnitStatus.Building
        || unitStatus == UnitStatus.WorkInField)
        {
            Debug.Log("Unit is busy");
            return;
        }

        if (unitStatus == UnitStatus.OnBoard)
        {
            SetUpPosition(transportShip.CurHex);
            transform.position = CurPos;
        }

        if (target.HexType != HexType.Ocean)
        {
            base.PrepareMoveToHex(target);
            if (unitStatus == UnitStatus.OnBoard)
                MakeLandFall();
        }
        else if (gameMgr.CheckIfHexHasOurShipToBoard(target))
        {
            base.PrepareMoveToHex(target);
        }
        else
        {
            StayOnHex(curHex);
        }
    }

    protected override void StayOnHex(Hex hex)
    {
        base.StayOnHex(hex);

        if (hex.HexType != HexType.Ocean)
            return;

        // Check again if this ship can be boarded
        NavalUnit ship = gameMgr.CheckIfHexHasOurShipToBoard(hex);

        if (ship != null)
            BoardingShip(ship);

        if (faction == gameMgr.PlayerFaction)
            gameMgr.CheckUnmetFaction(this);
    }

    public void UnitInit(GameManager gameMgr, UiManager uiMgr ,Faction fact, LandUnitData data)
    {
        this.gameMgr = gameMgr;
        this.uiMgr = uiMgr;
        faction = fact;
        flagSprite.sprite = fact.ShieldIcon;

        unitName = data.unitName;
        movePointMax = data.movePointMax;
        movePoint = data.movePointMax;
        strength = data.strength;
        visualRange = data.visualRange;
        unitSprite.sprite = data.unitIcon;
        unitStatus = UnitStatus.None;

        landUnitType = data.landUnitType;
        if (landUnitType == LandUnitType.HardyPioneers) 
            toolsNum = 100;

        armed = data.armed;

        hasMusket = data.hasMusket;
        if (hasMusket)
            musketsNum = 50;

        hasHorse = data.hasHorse;
        if (hasHorse)
            horseNum = 50;

    }

    private void MakeLandFall()
    {
        unitStatus = UnitStatus.None;
        gameObject.transform.parent = faction.UnitParent.transform;
        transportShip.Passengers.Remove(this);
        transportShip = null;
    }

    public void BoardingShip(NavalUnit ship)
    {
        if (transportShip == ship) return;
        else if (transportShip != null)
         transportShip.Passengers.Remove(this);

        ship.Passengers.Add(this);
        transportShip = ship;
        gameObject.transform.parent = ship.PassengerParent.transform;
        unitStatus = UnitStatus.OnBoard;
    }

    public void ClearingLand()
    {
        if (landUnitType != LandUnitType.HardyPioneers)
            return;

        if (unitStatus == UnitStatus.Clearing) 
            return;

        if (curHex.HexType == HexType.Ocean 
        || curHex.HexType == HexType.Mountain
        || curHex.HexType == HexType.Hills
        || !curHex.HasForest)
        {
            uiMgr.ShowColonyNoForestText();
        }
        else if (toolsNum < 20)
        {
            //Debug.Log("Not enough tools");
            uiMgr.ShowColonyNotEnoughToolsText(toolsNum);
        }
        else
        {
            unitStatus = UnitStatus.Clearing;
            toolsNum -= 20;
        }
    }

    public void BuildSettlement()
    {
        if (landUnitType != LandUnitType.HardyPioneers)
            return;

        if (unitStatus == UnitStatus.Building || curHex.HasTown)
            return;

        Debug.Log("Build Settlement");

        if (curHex.HexType == HexType.Ocean 
        || curHex.HexType == HexType.Mountain
        || curHex.HexType == HexType.Hills
        || curHex.HasForest)
        {
            //Debug.Log("Must be on Cleared Land");
            uiMgr.ShowColonyClearLandText();
        }
        else if (toolsNum < 20)
        {
            //Debug.Log("Not enough tools");
            uiMgr.ShowColonyNotEnoughToolsText(toolsNum);
        }
        else if (gameMgr.CheckIfAroundHexHasTown(curHex))
        {
            uiMgr.ShowColonyHasOtherTownAroundText();
        }
        else
        {
            unitStatus = UnitStatus.Building;
            toolsNum -= 20;
        }
    }
}