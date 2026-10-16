using System.Collections.Generic;
using UnityEngine;

public enum NavalUnitType
{
    Caraval,
    Merchantman,
    Galleon,
    Privateer,
    Frigate,
    ManOWar
}

public class NavalUnit : Unit
{
    [SerializeField] private NavalUnitType navalUnitType;
    public NavalUnitType NavalUnitType { get { return navalUnitType; } set { navalUnitType = value; } }

    [SerializeField] private bool armed = false;
    
    [SerializeField] private int cargoHoldNum;
    public int CargoHoldNum { get { return cargoHoldNum; } }

    [SerializeField] private List<LandUnit> passengers = new List<LandUnit>();
    public List<LandUnit> Passengers { get { return passengers; } set { passengers = value; } }

    [SerializeField] private GameObject passengerParent;
    public GameObject PassengerParent { get { return passengerParent; } }

    [SerializeField] private List<Cargo> cargoList = new List<Cargo>();
    public List<Cargo> CargoList { get { return cargoList; } set { cargoList = value; } }

    public void UnitInit(GameManager gameMgr, Faction fact, NavalUnitData data)
    {
        this.gameMgr = gameMgr;
        faction = fact;
        flagSprite.sprite = fact.ShieldIcon;

        unitName = data.unitName;
        movePointMax = data.movePointMax;
        movePoint = data.movePointMax;
        strength = data.strength;
        visualRange = data.visualRange;
        unitSprite.sprite = data.shipIcon;
        navalUnitType = data.navalUnitType;
        armed = data.armed;
        cargoHoldNum = data.cargoHoldNum;
    }

    public override void PrepareMoveToHex(Hex target)
    {
        base.PrepareMoveToHex(target);

        if (target.HexType != HexType.Ocean)
        {
            if (targetHex.HasTown)
            {
                // not our town
                if (targetHex.Town.Faction != gameMgr.PlayerFaction) StayOnHex(curHex);
            } else
            StayOnHex(curHex);
        }
    }

    protected override void StayOnHex(Hex hex)
    {
        Hex oldHex = curHex;

        base.StayOnHex(hex);

        foreach (LandUnit unit in passengers)
        {
            // remove passenger from old ocean hex
            unit.CurHex.UnitsInHex.Remove(unit);
            // Add passenger to new ocean hex
            hex.UnitsInHex.Add(unit);

            unit.CurHex = hex;
            unit.CurPos = hex.Pos;
        }

        if (faction == gameMgr.PlayerFaction)
            gameMgr.CheckUnmetFaction(this);

        if (destinationOnHex != null)
            CheckMoveToDestination();

        //gameMgr.CheckShipToEurope();
        if (faction == gameMgr.PlayerFaction    // only YOUR ships (no pop-up on AI turn)
        && oldHex != hex                    // only if it really moved (not "clicked land")
        && hex.X == GameManager.WIDTH - 1)  // reached the Europe edge
        {
            CameraController.instance.MoveCamera(CurPos);
            EuropeManager.instance.QuestionGoToEurope(this);
        }
    }

    public int AddCargo(int id, Cargo newCargo)
    {
        if (newCargo.Quantity <= 0) return 0;                 // nothing to load
        
        // hold already has cargo → only stack the SAME product
        if (id < cargoList.Count)
        {
            if (cargoList[id].ProductID != newCargo.ProductID)
                return newCargo.Quantity;                     // reject, all goes back to town

            cargoList[id].Quantity += newCargo.Quantity;

            if (cargoList[id].Quantity > 100)
            {
                int cargoLeft = cargoList[id].Quantity - 100;
                cargoList[id].Quantity = 100;
                return cargoLeft;                             // extra goes back to town
            }
            return 0;
        }

        // empty hold → add a new one (if the ship has space)
        if (cargoList.Count < cargoHoldNum)
        {
            cargoList.Add(new Cargo(newCargo.ProductID, newCargo.Quantity));
            return 0;
        }


        return newCargo.Quantity;
    }

    protected override void OnReachDestination()
    {
        if (faction == gameMgr.PlayerFaction) return;   // AI only
        if (passengers.Count == 0) return;

        Hex landHex = FindLandHexAround();
        if (landHex == null) return;

        // copy: DisembarkToHex removes from passengers while we loop
        List<LandUnit> leaving = new List<LandUnit>(passengers);

        foreach (LandUnit unit in leaving)
            unit.DisembarkToHex(landHex);
    }

    private Hex FindLandHexAround()
    {
        foreach (Hex hex in HexCalculator.GetHexAround(gameMgr.AllHexes, curHex))
        {
            if (hex.HexType != HexType.Ocean && !hex.HasTown)
                return hex;
        }
        return null;
    }

}