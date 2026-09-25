using System.Collections.Generic;
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

    
}