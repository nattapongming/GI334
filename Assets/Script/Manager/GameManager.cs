using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

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


    // Biome placement


    public const int ARCTICNORTH = HEIGHT - 2; // 58
    public const int ARCTICSOUTH = 2;

    public const int TUNDRANORTH = HEIGHT - 6; // 54
    public const int TUNDRASOUTH = 6;

    public const int GRASSNORTH = HEIGHT - 12; // 48
    public const int GRASSSOUTH = 12;

    public const int PRAIRIENORTH = HEIGHT - 20; // 40
    public const int PRAIRIESOUTH = 20;

    public const int SAVANNANORTH = HEIGHT - 24; // 36
    public const int SAVANNASOUTH = 24;

    public const int TROPICALNORTH = HEIGHT - 30; // 30
    public const int TROPICALSOUTH = 30;

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
        DetermineOcean();
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

    // Generate Methods

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

                int n = Random.Range(oceanEdgeIndex - 3, oceanEdgeIndex + 4);

                if (x >= n)
                {
                    hex.HextInit(x,y,hexPos, this, 0); //Ocean
                } else
                {
                    // int i = Random.Range(1, hexData.Length);
                    // hex.HextInit(x,y,hexPos, this, i); // Land

                    GenerateAllBiome(x, y, hex, hexPos); // Land
                }
                

                allHexes[x, y] = hex;
            }
        }
    }

    private void GenerateBiome(int x, int y, Hex hex, Vector3 hexPos, int
    defaultTerrain, List<int> otherTerrains)
    {
        int n = Random.Range(1, 101);

        if (n <= 50)
            hex.HextInit(x, y, hexPos, this, defaultTerrain); // Main Biome Land
        else
        {
            int i = Random.Range(0, otherTerrains.Count);
            hex.HextInit(x, y, hexPos, this, otherTerrains[i]); // Other Biome Land
        }
        
    }

    private void GenerateAllBiome(int x, int y, Hex hex, Vector3 hexPos)
    {
        int n = Random.Range(1, 101);

        // Arctic
        if ((y >= 0 && y < ARCTICSOUTH) || (y >= ARCTICNORTH && y < HEIGHT))
        {
            GenerateBiome(x, y, hex, hexPos, 8, new List<int> { 5 }); // Tundra
        }

        // Tundra
        else if ((y >= ARCTICSOUTH && y < TUNDRASOUTH) || (y >= TUNDRANORTH && y < ARCTICNORTH))
        {
            GenerateBiome(x, y, hex, hexPos, 5, new List<int> { 1 }); // Grassland
        }

        // Grassland
        else if ((y >= TUNDRASOUTH && y < GRASSSOUTH) || (y >= GRASSNORTH && y < TUNDRANORTH))
        {
            GenerateBiome(x, y, hex, hexPos, 1, new List<int> { 2, 5 }); // Prairie, Tundra
        }

        // Prairie
        else if ((y >= GRASSSOUTH && y < PRAIRIESOUTH) || (y >= PRAIRIENORTH && y < GRASSNORTH))
        {
            GenerateBiome(x, y, hex, hexPos, 2, new List<int> { 1, 3, 4 }); // Grassland, Savanna, Jungle
        }

        // Savanna
        else if ((y >= PRAIRIESOUTH && y < SAVANNASOUTH) || (y >= SAVANNANORTH && y < PRAIRIENORTH))
        {
            GenerateBiome(x, y, hex, hexPos, 3, new List<int> { 1, 2, 4, 6 }); // Grassland, Prairie, Jungle, Desert
        }

        // Tropical
        else if ((y >= SAVANNASOUTH && y < TROPICALSOUTH) || (y >= 30 && y < SAVANNANORTH))
        {
            GenerateBiome(x, y, hex, hexPos, 4, new List<int> { 1, 7 }); // Grassland, Swamp
        }

        //**Special Conditions**
        //Hills
        if (n > 80)
        {
            hex.ClearForest();
            GenerateBiome(x, y, hex, hexPos, 9, new List<int> {10}); // Mountains
        }
    }

    private void DetermineOcean()
    {
        oceanEdgeIndex = WIDTH - Random.Range(7, 10);
    }


    // Helper Methods

    
    private void ToggleHexTex()
    {
        foreach (Hex hex in allHexes)
            hex.ToggleBasicText(!showingText);

        showingText = !showingText;
    }
}
