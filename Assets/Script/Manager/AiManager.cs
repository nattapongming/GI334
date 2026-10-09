using UnityEngine;
using System.Collections;

public class AiManager : MonoBehaviour
{
    public static AiManager instance;

    void Awake()
    {
        instance = this;
    }

    private void AutoMoveToHex(Unit unit)
    {
        int n = Random.Range(0, 6);

        Hex toGoHex = HexCalculator.FindHexByDir(unit.CurHex, (HexDirection)n, GameManager.instance.AllHexes);

        if(toGoHex == null) return;

        unit.ShowHideSprite(true);
        unit.PrepareMoveToHex(toGoHex);
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

    public void StartAiTurn()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        StartCoroutine(EnemyUnitMoves());
    }
}