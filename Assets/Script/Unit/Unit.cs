using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public enum UnitType
{
    Land,
    Naval
}

public enum UnitStatus
{
    None,
    OnBoard,
    Fortified,
    Clearing,
    Building,
    WorkInField,
    WorkInTown,
    Hidden,
    ToBoard
}

public class Unit : MonoBehaviour, IPointerClickHandler
{
    // Basic info

    [SerializeField] protected string unitName;
    public string UnitName { get { return unitName; } }

    [SerializeField] protected UnitType unitType;
    public UnitType UnitType { get { return unitType; } }

    [SerializeField] protected UnitStatus unitStatus;
    public UnitStatus UnitStatus { get { return unitStatus; } set { unitStatus = value; } }

    [SerializeField] protected int strength;
    public int Strength { get { return strength; } set { strength = value; } }

    [SerializeField] protected int movePoint;
    public int MovePoint { get { return movePoint; } set { movePoint = value; } }

    [SerializeField] protected int movePointMax;
    public int MovePointMax { get { return movePointMax; } }

    [SerializeField] protected Faction faction;
    public Faction Faction { get { return faction; } }

    [SerializeField] protected int visualRange;
    public int VisualRange { get { return visualRange; } }

    [SerializeField] protected Unit targetUnit;
    public Unit TargetUnit { get { return targetUnit; } set { targetUnit = value; } }

    [SerializeField] protected SpriteRenderer statusSprite;
    public SpriteRenderer StatusSprite { get { return statusSprite; } }

    // Hex / position info

    [SerializeField] private Vector2 curPos;
    public Vector2 CurPos { get { return curPos; } set { curPos = value; } }

    [SerializeField] protected Hex curHex;
    public Hex CurHex { get { return curHex; } set { curHex = value; } }

    [SerializeField] protected Hex targetHex;
    public Hex TargetHex { get { return targetHex; } set { targetHex = value; } }

    [SerializeField] protected bool isMoving;
    public bool IsMoving { get { return isMoving; } set { isMoving = value; } }

    [SerializeField] protected bool isVisible;
    public bool Visible { get { return isVisible; } set { isVisible = value; } }

    [SerializeField] protected Hex destinationOnHex;
    public Hex DestinationOnHex { get { return destinationOnHex; } set { destinationOnHex = value; } }
    
    // Misc

    [Header("Unit")]
    [SerializeField] protected SpriteRenderer unitSprite;
    public SpriteRenderer UnitSprite { get { return unitSprite; } }

    [Header("Flag")]
    [SerializeField] protected SpriteRenderer flagSprite;

    [Header("Border")]
    [SerializeField] protected SpriteRenderer borderSprite;

    [Header("Status sprite")]
    [SerializeField] protected Sprite[] statusSpritesList;
    public Sprite[] StatusSpritesList { get { return statusSpritesList; } }

    [SerializeField] protected GameManager gameMgr;    

    protected virtual void Update()
    {
        if (isMoving == true)
        {
            MoveToHex();
        }

        if (Keyboard.current.enterKey.wasPressedThisFrame)
        {
            if (this == gameMgr.CurUnit 
                && gameMgr.PlayerTurn
                && curHex.Town != null 
                && curHex.Town.Faction == faction)
            {
                gameMgr.SetupCurrentTown(curHex.Town);
            }

        }
    }

    // OnHex

    public void SetUpPosition(Hex hex)
    {
        if (curHex != null)
        curHex.UnitsInHex.Remove(this);             // leave the old hex

        curHex = hex;

        if (!hex.UnitsInHex.Contains(this))
        hex.UnitsInHex.Add(this);            // join the new hex only once

        curPos = hex.Pos;
    }

    protected virtual void StayOnHex(Hex hex)
    {
        // Old Hex
        curHex.UnitsInHex.Remove(this);

        isMoving = false;
        curHex = hex;
        targetHex = null;
        transform.position = curHex.transform.position;

        // New Hex
        curHex.UnitsInHex.Add(this);

        if (faction == gameMgr.PlayerFaction)
        {
            gameMgr.ClearDarkFogAroundUnit(this);
            ToggleBorder(true, Color.green);
        }
        gameMgr.HideOtherLandUnits(this);
    }

    private void MoveToHex()
    {
        ToggleBorder(false, Color.green);

        transform.position = Vector2.MoveTowards(curPos, targetHex.transform.position, 4 * Time.deltaTime);
        curPos = transform.position;

        if (curPos == targetHex.Pos)
        {
            movePoint -= targetHex.MoveCost;
            StayOnHex(targetHex);

            if (faction == gameMgr.PlayerFaction)
            gameMgr.ClearDarkFogAroundEveryUnit(faction);
        }
    }

    // Move Methods

    public void SetUnitToFrontLayerOrder()
    {
        unitSprite.sortingOrder = 5;
        flagSprite.sortingOrder = 6;
    }

    public void SetUnitToNormalLayerOrder()
    {
        unitSprite.sortingOrder = 2;
        flagSprite.sortingOrder = 3;
    }

    public virtual void PrepareMoveToHex(Hex target)
    {
        if (target.MoveCost > movePoint)
            return;

        isMoving = true;
        targetHex = target;

        if (faction == gameMgr.PlayerFaction)
            gameMgr.LeaveSeenFogAroundUnit(this);
    }

    public void CheckMoveToDestination()
    {
        if (destinationOnHex == null) return;
        // Arrived
        if (curHex == destinationOnHex)
        {
            destinationOnHex = null;
            OnReachDestination();
            return;
        }

        // Find destination hex
        Hex nextHex = HexCalculator.FindNextHexToGo(this, curHex, destinationOnHex, gameMgr.AllHexes);

        if (nextHex == null)
        {
            // Full move points but still can't move → blocked, give up (AI moves randomly next turn)
            if (movePoint == movePointMax)
                destinationOnHex = null;
            return;
        }

        PrepareMoveToHex(nextHex);
    }

    protected virtual void OnReachDestination() { }   // child classes decide what happens on arrival

    public void SetUnitDestination(Hex hex)
    {
        destinationOnHex = hex;
    }

    // Sprite Methods

    public void ToggleBorder(bool flag, Color32 color)
    {
        borderSprite.gameObject.SetActive(flag);
        borderSprite.color = color;
    }

    public void ShowHideSprite(bool flag)
    {
        unitSprite.gameObject.SetActive(flag);
        flagSprite.gameObject.SetActive(flag);
    }

    // Input Methods

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (faction == gameMgr.PlayerFaction)
            {
                gameMgr.SelectPlayerUnit(this);
            }
        }

        if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (gameMgr.CurUnit == null) return;
            
            if (gameMgr.CheckIfHexIsAdjacent(gameMgr.CurUnit.CurHex, curHex))
            {
                if (faction == gameMgr.PlayerFaction) // Same faction
                    gameMgr.CurUnit.PrepareMoveToHex(curHex);
                else // Diff side unit
                {
                    Debug.Log($"{gameMgr.CurUnit} Attacks {unitName}");

                    gameMgr.CurUnit.AttackUnit(curHex, this);
                }
            }
        }
    }

    // Combat Medthod

    public void AttackUnit(Hex target, Unit defender) // Begin to attack
    {
        if (target.MoveCost > movePoint) return;

        targetHex = target;
        targetUnit = defender;
        movePoint = 0;

        gameMgr.StartCombat(this, defender);
    }

    // Satus

    public void ChangeStatusIcon()
    {
        if (statusSprite == null)
            return;

        switch (unitStatus)
        {
            case UnitStatus.Clearing:
                statusSprite.gameObject.SetActive(true);
                statusSprite.sprite = statusSpritesList[0];
                break;
            case UnitStatus.Building:
                statusSprite.gameObject.SetActive(true);
                statusSprite.sprite = statusSpritesList[1];
                break;
            default:
                statusSprite.gameObject.SetActive(false);
                statusSprite = null;
                break;
        }
    }
}