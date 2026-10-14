using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UiManager : MonoBehaviour
{
    [SerializeField] private GameObject townPanel;
    public GameObject TownPanel { get { return townPanel; } set { townPanel = value; } }

    [SerializeField] private TerrainSlot centerSlot;

    [SerializeField] private TerrainSlot[] areaSlots;

    [SerializeField] private GameObject unitDragPrefab;

    [SerializeField] private GameObject outsideTownParent;

    [SerializeField] private GameObject yieldIconPrefab;
    public GameObject YieldIconPrefab { get { return yieldIconPrefab; } }

    [SerializeField] private List<GameObject> allUnitDrag;
    public List<GameObject> AllUnitDrag { get { return allUnitDrag; } set { allUnitDrag = value; } }

    [SerializeField] private TerrainSlot currentSlot;

    [SerializeField] private GameObject blockImage;

    [SerializeField] private GameObject professionPanel;

    [SerializeField] private TMP_Text labelQuestionText;

    [SerializeField] private TMP_Text[] btnYieldTexts;
    
    [SerializeField] private Transform foodParent;

    [SerializeField] private TMP_Text foodText;

    [SerializeField] private List<GameObject> foodIconList = new List<GameObject>();

    // Ship

    [SerializeField] private ShipInPort currentShipIcon; // current ship icon a player has selected

    [SerializeField] private StockSlot[] stockSlots;

    [SerializeField] private CargoSlot[] cargoSlots; // ship's cargo
    public CargoSlot[] CargoSlots { get { return cargoSlots; } }

    [SerializeField] private List<GameObject> allShipIcons;
    public List<GameObject> AllShipIcons { get { return allShipIcons; } set { allShipIcons = value; } }

    [SerializeField] private GameObject shipInPortPrefab;
    
    [SerializeField] private GameObject portShipsParent;

    [SerializeField] private GameObject stockDragPrefab; // icon dragged from town's stock
    public GameObject StockDragPrefab { get { return stockDragPrefab; } }

    [SerializeField] private GameObject cargoDragPrefab; // icon dragged from ship's cargo

    // Price
    [SerializeField] TMP_Text moneyText;
    [SerializeField] TMP_Text moneyEuropeText;

    [Header("Europe")]
    [SerializeField] private GameObject europePanel;

    [SerializeField] private GameObject toEuropeShipsParent;

    [SerializeField] private GameObject fromEuropeShipsParent;

    [SerializeField] private List<GameObject> allShipToEuropeIcons;

    [SerializeField] private List<GameObject> allShipFromEuropeIcons;

    [SerializeField] private List<GameObject> allShipInEuropeIcons;
    public List<GameObject> AllShipInEuropeIcons { get { return allShipInEuropeIcons; } }

    [SerializeField] private GameObject europePortShipsParent; 
    
    [SerializeField] private CargoSlot[] cargoSlotsEurope; // ship's cargo hold (Europe)
    public CargoSlot[] CargoSLotEurope { get { return cargoSlotsEurope; } }

    [SerializeField] private StockSlot[] stockSlotsEurope; // Europe's port slot

    [SerializeField] private bool inEurope;
    public bool InEurope { get { return inEurope; } }

    [SerializeField]
    private GameObject shipDragPrefab; // ship icon drag
    public GameObject ShipDragPrefab { get { return shipDragPrefab; } }

    public static UiManager instance;

    void Awake()
    {
        instance = this;
    }

    public void SetUpHexSlots(Hex centerHex, Hex[] aroundHexes)
    {
        centerSlot.HexSlotInit(centerHex);
        // Setup auto Food production on ConterSlot

        if (centerSlot.Hex.YieldID == -1)
        centerSlot.SelectYield(0); // ← first open: set food + Accumulate + icons (once)
        else
        centerSlot.AdjustActualYield(); // ← later opens: only show icons, NO Accumulate

        for (int i = 0; i < areaSlots.Length; i++)
        {
            areaSlots[i].HexSlotInit(aroundHexes[i]);
        }
    }

    public void SetupCurrentTown(Hex curHex, Hex[] aroundHexes) 
    {
        DestoryOldUnitDrag();
        SetUpHexSlots(curHex, aroundHexes);
        SetupUnitDragOutSideTown(curHex);
        SetUpUnitDragWorkingInTerrain();
        SetupYieldInTerrain();
        UpdateTotalFoodIcons();
        SetupStockslots(curHex);
        SetupShipsInPort(curHex);
        SetupShipsCargoSlot(curHex);
        UpdateMoneyText();
    }

    public void SetupParentSpacing(int n, Transform parent, int iconWidth, int parentWidth)
    {
        if (n <= 1) return;

        HorizontalLayoutGroup layout = parent.GetComponent<HorizontalLayoutGroup>();

        int totalWidth = iconWidth * n;
        int excessWidth = totalWidth - parentWidth;

        if (excessWidth <= 0) return;

        int result = excessWidth / (n - 1);
        layout.spacing = -result;
    }

    private void DestoryOldUnitDrag()
    {
        foreach (GameObject obj in allUnitDrag)
        {
            Destroy(obj);
        }
        allUnitDrag.Clear();
    }

    private void HideUnitWorkInTown(Hex hex)
    {
        foreach (Unit unit in hex.UnitsInHex)
        {
            if (unit.UnitStatus == UnitStatus.WorkInField)
                unit.gameObject.SetActive(false);
        }
    }

    private void SetupUnitDragOutSideTown(Hex hex)
    {
        foreach (Unit unit in hex.UnitsInHex)
        {
            if (unit.UnitType == UnitType.Land && unit.UnitStatus == UnitStatus.None)
            {
                GameObject unitObj = Instantiate(unitDragPrefab, outsideTownParent.transform);
                allUnitDrag.Add(unitObj);

                UnitDrag unitDrag = unitObj.GetComponent<UnitDrag>();
                unitDrag.UnitInit((LandUnit)unit);
            }
        }
    }

    public void SetUpUnitDragWorkingInTerrain()
    {
        foreach (TerrainSlot terrainSlot in areaSlots)
        {
            if (terrainSlot.Hex == null) continue;

            if (terrainSlot.Hex.Labor != null)
            {
                GameObject unitObj = Instantiate(unitDragPrefab, terrainSlot.LaborParnet);
                allUnitDrag.Add(unitObj);

                UnitDrag unitDrag = unitObj.GetComponent<UnitDrag>();
                unitDrag.UnitInit((LandUnit)terrainSlot.Hex.Labor);

                unitDrag.IconParent = terrainSlot.LaborParnet;
                unitDrag.WorkAtNewTerrainSlot(terrainSlot);
            }
        }
    }

    private void SetupYieldInTerrain()
    {
        foreach (TerrainSlot terrainSlot in areaSlots)
        {
            if (terrainSlot.Hex == null) continue;

            if (terrainSlot.Hex.Labor != null && terrainSlot.Hex.YieldID != -1)
            terrainSlot.AdjustActualYield();
        }
    }

    public void RemoveAllYieldIcons()
    {
        centerSlot.RemoveYieldIcons();

        foreach (TerrainSlot terrainSlot in areaSlots)
        {
            if (terrainSlot.Hex == null) continue;

            terrainSlot.RemoveYieldIcons();
        }
    }

    public void ToggleTownPanel(bool show)
    {
        if (show == false)
        {
            DestoryOldUnitDrag();
            RemoveAllYieldIcons();
            HideUnitWorkInTown(GameManager.instance.CurTown.CurHex);
            DestroyAllShipIcons();
            DisableAllCargoSlots(cargoSlots);
            DeleteAllCargoDragsInSlots(cargoSlots);
        }
        townPanel.SetActive(show);
    }

    public void ToggleEuropePanel(bool flag)
    {
        if (flag == false)
        {
            DestroyAllShipsEuropeIcons();
            DisableAllCargoSlots(cargoSlotsEurope);
            DeleteAllCargoDragsInSlots(cargoSlotsEurope);

            europePanel.SetActive(false);
            inEurope = false;
        } else
        {
            townPanel.SetActive(false);
            europePanel.SetActive(true);
            inEurope = true;

            SetupShipsToEurope(EuropeManager.instance.ShipsToEurope);
            SetupShipsFromEurope(EuropeManager.instance.ShipsFromEurope);
            SetupShipsInEurope(EuropeManager.instance.ShipsInEurope);
            SetupShipsCargoSlotEurope(EuropeManager.instance.ShipsInEurope);
            SetupStockSlotEurope();
            UpdateMoneyEuropeText();
        }
    }

    public void UpdateLabelQuestionText()
    {
        if (currentSlot == null) return;

        string s = string.Format("Select a profession for {0}", currentSlot.Hex.Labor.UnitName);
        labelQuestionText.text = s;
    }

    public void UpdateButtonTextsYield()
    {
        if (currentSlot == null) return;

        for (int i = 0; i < btnYieldTexts.Length; i++)
        {
            string s = string.Format("{0} {1}", 
            currentSlot.NormalYield[i],
            GameManager.instance.ProductDatas[i].productName);

            btnYieldTexts[i].text = s;
        }
    }

    public void SelectProfession(TerrainSlot slot)
    {
        currentSlot = slot;

        UpdateLabelQuestionText();
        UpdateButtonTextsYield();

        blockImage.SetActive(true);
        professionPanel.SetActive(true);
    }

    public void SelectYield(int i) // Link to Select Profession Button on UI
    {
        Debug.Log($"Select: {i}");

        if (currentSlot != null)
            currentSlot.SelectYield(i);

        blockImage.SetActive(false);
        professionPanel.SetActive(false);

        if (currentSlot.Hex.YieldID == 0)
            UpdateTotalFoodIcons();
    }

    public void UpdateTotalFoodIcons()
    {
        foreach (GameObject obj in foodIconList) Destroy(obj);

        foodIconList.Clear();

        foodText.text = GameManager.instance.CurTown.TotalYieldThisTurn[0].ToString();
        foodText.gameObject.SetActive(true);
        
        for (int i = 0; i < GameManager.instance.CurTown.TotalYieldThisTurn[0]; i++)
        {
            GameObject iconobj = GenerateFoodIcon();
            foodIconList.Add(iconobj);
        }

        SetupParentSpacing(GameManager.instance.CurTown.TotalYieldThisTurn[0], foodParent, 64, 300);
    }

    public GameObject GenerateFoodIcon()
    {
        GameObject foodObj = Instantiate(yieldIconPrefab, foodParent);
        Image iconImg = foodObj.GetComponent<Image>();

        iconImg.sprite = GameManager.instance.ProductDatas[0].icons[0];

        return foodObj;
    }

    public void CheckActiveUIPanel()
    {
        if (townPanel.activeInHierarchy) ToggleTownPanel(false);

        if (europePanel.activeInHierarchy) ToggleEuropePanel(false);
    }

    public void SetupStockslots(Hex hex)
    {
        for (int i = 0; i < stockSlots.Length; i++)
        {
            stockSlots[i].StockInit(i, hex.Town, inEurope);
            //stockSlots[i].StockInit(i, hex.Town, GameManager.instance.ProductDatas[i].icons[0]);
        }
    }

    public void SetupShipsInPort(Hex hex)
    {
        foreach (Unit unit in hex.UnitsInHex)
        {
            if (unit.UnitType == UnitType.Naval)
            {
                GameObject unitObj = Instantiate(shipInPortPrefab, portShipsParent.transform);
                allShipIcons.Add(unitObj);

                ShipInPort shipIcon = unitObj.GetComponent<ShipInPort>();
                shipIcon.UnitInit((NavalUnit)unit, false);
            }
        }
    }

    public void SetupCargoSlot(NavalUnit ship, CargoSlot[] targetSlot)
    {
        for (int i = 0; i < targetSlot.Length; i++)
        {
            if (i < ship.CargoHoldNum)
            {
                targetSlot[i].gameObject.SetActive(true);
                targetSlot[i].SlotInit(ship, i);
            }
            else targetSlot[i].gameObject.SetActive(false);
        }
        UpdateCargoSlots(ship, targetSlot);
    }

    private void SetupShipsCargoSlot(Hex hex)
    {
        foreach (Unit unit in hex.UnitsInHex)
        {
            if (unit.UnitType == UnitType.Naval)
            {
                // first ship
                NavalUnit firstShip = unit.gameObject.GetComponent<NavalUnit>();
                SetupCargoSlot(firstShip, cargoSlots);

                break;
            }
        }
    }

    private void DestroyAllShipIcons()
    {
        foreach (GameObject obj in allShipIcons)
        {
            Destroy(obj);
        }
        allShipIcons.Clear();
    }

    private void DisableAllCargoSlots(CargoSlot[] cargoSlots)
    {
        foreach (CargoSlot slot in cargoSlots)
        {
            slot.gameObject.SetActive(false);
        }
    }

    private void DeleteAllCargoDragsInSlots(CargoSlot[] cargoSlots)
    {
        foreach (CargoSlot slot in cargoSlots)
        {
            if (slot.CargoDrag != null)
            {
                Destroy(slot.CargoDrag.gameObject);
                slot.CargoDrag = null;
            }
        }
    }

    public void UpdateCargoSlots(NavalUnit ship, CargoSlot[] cargoSlots) // Update icon in ship's hold
    {
        DeleteAllCargoDragsInSlots(cargoSlots);

        for (int i = 0; i < ship.CargoList.Count; i++)
        {
            GameObject cargoDragObj = Instantiate(cargoDragPrefab, 
            cargoSlots[i].transform.position,
            Quaternion.identity, cargoSlots[i].transform);

            CargoDrag cargoDrag = cargoDragObj.GetComponent<CargoDrag>();
            cargoSlots[i].CargoDrag = cargoDrag;

            int productId = ship.CargoList[i].ProductID;
            Sprite icon = GameManager.instance.ProductDatas[productId].icons[0];

            cargoDrag.CargoDragInit(ship, icon, i);
        }
    }

    public void ToggleStockDragRayCast(bool flag)
    {
        foreach (StockSlot slot in stockSlots)
        {
            slot.ToggleRayCastStockDrag(flag);
        }
    }

    public void CheckEuropePanel()
    {
        if (europePanel.activeInHierarchy)
            ToggleEuropePanel(false);
        else 
            ToggleEuropePanel(true);
    }

    private void SetupShipsToEurope(List<ShipInTransit> shipToEU)
    {
        foreach (ShipInTransit shipInTransit in shipToEU)
        {
            GameObject unitObj = Instantiate(shipInPortPrefab,toEuropeShipsParent.transform);
            allShipToEuropeIcons.Add(unitObj);

            ShipInPort shipIcon = unitObj.GetComponent<ShipInPort>();
            shipIcon.UnitInit(shipInTransit.Ship, false);
            shipIcon.UpdateTurnText(shipInTransit.TurnLeft);
        }
    }

    public void DestroyShipIcons(List<GameObject> shipList)
    {
        foreach (GameObject obj in shipList)
        {
            Destroy(obj);
        } shipList.Clear();
    }

    private void DestroyAllShipsEuropeIcons()
    {
        DestroyShipIcons(allShipToEuropeIcons);
        DestroyShipIcons(allShipFromEuropeIcons);
        DestroyShipIcons(allShipInEuropeIcons);
    }

    public void SetupShipsInEurope(List<NavalUnit> ships)
    {
        foreach (Unit unit in ships)
        {
            if (unit.UnitType == UnitType.Naval)
            {
                GameObject unitObj = Instantiate(shipInPortPrefab, europePortShipsParent.transform);
                allShipInEuropeIcons.Add(unitObj);

                ShipInPort shipIcon = unitObj.GetComponent<ShipInPort>();
                shipIcon.UnitInit((NavalUnit)unit, true);
            }
        }
    }

    private void SetupShipsCargoSlotEurope(List<NavalUnit> ships)
    {
        foreach (Unit unit in ships)
        {
            if (unit.UnitType == UnitType.Naval)
            {
                // first ship
                NavalUnit firstShip = 
                unit.gameObject.GetComponent<NavalUnit>();
                SetupCargoSlot(firstShip, cargoSlotsEurope);

                break;
            }
        }
    }

    private void SetupShipsFromEurope(List<ShipInTransit> shipToEU)
    {
        foreach (ShipInTransit shipInTransit in shipToEU)
        {
            GameObject unitObj = Instantiate(shipInPortPrefab,
            fromEuropeShipsParent.transform);
            allShipFromEuropeIcons.Add(unitObj);

            ShipInPort shipIcon = unitObj.GetComponent<ShipInPort>();
            shipIcon.UnitInit(shipInTransit.Ship, false);
            shipIcon.UpdateTurnText(shipInTransit.TurnLeft);
        }
    }

    public void UpdateIconsFromEuropeToNewWorld()
    {
        DestroyShipIcons(allShipFromEuropeIcons);
        SetupShipsFromEurope(EuropeManager.instance.ShipsFromEurope);

        DestroyShipIcons(allShipInEuropeIcons);
        SetupShipsInEurope(EuropeManager.instance.ShipsInEurope);
        
        DeleteAllCargoDragsInSlots(cargoSlotsEurope);
        DisableAllCargoSlots(cargoSlotsEurope);
        SetupShipsCargoSlotEurope(EuropeManager.instance.ShipsInEurope);
    }

    public void SetupStockSlotEurope()
    {
        for (int i = 0; i < stockSlotsEurope.Length; i++)
        {
            stockSlotsEurope[i].StockInitEurope(i);
        }
    }

    // Money Medthod

    public void UpdateMoneyText()
    {
        moneyText.text = $"{GameManager.instance.PlayerFaction.Money}";
    }

    public void UpdateMoneyEuropeText()
    {
        moneyEuropeText.text = $"{GameManager.instance.PlayerFaction.Money}";
    }
}