using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;

public class GameManager : MonoBehaviour
{
    // Grid

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

    // Faction

    [SerializeField] private Faction playerFaction;
    public Faction PlayerFaction { get { return playerFaction; } }

    [SerializeField] private Faction[] factions;
    public Faction[] Factions { get { return factions; } }

    [SerializeField] private FactionData[] factionData;
    public FactionData[] FactionDatas { get { return factionData; } }

    [SerializeField] private Faction curForeignFaction; // cur Faction that player meet
    public Faction CurForeignFaction { get { return curForeignFaction; } set { curForeignFaction = value; } }


    // Unit Prefab

    [SerializeField] private GameObject landUnitPrefab;
    [SerializeField] private GameObject navelUnitPrefab;
    [SerializeField] private GameObject townPrefab;


    // Cur unit

    [SerializeField] private Unit curUnit;
    public Unit CurUnit { get { return curUnit; } set { curUnit = value; } }

    [SerializeField] private Unit curAiUnit;
    public Unit CurAiUnit { get { return curAiUnit; } set { curAiUnit = value; } }

    [SerializeField] private Town curTown;
    public Town CurTown { get { return curTown; } set { curTown = value; } }

    // Unit Data

    [SerializeField] private LandUnitData[] landUnitData;
    public LandUnitData[] LandUnitData { get { return landUnitData; } }

    [SerializeField] private NavalUnitData[] navalUnitData;
    public NavalUnitData[] NavalUnitData { get { return navalUnitData; } }

    // Turn

    [SerializeField] private int gameTurn;
    public int GameTurn { get { return gameTurn; } set { gameTurn = value;} }
    
    // Player turn

    [SerializeField] private bool playerTurn = true;
    public bool PlayerTurn { get { return playerTurn; } set { playerTurn = value; } }

    [SerializeField] private int nativeTownNum;

    // Product

    [SerializeField] private ProductData[] productDatas;
    public ProductData[] ProductDatas { get { return productDatas; } }

    // Combat

    [SerializeField] private Unit winner;

    [SerializeField] private Unit loser;

    // Misc

    [SerializeField] private int year = 1600;
    public int Year { get { return year; } set { year = value; } }

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
        SetUpFaction();
        SelectPlayerFaction();
        DetermineOcean();
        GenerateAllHexes();

        GenerateAllEuropeanShips();
        GenerateAllEuropeanExploreUnits();
        GenerateAllNativeTowns();
        GenerateAllNativeUnits();

        ShowFirstKingDialogText();
    }

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current.iKey.wasPressedThisFrame)
        {
            ToggleHexText();
        }

        if (Keyboard.current.tabKey.wasPressedThisFrame)
            SelectNextPlayerUnit();

        if (Keyboard.current.backspaceKey.wasPressedThisFrame)
            EndTurn();

        if (Keyboard.current.eKey.wasPressedThisFrame)
            EuropePanel();
    }

    public void EuropePanel()
    {
        UiManager.instance.CheckEuropePanel();   
    }

    // Hex method

    public void ShowToggleBorder(Unit unit)
    {
        if (unit.Faction == playerFaction)
            unit.ToggleBorder(true, Color.green);
        else 
            unit.ToggleBorder(true, Color.red);

    }

    public void ClearToggleBorder(Unit unit)
    {
        unit.ToggleBorder(false, Color.green);
    }

    public void FocusPlayerUnit(Unit unit)
    {
        ShowToggleBorder(unit);
    }

    public void ClearDarkFogAroundUnit(Unit unit)
    {
        unit.CurHex.DiscoverHex();

        List<Hex> adjHexes = HexCalculator.GetHexAround(allHexes, unit.CurHex);

        foreach (Hex hex in adjHexes)
        {
            hex.DiscoverHex();
        }
    }

    public bool CheckIfHexIsAdjacent(Hex centerHex, Hex targetHex)
    {
        List<Hex> adjHexes = HexCalculator.GetHexAround(allHexes, centerHex);

        return (adjHexes.Contains(targetHex)) ? true : false;
    }

    // Turn method

    public void EndTurn()
    {
        if (curUnit != null)
            curUnit.ToggleBorder(false, Color.green);

        UiManager.instance.CheckActiveUIPanel();

        Debug.Log("EndTurn");
        playerTurn = false;
        AiManager.instance.StartAiTurn();
    }

    public void StartNewTuen()
    {
        playerTurn = true;
        SelectPlayerFirstUnit();
        AccumulateStockAllTowns();
        EuropeManager.instance.UpdateShipInTransitTurn();
    }

    // Unit method

    public void LeaveSeenFogAroundUnit(Unit unit)
    {
        unit.CurHex.SeenHex();

        List<Hex> adjHexes = HexCalculator.GetHexAround(allHexes, unit.CurHex);

        foreach (Hex hex in adjHexes)
        {
            hex.SeenHex();
        }
    }

    public void ClearDarkFogAroundEveryUnit(Faction faction)
    {
        foreach (Unit unit in faction.Units)
        {
            if (unit.CurHex == null) continue; 
            ClearDarkFogAroundUnit(unit);
        }
    }

    public void CheckUnitClearingLand(Faction faction, Unit unit)
    {
        if (unit.UnitStatus == UnitStatus.Clearing)
        {
            unit.CurHex.ClearForest();
            unit.UnitStatus = UnitStatus.None;
        }
    }

    public void CheckUnitBuildingSettlement(Faction faction, Unit unit)
    {
        if (unit.UnitStatus == UnitStatus.Building)
        {
            GenerateTown(faction, unit.CurHex);
            unit.UnitStatus = UnitStatus.None;
        }
    }

    public NavalUnit CheckIfHexHasOurShipToBoard(Hex hex)
    {
        foreach (Unit unit in hex.UnitsInHex)
        {
            if (unit.UnitType == UnitType.Naval && unit.Faction == playerFaction)
            {
                NavalUnit ship = (NavalUnit)unit;
                if (ship.Passengers.Count < ship.CargoHoldNum)
                    return ship;
            }
        }
        return null;
    }

    public void ResetAllUnits(Faction faction)
    {
        foreach (Unit unit in faction.Units)
        {
            CheckUnitClearingLand(faction, unit);
            CheckUnitBuildingSettlement(faction, unit);
            unit.MovePoint = unit.MovePointMax;
        }
    }

    public void SelectAiUnit(Unit unit)
    {
        if (curUnit != null)
            ClearToggleBorder(curUnit);

        curAiUnit = unit;

        FocusPlayerUnit(curAiUnit);
    }

    public void SelectPlayerUnit(Unit unit)
    {
        if (curUnit != null)
        {
            ClearToggleBorder(curUnit);
            curUnit.SetUnitToNormalLayerOrder();

            if (curUnit.UnitStatus == UnitStatus.OnBoard)
                curUnit.gameObject.SetActive(false);
        }

        if (unit.UnitType == UnitType.Land && unit.UnitStatus != UnitStatus.WorkInField)
        {
            unit.gameObject.SetActive(true);
            unit.SetUnitToFrontLayerOrder();
        }
        
        curUnit = unit;
        CameraController.instance.MoveCamera(curUnit.transform.position);

        HideOtherLandUnits(curUnit);
        FocusPlayerUnit(curUnit);
    }

    public void SelectPlayerFirstUnit()
    {
        if (playerFaction.Units.Count > 0)
        {
            Unit firstUnit = playerFaction.Units[0];
            SelectPlayerUnit(firstUnit);
            CameraController.instance.MoveCamera(firstUnit.CurPos);
        }
    }

    public void HideOtherLandUnits(Unit thisUnit)
    {
        if (thisUnit.CurHex == null) return;

        foreach (Unit other in thisUnit.CurHex.UnitsInHex)
        {
            if (other.UnitType == UnitType.Land && other.Faction == playerFaction)
            {
                if (other != thisUnit) other.gameObject.SetActive(false);
            }
        }
    }

    // Generate Methods

    private void GenerateEuroShip(Faction fact, int yPos)
    {
        int x = WIDTH - 2; // near right edge of the map
        int y = yPos;
        Hex hex = allHexes[x,y];

        GameObject obj = Instantiate(navelUnitPrefab, hex.Pos, Quaternion.identity, fact.UnitParent);
        NavalUnit ship = obj.GetComponent<NavalUnit>();

        ship.UnitInit(this, fact, navalUnitData[0]); // init Caravel
        ship.SetUpPosition(hex);
        fact.Units.Add(ship); //First Unit of European nations is a ship

        if (fact == playerFaction)
        {
            ClearDarkFogAroundUnit(ship);
            SelectPlayerUnit(ship);
            CameraController.instance.MoveCamera(ship.CurPos);
            ship.Visible = true;
        }
    }

    private void GeneratePassengerUnit(Faction faction, Hex hex, int unitId, bool show, NavalUnit ship)
    {
        GameObject obj = Instantiate(landUnitPrefab, hex.Pos, Quaternion.identity, ship.PassengerParent.transform);
        LandUnit unit = obj.GetComponent<LandUnit>();

        unit.UnitInit(this, faction, landUnitData[unitId]);
        unit.SetUpPosition(hex);
        unit.UnitStatus = UnitStatus.OnBoard;
        obj.SetActive(false);

        faction.Units.Add(unit);
        ship.Passengers.Add(unit);
        unit.TransportShip = ship;
    }

    private void GenerateAllEuropeanExploreUnits()
    {
        for (int i = 0; i < 5; i++)
        {
            NavalUnit firstShip = factions[i].Units[0].gameObject.GetComponent<NavalUnit>();

            GeneratePassengerUnit(factions[i], firstShip.CurHex, 1, false, firstShip); // Veteran Soldiers

            GeneratePassengerUnit(factions[i], firstShip.CurHex, 2, false, firstShip); // Hardy Pioneers
        }
    }

    private void GenerateAllEuropeanShips()
    {
        int interval = HEIGHT / 5;
        int n = HEIGHT;

        List<int> spots = new List<int>();

        for (int i = 0; i < 5; i++)
        {
            n -= Random.Range(5, interval);
            spots.Add(n);
        }

        for (int i = 0; i < 5; i++)
        {
            int index = Random.Range(0, spots.Count);
            int yPosition = spots[index];
            spots.RemoveAt(index);

            GenerateEuroShip(factions[i], yPosition);
        }
    }

    public void CheckToGenerateShipFromEurope(NavalUnit ship)
    {
        int y = Random.Range(0, HEIGHT);

        if (!playerFaction.Units.Contains(ship))
        {
            Debug.Log("New Ship");
            GenerateEuroShip(playerFaction, y);
        }
        else
        {
            Debug.Log("Old Ship");
            MoveEuropeanShip(ship);
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

    public void GenerateTown(Faction faction, Hex curHex)
    {
        GameObject obj = Instantiate(townPrefab, curHex.Pos,Quaternion.identity, faction.TownParent);
        Town town = obj.GetComponent<Town>();

        town.TownInit(this, faction);
        town.CurHex = curHex;
        town.CurPos = town.CurHex.Pos;
        faction.Towns.Add(town);

        curHex.HasTown = true;
        curHex.Town = town;

        curTown = town;
    }

    public void SetupCurrentTown(Town town)
    {
        curTown = town;
        Hex[] aroundHexes = HexCalculator.GetHexAroundToArray(allHexes, curTown.CurHex);

        UiManager.instance.ToggleTownPanel(true);
        UiManager.instance.SetupCurrentTown(curTown.CurHex, aroundHexes);
    }

    private void GenerateAllNativeTowns()
    {
        for (int i = 5; i < factions.Length; i++)
        {
            nativeTownNum = Random.Range(5, 10);

            for (int j = 0; j < nativeTownNum; j++)
            {
                int landEdge = oceanEdgeIndex - 1;

                int x = Random.Range(0, landEdge);
                int y = Random.Range(0, HEIGHT);
                Hex hex = allHexes[x, y];

                if (HexCalculator.CheckIfHexAroundHasTown(allHexes, hex))
                    continue;

                if (hex.HexType == HexType.Mountain || hex.HexType == HexType.Ocean)
                    continue;

                    if (hex.HexType != HexType.Ocean)
                    GenerateTown(factions[i], hex);
            }
        }
    }

    private void GenerateLandUnit(Faction faction, Hex hex, int unitId, bool show)
    {
        GameObject obj = Instantiate(landUnitPrefab, hex.Pos, Quaternion.identity, faction.UnitParent);
        LandUnit unit = obj.GetComponent<LandUnit>();

        unit.UnitInit(this, faction, landUnitData[unitId]);
        unit.SetUpPosition(hex);
        unit.ShowHideSprite(show);

        faction.Units.Add(unit);
    }

    private void GenerateAllNativeUnits()
    {
        for (int i = 5; i < factions.Length; i++)
        {
            foreach (Town town in factions[i].Towns)
            {
                int id = factions[i].IsTropicalNative ? 4 : 3; //Tropical or Plain Indians

                GenerateLandUnit(factions[i], town.CurHex, id, false);
            }
        }
    }

    private void DetermineOcean()
    {
        oceanEdgeIndex = WIDTH - Random.Range(7, 10);
    }

    // Faction Methods

    private void SetUpFaction()
    {
        for (int i = 0; i < factions.Length; i++)
        {
            factions[i].FactionInit(factionData[i]);
        }
    }

    public void SelectPlayerFaction(int index = 0)
    {
        int i = Settings.playerNationId;
        playerFaction = factions[i];
    }

    // Helper Methods

    
    private void ToggleHexText()
    {
        foreach (Hex hex in allHexes)
            hex.ToggleBasicText(!showingText);

        showingText = !showingText;
    }

    private int FindIndexOfCurUnit()
    {
        if (playerFaction.Units.Contains(curUnit))
        {
            for (int i = 0; i < playerFaction.Units.Count; i++)
            {
                if (curUnit == playerFaction.Units[i])
                    return i;
            }
            return -1;
        }
        else
            return -1;
    }

    private void SelectNextPlayerUnit()
    {
        int i = FindIndexOfCurUnit();
        i++;

        if (i >= playerFaction.Units.Count)
            i = 0;

        SelectPlayerUnit(playerFaction.Units[i]);
    }

    // Resource Medthod

    public void AccumulateStockAllTowns()
    {
        for (int i = 0; i < factions.Length; i++)
        {
            for (int j = 0; j < factions[i].Towns.Count; j++)
            {
                factions[i].Towns[j].AccumulateToWarehouse();
            }
        }
    }

    // Ship to Europe

    public void CheckShipToEurope()
    {
        foreach (Unit unit in playerFaction.Units)
        {
            if (unit.UnitType == UnitType.Naval && unit.CurHex != null)
            {
                if (unit.CurHex.X == WIDTH - 1)
                {
                    NavalUnit ship = (NavalUnit)unit;
                    CameraController.instance.MoveCamera(unit.CurPos);

                    EuropeManager.instance.QuestionGoToEurope(ship);
                }
            }
        }
    }

    private void MoveEuropeanShip(NavalUnit ship)
    {
        int x = WIDTH - 2; // near right edge of a map
        int y = Random.Range(0, HEIGHT);
        Hex hex = allHexes[x, y];

        Debug.Log($"Position: {hex.Pos}");

        ship.SetUpPosition(hex);
        ship.gameObject.SetActive(true);
        ship.gameObject.transform.position = ship.CurPos;

        ClearDarkFogAroundUnit(ship);
        SelectPlayerUnit(ship);
        CameraController.instance.MoveCamera(ship.CurPos);
        ship.Visible = true;
    }

    // Combat

    private IEnumerator ShowCausaltyReportWait(float seconds)
    {
        // attacking animation
        yield return new WaitForSeconds(seconds);
        Debug.Log("Show combat Dialog");
        DialogManager.instance.ToggleCombatPanel(true);
    }

    private void CalculateCombatLogic(Unit ourUnit, Unit enemyUnit)
    {
        if (ourUnit.Strength > enemyUnit.Strength)
        {
            winner = ourUnit;
            loser = enemyUnit;
        } 
        else if (ourUnit.Strength < enemyUnit.Strength)
        {
            winner = enemyUnit;
            loser = ourUnit;
        }
        else // draw 
        {
            int n = Random.Range(0,2);

            if (n == 0)
            {
                winner = ourUnit;
                loser = enemyUnit;
            }
            else
            {
                winner = enemyUnit;
                loser = ourUnit;
            }
        }
    }

    public void SetupDeadUnit(Unit deadUnit)
    {
        Faction faction = deadUnit.Faction;
        Hex hex = deadUnit.CurHex;

        if (deadUnit.UnitType == UnitType.Naval)
        {
            NavalUnit ship = (NavalUnit)deadUnit;

            foreach (LandUnit passenger in ship.Passengers)
            {
                if (hex.UnitsInHex.Contains(passenger))
                    hex.UnitsInHex.Remove(passenger);

                faction.Units.Remove(passenger);
                Destroy(passenger.gameObject);
            }
        }

        if (hex.UnitsInHex.Contains(deadUnit))
            hex.UnitsInHex.Remove(deadUnit);

        faction.Units.Remove(deadUnit);
        Destroy(deadUnit.gameObject);
    }

    private IEnumerator ShowUnitDies(float seconds, Unit winner, Unit loser)
    {
        yield return new WaitForSeconds(seconds);
        Debug.Log("Hide Combat Dialog");
        DialogManager.instance.ToggleCombatPanel(false);

        // loser dies animation
        Debug.Log("Animation");

        SetupDeadUnit(loser);

        if (curUnit == loser)
        {
            curUnit = null;
            SelectPlayerFirstUnit();
        }

        DialogManager.instance.ShowCombatResult(winner, loser);
    }

    private void ShowMilitaryResultDialog()
    {
        DialogManager.instance.ShowCombatResult(winner, loser);
    }

    public void StartCombat(Unit attacker, Unit defender)
    {
        DialogManager.instance.CombatAnalysisInit(attacker, defender);
        StartCoroutine(ShowCausaltyReportWait(0.5f));

        CalculateCombatLogic(attacker, defender);

        StartCoroutine(ShowUnitDies(4f, winner, loser));
    }

    // Dialog

    public void ShowFirstKingDialogText()
    {
        DialogManager.instance.ShowFirstKingQuestionDialogText();
    }
}
