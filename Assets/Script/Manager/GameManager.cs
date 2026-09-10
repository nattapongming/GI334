using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    [SerializeField] private Grid grid;
    [SerializeField] Transform hexParent;
    [SerializeField] GameObject hexPrefab;

    public const int WIDTH = 50;
    public const int HEIGHT = 60;

    [SerializeField] private Hex[,] allHexes = new Hex[WIDTH, HEIGHT];
    public Hex[,] AllHexes { get { return allHexes; } }

    [SerializeField] private HexData[] hexData;
    public HexData[] HexData { get { return hexData; } }

    [SerializeField] private bool showingText;
    [SerializeField] private int oceanEdgeIndex;


    public static GameManager instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GenerateAllHexes();
    }

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current.iKey.wasPressedThisFrame)
        {
            ToggleHexTex();
        }
    }

    private void GenerateAllHexes()
    {
        for (int x = 0; x < WIDTH; x++)
        {
            for (int y = 0; y < HEIGHT; y++)
            {
                Vector3 hexPos = grid.GetCellCenterWorld(new Vector3Int(x, y));

                // Create obj and call hex component to initialize it
                GameObject hexObj = Instantiate(hexPrefab, hexPos, Quaternion.identity, hexParent);
                Hex hex = hexObj.GetComponent<Hex>();

                int i = Random.Range(1, hexData.Length);
                hex.HextInit(x,y,hexPos, this, i);
                allHexes[x, y] = hex;
            }
        }
    }

    // Helper Methods

    private void ToggleHexTex()
    {
        foreach (Hex hex in allHexes)
            hex.ToggleBasicText(!showingText);

        showingText = !showingText;
    }
}
