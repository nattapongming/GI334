using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private Grid grid;
    [SerializeField] Transform hexParrent;
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
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
