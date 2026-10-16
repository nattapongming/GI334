using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AiManager : MonoBehaviour
{
    public static AiManager instance;

    void Awake()
    {
        instance = this;
    }

    private void AutoMoveToHex(Unit unit)
    {
        unit.ShowHideSprite(true);

        if (unit.DestinationOnHex != null)
            unit.CheckMoveToDestination();
        else 
            RandomMoveOneHex(unit);
    }

    private IEnumerator EnemyUnitMoves()
    {
        foreach (Faction faction in GameManager.instance.Factions)
        {
            GameManager.instance.ResetAllUnits(faction);

            if (faction == GameManager.instance.PlayerFaction)
                continue;

            foreach (Unit unit in faction.Units)
            {
                GameManager.instance.SelectAiUnit(unit);

                if (unit.Visible)
                    CameraController.instance.MoveCamera(unit.CurPos);

                if (unit.UnitStatus == UnitStatus.OnBoard) 
                    continue;

                AutoMoveToHex(unit);

                yield return new WaitForSeconds(0.1f);
            }

            
        }
        GameManager.instance.GameTurn++;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        GameManager.instance.StartNewTuen();
    }

    // public void RandomMoveOneHex(Unit unit)
    // {
    //     Hex anyHex = null;
    //     int n = Random.Range(0, 6);

    //     // Move 
    //     anyHex = HexCalculator.FindHexByDir(unit.CurHex, (HexDirection)n, GameManager.instance.AllHexes);

    //     if (anyHex == null) return;

    //     switch (unit.UnitType)
    //     {
    //         case UnitType.Land:
    //             LandUnit landUnit = (LandUnit)unit;
    //             landUnit.PrepareMoveToHex(anyHex);
    //             break;

    //         case UnitType.Naval:
    //             NavalUnit navalUnit = (NavalUnit)unit;
    //             navalUnit.PrepareMoveToHex(anyHex);
    //             break;
    //     }

    // }

    public void RandomMoveOneHex(Unit unit)
    {
        List<Hex> canGo = new List<Hex>();

        foreach (Hex hex in HexCalculator.GetHexAround(GameManager.instance.AllHexes, unit.CurHex))
        {
            if (hex.MoveCost > unit.MovePoint) continue;                                  // too expensive
            if (unit.UnitType == UnitType.Land  && hex.HexType == HexType.Ocean) continue; // land unit → no sea
            if (unit.UnitType == UnitType.Naval && hex.HexType != HexType.Ocean) continue; // ship → no land
            if (hex.UnitsInHex.Exists(u => u.Faction != unit.Faction)) continue;           // don't walk onto other factions

            canGo.Add(hex);
        }

        if (canGo.Count == 0) return;   // really trapped → stay this turn

        unit.PrepareMoveToHex(canGo[Random.Range(0, canGo.Count)]);
    }


    public void StartAiTurn()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        StartCoroutine(EnemyUnitMoves());
    }
}