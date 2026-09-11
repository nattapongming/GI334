using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

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

public class Hex : MonoBehaviour, IPointerClickHandler
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
    [SerializeField] private string hexName;
    public string HexName { get { return hexName; } set { hexName = value; } }
    [SerializeField] private int[] resourceYields;
    public int[] ResourceYields { get { return resourceYields; } set { resourceYields = value; } }

    [Header("Movecost")]
    private int moveCost = 1;
    public int MoveCost { get { return moveCost; } }
    
    
    // For of War


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
    [SerializeField] private bool hasForest;
    public bool HasForest { get { return hasForest; } set { hasForest = value; } }

    

    // Sprites


    [Header("Terrain")]
    [SerializeField] private Sprite[] terrainSprites;

    [Header("Forest")]
    [SerializeField] private Sprite[] forestSprites;

    [SerializeField] protected bool visible = false;
    public bool Visible { get { return visible; } set { visible = value; } }
    
    // Special


    [Header("Special")]
    [SerializeField] private bool specialHex;
    public bool SpecialHex { get { return specialHex; } set { specialHex = value; } }

    // Misc.
    [Header("Misc.")]
    [SerializeField] private TMP_Text hexText;
    private GameManager gameManager;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void HextInit(int x, int y, Vector2 pos, GameManager gameManager, int i)
    {
        this.x = x;
        this.y = y;
        this.pos = pos;
        this.gameManager = gameManager;

        hexText.text = $"{x}, {y}";

        hexName = gameManager.HexData[i].hexName;
        hexType = gameManager.HexData[i].hexType;
        terrainSprites = gameManager.HexData[i].terrainSprites;
        forestSprites = gameManager.HexData[i].forestSprites;
        resourceYields = gameManager.HexData[i].resourceYields;
        moveCost = gameManager.HexData[i].moveCost;

        SetSortingOrder();

        RandomTerrainSprite(terrainSprites);

        // When init 85% for forest
        int n = Random.Range(1, 101);

        if (n <= 85)
        {
            if (forestSprites.Length > 0)
            {
                RandomForestSprite(forestSprites);
                hasForest = true;
                moveCost += 1;
            }
        }
    }

    private void SetSortingOrder()
    {
        // Sorting Order
        int baseOrder = y * -10;

        terrainSprite.sortingOrder = baseOrder;
        forestSprite.sortingOrder = baseOrder + 1;
        fogSprite.sortingOrder = baseOrder + 5;
        darkSprite.sortingOrder = baseOrder + 6;

        // Artic, Hills, Mountain Sorting Order
        // Just in case want to know which one is higher than flat
        switch (hexType)
        {
            case HexType.Arctic:
                break;
            case HexType.Hills:
                break;
            case HexType.Mountain:
                terrainSprite.sortingOrder = baseOrder + 1; // Higher terrain
                break;
        }
    }

    // Hex Methods

    public void ClearForest()
    {
        hasForest = false;
        forestSprite.gameObject.SetActive(false);
        moveCost -= 1;
    }

    private void ToggleFog(bool flag)
    {
        fogSprite.gameObject.SetActive(flag);
    }

    private void ToggleDark(bool flag)
    {
        darkSprite.gameObject.SetActive(flag);
    }

    public void DiscoverHex()
    {
        ToggleFog(false);
        ToggleDark(false);
        visible = true;

    }

    public void SeenHex()
    {
        ToggleFog(true);
        ToggleDark(false);
        visible = false;
    }

    // Text Method

    public void ToggleBasicText(bool flag)
    {
        hexText.gameObject.SetActive(flag);
    }

    // Sprite Method

    private void RandomTerrainSprite(Sprite[] sprites)
    {
        int i = Random.Range(0, sprites.Length);
        terrainSprite.sprite = sprites[i];
    }

    private void RandomForestSprite(Sprite[] sprites)
    {
        int i = Random.Range(0, sprites.Length);
        forestSprite.gameObject.SetActive(true);
        forestSprite.sprite = sprites[i];
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (gameManager.CheckIfHexIsAdjacent(gameManager.CurUnit.CurHex, this))
            {
                gameManager.CurUnit.PrepareMoveToHex(this);
            }
        }
    }
}
