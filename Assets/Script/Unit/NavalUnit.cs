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
        base.StayOnHex(hex);

        foreach (LandUnit passenger in passengers)
        {
            // remove passenger from old ocean hex
            passenger.CurHex.UnitsInHex.Remove(passenger);
            // Add passenger to new ocean hex
            hex.UnitsInHex.Add(passenger);

            passenger.CurHex = hex;
            passenger.CurPos = hex.Pos;
        }
    }

    public int AddCargo(int id, Cargo newCargo)
    {
        if (cargoList.Count < cargoHoldNum)
        {
            cargoList.Add(new Cargo(newCargo.ProductID, newCargo.Quantity));
            return 0;
        } 
        // all cargo taken, check if there is space
        else
        {
            cargoList[id].Quantity += newCargo.Quantity;
            newCargo.Quantity = 0;

            if (cargoList[id].Quantity > 100)
            {
                int cargoLeft = cargoList[id].Quantity - 100;
                cargoList[id].Quantity = 100;
                return cargoLeft;
            }
        }

        return newCargo.Quantity;
    }
}