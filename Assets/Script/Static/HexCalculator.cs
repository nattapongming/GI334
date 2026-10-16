using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum HexDirection
{
    TopLeft,
    TopRight,
    Right,
    BottomRight,
    BottomLeft,
    Left
}

public static class HexCalculator
{
    public static Hex FindHexByDir(Hex center, HexDirection dir, Hex[,] hexes)
    {   
        int width = GameManager.WIDTH;
        int height = GameManager.HEIGHT;

        Hex hexResult = null;

        int x = center.X;
        int y = center.Y;

        switch (dir)
        {
            case HexDirection.TopLeft:
                if ((y % 2) == 0)
                    x -= 1;
                y += 1;
                break;

            case HexDirection.TopRight:
                if ((y % 2) != 0)
                    x += 1;
                y += 1;
                break;

            case HexDirection.Right:
                x += 1;
                break;

            case HexDirection.BottomRight:
                if ((y % 2) != 0)
                    x += 1;
                y -= 1;
                break;

            case HexDirection.BottomLeft:
                if ((y % 2) == 0)
                    x -= 1;
                y -= 1;
                break;

            case HexDirection.Left:
                x -= 1;
                break;

        }

        if ((x < 0 || x >= width) || (y < 0 || y >= height))
            return null;

        hexResult = hexes[x,y];
        return hexResult;
    }

    public static List<Hex> GetHexAround(Hex[,] hexes, Hex center)
    {
        List<Hex> hexList = new List<Hex>();

        for (int i = 0; i < 6; i++)
        {
            Hex hex = FindHexByDir(center, (HexDirection)i, hexes);

            if (hex != null)
                hexList.Add(hex);
        }
        return hexList;
    }

    public static bool CheckIfHexAroundHasTown(Hex[,] hexes, Hex center)
    {
        for (int i = 0; i < 6; i++)
        {
            Hex hex = FindHexByDir(center, (HexDirection)i, hexes);

            if (hex == null)
            continue;

            if (hex.HasTown)
            return true;
        } return false;
    }

    public static Hex[] GetHexAroundToArray(Hex[,] hexes, Hex center)
    {
        Hex[] hexArray = new Hex[6];

        for (int i = 0; i < 6; i++)
        {
            Hex hex = FindHexByDir(center, (HexDirection)i, hexes);
            hexArray[i] = hex;
        }
        return hexArray;
    }

    public static Faction CheckNeverMetFaction(Hex[,] hexes, Unit playerUnit)
    {
        List<Hex> aroundHexes = new List<Hex>();
        aroundHexes = GetHexAround(hexes, playerUnit.CurHex);

        foreach (Hex hex in aroundHexes)
        {
            foreach (Unit other in hex.UnitsInHex)
            {
                if (other.Faction != playerUnit.Faction)
                {
                    if (other.Faction.MetByPlayer == false)
                    return other.Faction;
                }
            }
        }
        return null;
    }

    private static List<HexDirection> GetHexDirectionList(Hex currentHex, Hex destHex)
    {
        List<HexDirection> directions = new List<HexDirection>();

        if (destHex.Y > currentHex.Y)            // above
        {
            if (destHex.X <= currentHex.X)
                directions.AddRange(new[] { HexDirection.TopLeft, HexDirection.TopRight, HexDirection.Left });
            else
                directions.AddRange(new[] { HexDirection.TopRight, HexDirection.TopLeft, HexDirection.Right });
        }
        else if (destHex.Y < currentHex.Y)       // below
        {
            if (destHex.X <= currentHex.X)
                directions.AddRange(new[] { HexDirection.BottomLeft, HexDirection.BottomRight, HexDirection.Left });
            else
                directions.AddRange(new[] { HexDirection.BottomRight, HexDirection.BottomLeft, HexDirection.Right });
        }
        else                                     // same row
        {
            if (destHex.X > currentHex.X)
                directions.AddRange(new[] { HexDirection.Right, HexDirection.TopRight, HexDirection.BottomRight });
            else
                directions.AddRange(new[] { HexDirection.Left, HexDirection.TopLeft, HexDirection.BottomLeft });
        }
        return directions;
    }


    public static Hex FindNextHexToGo(Unit unit, Hex currentHex, Hex destinationHex, Hex[,] allHexes)
    {
        if (destinationHex == null) return null;

        if (destinationHex == currentHex) return null;

        List<HexDirection> directions = GetHexDirectionList(currentHex, destinationHex);

        List<Hex> hexesAround = new List<Hex>();

        foreach (HexDirection dir in directions)
        {
            Hex hex = FindHexByDir(currentHex, dir, allHexes);
            if (hex == null) continue;

            if (unit.UnitType == UnitType.Naval && hex.HexType == HexType.Ocean)
                hexesAround.Add(hex);

            if (unit.UnitType == UnitType.Land && hex.HexType != HexType.Ocean)
                hexesAround.Add(hex);
        }

        foreach (Hex hex in hexesAround)
        {
            if (hex.MoveCost <= unit.MovePoint) return hex;
        }

        return null;
    }
}