using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.UI;

public enum HexType 
{
    Ocean,
    GrassLand,
    Prairie,
    Savanna,
    Jungle,
    Tundra,
    Desert,
    Swamp,
    Arctic,
    Hills,
    Mountain
}

public class Hex : MonoBehaviour
{
    [SerializeField]
    private int x;
    public int X { get { return x; } set { x = value; } }

    [SerializeField]
    private int y;
    public int Y { get { return y; } set { y = value; } }

    [SerializeField]
    private Vector2 pos;
    public Vector2 Pos { get { return pos; } set { pos = value; } }

    [SerializeField]
    private HexType hexType = HexType.GrassLand;
    public HexType HexType { get { return hexType; } }

    [Header("Basic")]
    [SerializeField] private SpriteRenderer terrainSprite;
    [SerializeField] private SpriteRenderer forestSprite;

    [Header("Fog of War")]
    [SerializeField] private SpriteRenderer fogSprite;
    [SerializeField] private SpriteRenderer darkSprite;

    [Header("Town")]
    [SerializeField] private bool hasTown;
    public bool HasTown { get { return hasTown; } set { hasTown = value; } }

    [Header("River")]
    [SerializeField] private bool hasRiver;
    public bool HasRiver { get { return hasRiver; } set { hasRiver = value; } }

    [Header("Forest")]
    [SerializeField] private bool hasForset;
    public bool HasForset { get { return hasForset; } set {hasForset = value; } }

    [Header("Movecost")]
    private int moveCost = 1;
    public int MoveCost { get { return moveCost; } }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
