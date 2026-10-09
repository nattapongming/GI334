using UnityEngine;

public class Town : MonoBehaviour
{
    [SerializeField] protected Faction faction;
    public Faction Faction { get { return faction; } }

    [SerializeField] protected int visualRange;
    public int VisualRange { get { return visualRange; } }

    [SerializeField] private Vector2 curPos;
    public Vector2 CurPos { get { return curPos; } set { curPos = value; } }

    [SerializeField] protected Hex curHex;
    public Hex CurHex { get { return curHex; } set { curHex = value; } }

    [Header("Town")]
    [SerializeField] protected SpriteRenderer townSprite;
    public SpriteRenderer TownSprite { get { return townSprite; } }

    [Header("Border")]
    [SerializeField] protected SpriteRenderer borderSprite;

    [Header("Flag")]
    [SerializeField] protected SpriteRenderer flagSprite;

    [SerializeField] protected GameManager gameMgr;

    // Product

    [SerializeField] private int[] warehouse = new int [16];
    public int[] Warehouse { get { return warehouse; } }

    [SerializeField] private int[] totalYieldThisTurn = new int [16]; // Production this turn
    public int[] TotalYieldThisTurn { get { return totalYieldThisTurn; } set { totalYieldThisTurn = value; } }

    [SerializeField] private int crossNum; // no, of crosses produced in this turn
    public int CrossNum { get { return crossNum; } set { crossNum = value; } }
    [SerializeField] private int bellNum; // no, of bells produced in this turn
    public int BellNum { get { return bellNum; } set { bellNum = value; } }
    public void TownInit(GameManager gameMgr, Faction fact)
    {
        this.gameMgr = gameMgr;
        faction = fact;
        flagSprite.sprite = fact.FlagIcon;
        townSprite.sprite = fact.TownIcon;
    }

    public void AccumulateToWarehouse()
    {
        for (int i = 0; i < 16; i++)
        {
            warehouse[i] += totalYieldThisTurn[i];
        }
    }
}